using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private bool needsToSetToPreferences = false;
    private bool needsToFindSliders = true;

    private void Start()
    {
        needsToSetToPreferences = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        needsToFindSliders = true;
    }


    private void Update()
    {
        if (needsToFindSliders && masterSlider && musicSlider && sfxSlider)
        {
            needsToFindSliders = false;
            needsToSetToPreferences = true;
        }

        if (needsToFindSliders)
        {
            GameObject masterSliderObj = GameObject.Find("VolumeSlider");
            GameObject musicSliderObj = GameObject.Find("MusicVolumeSlider");
            GameObject sfxSliderObj = GameObject.Find("EffectsVolumeSlider");

            if (masterSliderObj && musicSliderObj && sfxSliderObj)
            {
                masterSliderObj.TryGetComponent(out masterSlider);
                musicSliderObj.TryGetComponent(out musicSlider);
                sfxSliderObj.TryGetComponent(out sfxSlider);
            }
        }
        else
        {
            if (needsToSetToPreferences)
            {
                float lastMasterVolume = PlayerPrefs.GetFloat("MasterVolume");
                float lastMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
                float lastSFXVolume = PlayerPrefs.GetFloat("SFXVolume");

                if (lastMasterVolume == 0f) lastMasterVolume = 50f;
                if (lastMusicVolume == 0f) lastMusicVolume = 50f;
                if (lastSFXVolume == 0f) lastSFXVolume = 50f;

                masterSlider.value = lastMasterVolume;
                musicSlider.value = lastMusicVolume;
                sfxSlider.value = lastSFXVolume;

                needsToSetToPreferences = false;
            }

            SetMasterVolume(masterSlider.value);
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);
        }
    }

    private float PercentToDB(float percentIn)
    {
        if (percentIn <= 1) return -80f;

        float dbValue = 10 * Mathf.Log10(percentIn / 100f);
        return dbValue;
    }

    private float DBToPercent(float dbIn)
    {
        return 100 * Mathf.Pow(10, dbIn / 10f);
    }

    public void SetMasterVolume(float percentVolume)
    {
        if (!float.IsFinite(percentVolume)) return;

        float dbVolume = PercentToDB(percentVolume);

        audioMixer.SetFloat("MasterVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", percentVolume);
    }

    public void SetMusicVolume(float percentVolume)
    {
        if (!float.IsFinite(percentVolume)) return;

        float dbVolume = PercentToDB(percentVolume);

        audioMixer.SetFloat("MusicVolume", dbVolume);
        PlayerPrefs.SetFloat("MusicVolume", percentVolume);
    }

    public void SetSFXVolume(float percentVolume)
    {
        if (!float.IsFinite(percentVolume)) return;

        float dbVolume = PercentToDB(percentVolume);

        audioMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat("SFXVolume", percentVolume);
    }

    public void SetAudioDevice(int deviceIndex)
    {

    }
}
