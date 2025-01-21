using System;
using System.Collections.Generic;
using FZI.Tools;
using UnityEngine;

namespace FZI.SoundManger
{
    [Serializable]
    public class SoundListItem
    {
        public float volumeMultiplier = 1f;
        public AudioClip clip;
    }
    
    [Serializable]
    public class SoundList
    {
        [HideInInspector]
        public string name;
        public SoundListItem[] sounds;
    }

    [Serializable]
    public class AmbientItem
    {
        public AudioSource audioSource;
        public SoundListItem sound;
        public float currentVolume = 1f;
    }

    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource _SFX;
        [SerializeField] private Transform _ambientsParent;
        [SerializeField] private AudioSource _ambientPrefab;
        
        [SerializeField] private SoundList[] _sounds;
        
        private SoundSettings _settings;
        public static SoundSettings Settings => GetInstance()._settings;
        
        private Dictionary<SoundEnum, AmbientItem> _ambientItems;
        private FZIPool<AudioSource> _ambientPool;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }

            _ambientPool = new FZIPool<AudioSource>(
                () => Instantiate(_ambientPrefab, _ambientsParent),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                obj => Destroy(obj.gameObject)
                );
            _ambientItems = new Dictionary<SoundEnum, AmbientItem>();
        }

        private void OnValidate()
        {
            string[] names = Enum.GetNames(typeof(SoundEnum));
            Array.Resize(ref _sounds, names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                _sounds[i].name = names[i];
            }
        }
        private void OnApplicationQuit()
        {
            SaveSettings();
        }

        public static SoundManager GetInstance()
        {
            if (_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<SoundManager>();
            DontDestroyOnLoad(_instance.gameObject);
            
            _instance._settings = JsonUtility.FromJson<SoundSettings>(PlayerPrefs.GetString("SoundSettings", ""));
            if (_instance._settings == null)
            {
                _instance._settings = new SoundSettings();
                PlayerPrefs.SetString("SoundSettings", JsonUtility.ToJson(_instance._settings));
            }
            
            SoundSettings settings = _instance._settings;
            _instance.SetMusicVolumeInternal(settings.MusicVolume, settings.MusicMuted);
            _instance.SetSfxVolumeInternal(settings.SfxVolume, settings.SfxMuted);
            _instance.SetAmbientsVolumeInternal(settings.AmbientsVolume);
            _instance.SetMasterVolumeInternal(settings.MasterVolume);
            return _instance;
        }

        private void PlaySoundInternal(SoundEnum sound)
        {
            SoundList sounds = _sounds[(int)sound-1];
            if (sounds == null || sounds.sounds.Length == 0)
            {
                Debug.LogWarning($"There is no sounds of type {sound.ToString()}");
                return;
            }
            SoundListItem item = sounds.sounds[UnityEngine.Random.Range(0, sounds.sounds.Length)];
            _SFX.volume = _settings.SfxVolume * _settings.MasterVolume * item.volumeMultiplier;
            _SFX.PlayOneShot(item.clip);
        }
        private void PlayMusicInternal(SoundEnum sound)
        {
            SoundList sounds = _sounds[(int)sound-1];
            if (sounds == null || sounds.sounds.Length == 0)
            {
                Debug.LogWarning($"There is no tracks of type {sound.ToString()}");
                return;
            }

            SoundListItem item = sounds.sounds[UnityEngine.Random.Range(0, sounds.sounds.Length)];
            _music.clip = item.clip;
            _music.volume = _settings.MusicVolume * _settings.MasterVolume * item.volumeMultiplier;
            _music.Play();
        }

        private void PlayAmbientInternal(SoundEnum sound, float volume)
        {
            SoundList sounds = _sounds[(int)sound-1];
            if (sounds == null || sounds.sounds.Length == 0)
            {
                Debug.LogWarning($"There is no tracks of type {sound.ToString()}");
                return;
            }

            if (_ambientItems.ContainsKey(sound))
            {
                Debug.LogWarning($"This type of ambient is already playing: {sound.ToString()}");
                return;
            }
            AudioSource ambient = _ambientPool.Get();
            
            
            SoundListItem item = sounds.sounds[UnityEngine.Random.Range(0, sounds.sounds.Length)];

            ambient.clip = item.clip;
            ambient.volume = _settings.MasterVolume * item.volumeMultiplier * volume;
            
            _ambientItems.Add(sound, new AmbientItem()
            {
                audioSource = ambient,
                sound = item,
                currentVolume = volume
            });
            ambient.Play();
        }
        private void StopAmbientInternal(SoundEnum sound)
        {
            if (!_ambientItems.TryGetValue(sound, out var item))
            {
                Debug.LogWarning($"There is no ambient of this type playing: {sound.ToString()}");
                return;
            }

            AudioSource ambient = item.audioSource;
            ambient.Stop();
            _ambientPool.Release(ambient);
            _ambientItems.Remove(sound);
        }
        
        private void SetMasterVolumeInternal(float volume)
        {
            _settings.MasterVolume = volume;
            SetMusicVolumeInternal(_settings.MusicVolume, _settings.MusicMuted);
            SetSfxVolumeInternal(_settings.SfxVolume, _settings.SfxMuted);
            SetAmbientsVolumeInternal(_settings.AmbientsVolume);
        }
        private void SetMusicVolumeInternal(float volume, bool mute)
        {
            _settings.MusicVolume = volume;
            _settings.MusicMuted = mute;
            _music.mute = mute;
            _music.volume = _settings.MasterVolume * _settings.MusicVolume;
        }
        private void SetSfxVolumeInternal(float volume, bool mute)
        {
            _settings.SfxVolume = volume;
            _settings.SfxMuted = mute;
            _SFX.mute = mute;
            _SFX.volume = _settings.MasterVolume * _settings.SfxVolume;
        }
        private void SetAmbientVolumeInternal(SoundEnum sound, float volume)
        {
            if (!_ambientItems.ContainsKey(sound))
            {
                Debug.LogWarning($"There is no ambient of this type playing: {sound.ToString()}");
                return;
            }
            var item = _ambientItems[sound];
            item.currentVolume = volume;
            _ambientItems[sound].audioSource.volume = 
                _settings.MasterVolume * item.currentVolume * item.sound.volumeMultiplier * _settings.AmbientsVolume;
        }
        
        private void SetAmbientsVolumeInternal(float volume)
        {
            _settings.AmbientsVolume = volume;
            foreach (var ambientItem in _ambientItems)
            {
                AmbientItem ambient = ambientItem.Value;
                ambient.audioSource.volume = _settings.MasterVolume * ambient.currentVolume * ambient.sound.volumeMultiplier * _settings.AmbientsVolume;
            }
        }

        public static void PlaySound(SoundEnum sound) => GetInstance().PlaySoundInternal(sound);
        public static void PlayMusic(SoundEnum sound) => GetInstance().PlayMusicInternal(sound);
        public static void PlayAmbient(SoundEnum sound, float volume = 1f) => GetInstance().PlayAmbientInternal(sound, volume);
        public static void StopAmbient(SoundEnum sound) => GetInstance().StopAmbientInternal(sound);
        public static void SetMasterVolume(float volume) => GetInstance().SetMasterVolumeInternal(volume);
        public static void SetMusicVolume(float volume, bool mute) => GetInstance().SetMusicVolumeInternal(volume, mute);
        public static void SetSfxVolume(float volume, bool mute) => GetInstance().SetSfxVolumeInternal(volume, mute);
        public static void SetAmbientVolume(SoundEnum soundEnum, float volume) => GetInstance().SetAmbientVolumeInternal(soundEnum, volume);
        public static void SetAmbientsVolume(float volume) => GetInstance().SetAmbientsVolumeInternal(volume);
        
        public static void SaveSettings()
        {
            PlayerPrefs.SetString("SoundSettings", JsonUtility.ToJson(GetInstance()._settings));
        }
        
        
    }
   
}