using System;
using System.Collections.Generic;
using UnityEngine;

namespace FZI.SoundManger
{
    [Serializable]
    public class SoundList
    {
        [HideInInspector]
        public string name;
        public AudioClip[] sounds;
    }

    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource _SFX;
        
        [SerializeField] private SoundList[] _sounds;

        private SoundSettings _settings;
        public static SoundSettings Settings => GetInstance()._settings;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
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
            _instance.SetMasterVolumeInternal(settings.MasterVolume);
            return _instance;
        }

        private void PlaySoundInternal(SoundEnum sound)
        {
            SoundList sounds = _sounds[(int)sound];
            if (sounds == null || sounds.sounds.Length == 0)
            {
                Debug.LogWarning($"There is no sounds of type {sound.ToString()}");
                return;
            }
            _SFX.PlayOneShot(sounds.sounds[UnityEngine.Random.Range(0, sounds.sounds.Length)]);
        }
        private void PlayMusicInternal(SoundEnum sound)
        {
            SoundList sounds = _sounds[(int)sound];
            if (sounds == null || sounds.sounds.Length == 0)
            {
                Debug.LogWarning($"There is no tracks of type {sound.ToString()}");
                return;
            }
            _music.clip = sounds.sounds[UnityEngine.Random.Range(0, sounds.sounds.Length)];
            _music.Play();
        }

        private void SetMasterVolumeInternal(float volume)
        {
            _settings.MasterVolume = volume;
            _music.volume = volume * _settings.MusicVolume;
            _SFX.volume = volume * _settings.SfxVolume;
        }
        private void SetMusicVolumeInternal(float volume, bool mute)
        {
            _settings.MusicVolume = volume;
            _settings.MusicMuted = mute;
            _music.mute = mute;
            _music.volume = _settings.MasterVolume * volume;
        }
        private void SetSfxVolumeInternal(float volume, bool mute)
        {
            _settings.SfxVolume = volume;
            _settings.SfxMuted = mute;
            _SFX.mute = mute;
            _SFX.volume = _settings.MasterVolume * volume;
        }

        public static void PlaySound(SoundEnum sound) => GetInstance().PlaySoundInternal(sound);
        public static void PlayMusic(SoundEnum sound) => GetInstance().PlayMusicInternal(sound);
        public static void SetMasterVolume(float volume) => GetInstance().SetMasterVolumeInternal(volume);
        public static void SetMusicVolume(float volume, bool mute) => GetInstance().SetMusicVolumeInternal(volume, mute);
        public static void SetSfxVolume(float volume, bool mute) => GetInstance().SetSfxVolumeInternal(volume, mute);
        public static void SaveSettings()
        {
            PlayerPrefs.SetString("SoundSettings", JsonUtility.ToJson(GetInstance()._settings));
        }
        
        
    }
   
}