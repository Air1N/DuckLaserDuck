using System;
using System.Collections;
using System.Collections.Generic;
using ArabicSupport;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class FixArabicText : MonoBehaviour
{
    public string text;
    public string fixedText;
    public bool showTashkeel = true;
    public bool useHinduNumbers = false;

    void Start()
    {
        FixText(null);
        LocalizationSettings.SelectedLocaleChanged += FixText;
    }

    public void FixText(Locale newLocale)
    {
        if (LocalizationSettings.SelectedLocale.Identifier.Code != "ar") return;

        TextMeshProUGUI textMesh = gameObject.GetComponent<TextMeshProUGUI>();
        text = textMesh.text;

        Debug.Log("Fixing text: " + text);

        fixedText = ArabicFixer.Fix(text, showTashkeel, useHinduNumbers);
        textMesh.text = fixedText;
    }

    public void FixTextFromString(string textString)
    {
        if (LocalizationSettings.SelectedLocale.Identifier.Code != "ar") return;

        TextMeshProUGUI textMesh = gameObject.GetComponent<TextMeshProUGUI>();

        fixedText = ArabicFixer.Fix(textString, showTashkeel, useHinduNumbers);
        textMesh.text = fixedText;
    }

    void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= FixText;
    }
}
