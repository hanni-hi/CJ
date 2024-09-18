using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class SettingManager : MonoBehaviour
{
    public static SettingManager instance = null;

    [Header("Audio Setting")]
    public AudioMixer audioMixer;
    public Slider slider;

    [Header("Resolution Setting")]
    public Toggle fullscreenToggle;

        private void Awake()
    {
        if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeVolumeSettings();

        InitializeResolutionSettings();
    }

    private void InitializeVolumeSettings()
    {
        if (slider != null)
        {
            float currentVolume;
            audioMixer.GetFloat("Master",out currentVolume);
            slider.value = Mathf.Pow(10,currentVolume/20);// dB 값을 슬라이더 값으로 변환
            slider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float sliderValue)
    {
        float volume = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat("Master",volume);
    }

    private void InitializeResolutionSettings()
    {
        if(fullscreenToggle !=null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreenMode);
        }
    }

    public void SetFullscreenMode(bool isFullscreen)
    {
        Screen.SetResolution(2560, 1440, isFullscreen);
    }
}
