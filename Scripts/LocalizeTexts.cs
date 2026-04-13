using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LocalizeTexts : MonoBehaviour
{
    public string languageSelected = "English";

    private string lastLanguageSelected = "English";
    public int dropdownValue = 0;

    public bool needsToFindLanguageDropdown = true;
    private TMP_Dropdown languageDropdown;

    private int tick = 0;

    public void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void TempCycleLanguages()
    {
        tick++;
        if (tick % 100 == 0)
        {
            languageDropdown.value = (languageDropdown.value + 1) % 12;
        }
    }

    // private void FixedUpdate()
    // {
    //     TempCycleLanguages();
    // }

    public void Update()
    {
        if (needsToFindLanguageDropdown)
        {
            languageDropdown = FindAnyObjectByType<TMP_Dropdown>();

            if (languageDropdown)
            {
                languageSelected = PlayerPrefs.GetString("Language");

                languageDropdown.value = PlayerPrefs.GetInt("LanguageDropdownValue");
                needsToFindLanguageDropdown = false;
            }
        }
        else
        {
            languageSelected = languageDropdown.captionText.text;
            dropdownValue = languageDropdown.value;

            if (languageSelected != lastLanguageSelected)
            {
                SetLanguage();
            }
            lastLanguageSelected = languageSelected;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        needsToFindLanguageDropdown = true;
    }

    public void SetLanguage()
    {
        PlayerPrefs.SetString("Language", languageSelected);
        PlayerPrefs.SetInt("LanguageDropdownValue", dropdownValue);

        switch (languageSelected)
        {
            case "ﺔﻴﺑﺮﻌﻟا":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                break;
            case "中文":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                break;
            case "english":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[2];
                break;
            case "français":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[3];
                break;
            case "deutsch":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[4];
                break;
            case "indonesia":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[5];
                break;
            case "italiano":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[6];
                break;
            case "日本語":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[7];
                break;
            case "한국어":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[8];
                break;
            case "português":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[9];
                break;
            case "русский":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[10];
                break;
            case "español":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[11];
                break;
        }
    }
}
