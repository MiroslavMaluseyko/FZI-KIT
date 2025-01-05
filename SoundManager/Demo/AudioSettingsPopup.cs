using System;
using FZI.SoundManger;
using UnityEngine;
using UnityEngine.UI;

    public class AudioSettingsPopup : MonoBehaviour
    {
        [SerializeField] private Slider _masterAudio;
        [SerializeField] private Slider _music;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Slider _sfx;
        [SerializeField] private Toggle _sfxToggle;

        private void Awake()
        {
            _masterAudio.onValueChanged.AddListener(SoundManager.SetMasterVolume);
            var settings = SoundManager.Settings;
            _music.onValueChanged.AddListener(val =>
            {
                SoundManager.SetMusicVolume(val, settings.MusicMuted);
            });
            _sfx.onValueChanged.AddListener(val =>
            {
                SoundManager.SetSfxVolume(val, settings.SfxMuted);
            });
            _musicToggle.onValueChanged.AddListener(isOn =>
            {
                SoundManager.SetMusicVolume(settings.MusicVolume, !isOn);
            });
            _sfxToggle.onValueChanged.AddListener(isOn =>
            {
                SoundManager.SetSfxVolume(settings.SfxVolume, !isOn);
            });
        }

        private void OnEnable()
        {
            var settings = SoundManager.Settings;
            _masterAudio.SetValueWithoutNotify(settings.MasterVolume);
            _music.SetValueWithoutNotify(settings.MusicVolume);
            _sfx.SetValueWithoutNotify(settings.SfxVolume);
            _musicToggle.SetIsOnWithoutNotify(!settings.MusicMuted);
            _sfxToggle.SetIsOnWithoutNotify(!settings.SfxMuted);
        }

        private void OnDisable()
        {
            SoundManager.SaveSettings();
        }
    }
