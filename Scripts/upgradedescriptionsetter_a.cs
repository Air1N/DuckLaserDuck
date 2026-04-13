using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Generates localized upgrade descriptions with dynamic stat values.
/// Supports two display modes: inventory (showing total accumulated change) 
/// and selection (showing current -> new value transitions).
/// </summary>
public static class UpgradeDescriptionSetterA // A
{
    #region Constants

    private static readonly string[] PercentageFieldIdentifiers = { "Mult", "Chance", "Odds", "Percent" };
    private const string LocalizationTableName = "StringLocalizationTable";

    private const string ColorYellow = "<color=yellow>";
    private const string ColorGrey = "<color=grey>";
    private const string ColorWhite = "<color=white>";
    private const string ColorClose = "</color>";
    private const string TransitionArrow = " -> ";

    #endregion

    #region Public API

    /// <summary>
    /// Generates a formatted, localized description for an upgrade.
    /// </summary>
    /// <param name="localizedString">The LocalizedString to format with arguments</param>
    /// <param name="upgradeManager">Player's upgrade manager containing current stats</param>
    /// <param name="upgrade">The upgrade being described</param>
    /// <param name="localizationKey">Key for the localization table entry</param>
    /// <param name="isInventoryDisplay">True: show total change from original. False: show current -> new transition</param>
    /// <returns>Formatted rich-text description string</returns>
    public static string GetLocalizedDescription(
        LocalizedString localizedString,
        PlayerUpgradesManager upgradeManager,
        UpgradeToPlayer upgrade,
        string localizationKey,
        bool isInventoryDisplay)
    {
        if (!ValidateUpgrade(upgrade, localizationKey, out string upgradeError))
            return upgradeError;

        if (!ValidateUpgradeManager(upgradeManager, localizationKey, out string managerError))
            return managerError;

        if (!TryGetLocalizationEntry(localizationKey, out StringTableEntry entry))
            return $"[Missing key: {localizationKey}]";

        Dictionary<string, object> formattedArguments = isInventoryDisplay
            ? BuildInventoryArguments(upgradeManager, upgrade)
            : BuildSelectionArguments(upgradeManager, upgrade);

        return FormatWithArguments(localizedString, formattedArguments, localizationKey);
    }

    #endregion

    #region Input Validation

    private static bool ValidateUpgrade(UpgradeToPlayer upgrade, string localizationKey, out string errorMessage)
    {
        if (upgrade == null)
        {
            errorMessage = $"[Missing upgrade data for {localizationKey}]";
            Debug.LogError($"[UpgradeDescriptionSetter] UpgradeToPlayer is NULL for key '{localizationKey}'");
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static bool ValidateUpgradeManager(PlayerUpgradesManager manager, string localizationKey, out string errorMessage)
    {
        if (manager == null)
        {
            errorMessage = $"[Missing manager for {localizationKey}]";
            Debug.LogError($"[UpgradeDescriptionSetter] PlayerUpgradesManager is NULL for key '{localizationKey}'");
            return false;
        }

        if (manager.originalValues == null || manager.originalValues.Count == 0)
        {
            errorMessage = $"[Original values not initialized for {localizationKey}]";
            Debug.LogError($"[UpgradeDescriptionSetter] originalValues dictionary is empty for key '{localizationKey}'");
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static bool TryGetLocalizationEntry(string key, out StringTableEntry entry)
    {
        entry = null;

        var table = LocalizationSettings.StringDatabase.GetTable(LocalizationTableName, LocalizationSettings.SelectedLocale);
        if (table == null)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Localization table '{LocalizationTableName}' not found");
            return false;
        }

        entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Entry not found for key '{key}' in table '{LocalizationTableName}'");
            return false;
        }

        return true;
    }

    #endregion

    #region Inventory Display Mode (Total Change From Original)

    private static Dictionary<string, object> BuildInventoryArguments(
        PlayerUpgradesManager manager,
        UpgradeToPlayer upgrade)
    {
        var arguments = new Dictionary<string, object>();

        foreach (FieldInfo upgradeField in GetNumericUpgradeFields())
        {
            if (!TryGetUpgradeFieldValue(upgradeField, upgrade, out float upgradeAmount))
                continue;

            if (Mathf.Approximately(upgradeAmount, 0f))
                continue;

            if (!TryFindMatchingManagerField(upgradeField, out FieldInfo managerField))
                continue;

            if (!TryGetManagerFieldValues(managerField, manager, out float currentValue, out float originalValue))
                continue;

            float totalChange = currentValue - originalValue;
            string formatted = FormatInventoryStatChange(upgradeField.Name, totalChange);
            arguments[upgradeField.Name] = formatted;
        }

        LogArgumentsDebug(arguments, "Inventory");
        return arguments;
    }

    private static string FormatInventoryStatChange(string fieldName, float totalChange)
    {
        string signPrefix = totalChange >= 0f ? "+" : "";

        if (IsPercentageField(fieldName))
        {
            int percentValue = Mathf.RoundToInt(totalChange * 100f);
            return WrapYellow($"{signPrefix}{percentValue}%");
        }
        else
        {
            float roundedValue = RoundToTwoDecimals(totalChange);
            return WrapYellow($"{signPrefix}{roundedValue}");
        }
    }

    #endregion

    #region Selection Display Mode (Current -> New Transition)

    private static Dictionary<string, object> BuildSelectionArguments(
        PlayerUpgradesManager manager,
        UpgradeToPlayer upgrade)
    {
        var arguments = new Dictionary<string, object>();

        foreach (FieldInfo upgradeField in GetNumericUpgradeFields())
        {
            if (!TryGetUpgradeFieldValue(upgradeField, upgrade, out float upgradeAmount))
                continue;

            if (Mathf.Approximately(upgradeAmount, 0f))
                continue;

            if (!TryFindMatchingManagerField(upgradeField, out FieldInfo managerField))
                continue;

            if (!TryGetManagerFieldValues(managerField, manager, out float currentValue, out float originalValue))
                continue;

            bool hasExistingModifications = !Mathf.Approximately(currentValue, originalValue);
            string formatted = FormatSelectionStatChange(
                upgradeField.Name,
                currentValue,
                upgradeAmount,
                originalValue,
                hasExistingModifications);

            arguments[upgradeField.Name] = formatted;
        }

        LogArgumentsDebug(arguments, "Selection");
        return arguments;
    }

    private static string FormatSelectionStatChange(
        string fieldName,
        float currentValue,
        float upgradeAmount,
        float originalValue,
        bool hasExistingModifications)
    {
        if (IsPercentageField(fieldName))
        {
            return FormatPercentageStatChange(currentValue, upgradeAmount, originalValue, hasExistingModifications);
        }
        else
        {
            return FormatFlatStatChange(currentValue, upgradeAmount, hasExistingModifications);
        }
    }

    private static string FormatPercentageStatChange(
        float currentValue,
        float upgradeAmount,
        float originalValue,
        bool hasExistingModifications)
    {
        // Multipliers starting at 1.0 (100%) display as relative bonuses.
        // Example: 1.0 -> 1.2 shows as "0% -> 20%" rather than "100% -> 120%"
        // This makes it clearer that the player gained +20% bonus damage, etc.
        bool isBaseOneMultiplier = Mathf.Approximately(originalValue, 1f);

        float displayCurrent = currentValue;
        if (isBaseOneMultiplier)
        {
            displayCurrent -= 1f;
        }

        float currentAsPercent = Mathf.Round(displayCurrent * 100f);
        float upgradeAsPercent = Mathf.Round(upgradeAmount * 100f);
        float newValueAsPercent = currentAsPercent + upgradeAsPercent;

        if (hasExistingModifications)
        {
            return BuildTransitionString($"{currentAsPercent}%", $"{newValueAsPercent}%");
        }
        else
        {
            string signPrefix = upgradeAsPercent >= 0f ? "+" : "";
            return WrapYellow($"{signPrefix}{upgradeAsPercent}%");
        }
    }

    private static string FormatFlatStatChange(
        float currentValue,
        float upgradeAmount,
        bool hasExistingModifications)
    {
        float newValue = RoundToTwoDecimals(currentValue + upgradeAmount);

        if (hasExistingModifications)
        {
            float displayCurrent = RoundToTwoDecimals(currentValue);
            return BuildTransitionString(displayCurrent.ToString(), newValue.ToString());
        }
        else
        {
            string signPrefix = newValue >= 0f ? "+" : "";
            return WrapYellow($"{signPrefix}{newValue}");
        }
    }

    private static string BuildTransitionString(string previousValue, string newValue)
    {
        string greyPrevious = WrapGrey(previousValue);
        string whiteArrow = WrapWhite(TransitionArrow);
        string yellowNew = WrapYellow(newValue);
        return $"{greyPrevious}{whiteArrow}{yellowNew}";
    }

    #endregion

    #region Reflection Helpers

    private static IEnumerable<FieldInfo> GetNumericUpgradeFields()
    {
        FieldInfo[] allFields = typeof(UpgradeToPlayer).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in allFields)
        {
            if (IsNumericType(field.FieldType))
            {
                yield return field;
            }
        }
    }

    private static bool TryFindMatchingManagerField(FieldInfo upgradeField, out FieldInfo managerField)
    {
        managerField = null;

        // UpgradeToPlayer fields use "add" prefix: "addDmgModifierFlat"
        // PlayerUpgradesManager fields have no prefix: "dmgModifierFlat"
        string expectedManagerFieldName = upgradeField.Name
            .Replace("add", "")
            .ToLowerInvariant();

        FieldInfo[] managerFields = typeof(PlayerUpgradesManager).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo candidate in managerFields)
        {
            if (!IsNumericType(candidate.FieldType))
                continue;

            if (candidate.Name.ToLowerInvariant() == expectedManagerFieldName)
            {
                managerField = candidate;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetUpgradeFieldValue(FieldInfo field, UpgradeToPlayer upgrade, out float value)
    {
        value = 0f;

        if (field == null || upgrade == null)
        {
            Debug.LogWarning("[UpgradeDescriptionSetter] Null field or upgrade in TryGetUpgradeFieldValue");
            return false;
        }

        try
        {
            object rawValue = field.GetValue(upgrade);
            value = Convert.ToSingle(rawValue);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Failed to read field '{field.Name}': {e.Message}");
            return false;
        }
    }

    private static bool TryGetManagerFieldValues(
        FieldInfo managerField,
        PlayerUpgradesManager manager,
        out float currentValue,
        out float originalValue)
    {
        currentValue = 0f;
        originalValue = 0f;

        if (managerField == null || manager == null)
        {
            Debug.LogWarning("[UpgradeDescriptionSetter] Null managerField or manager in TryGetManagerFieldValues");
            return false;
        }

        try
        {
            object rawCurrent = managerField.GetValue(manager);
            currentValue = Convert.ToSingle(rawCurrent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Failed to read current value for '{managerField.Name}': {e.Message}");
            return false;
        }

        if (!manager.originalValues.TryGetValue(managerField.Name, out object rawOriginal))
        {
            Debug.LogWarning($"[UpgradeDescriptionSetter] Original value not found for '{managerField.Name}'");
            return false;
        }

        try
        {
            originalValue = Convert.ToSingle(rawOriginal);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Failed to parse original value for '{managerField.Name}': {e.Message}");
            return false;
        }
    }

    private static bool IsNumericType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or TypeCode.SByte or
            TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or
            TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => true,
            _ => false
        };
    }

    #endregion

    #region Formatting Utilities

    private static bool IsPercentageField(string fieldName)
    {
        foreach (string identifier in PercentageFieldIdentifiers)
        {
            if (fieldName.IndexOf(identifier, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    private static float RoundToTwoDecimals(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    private static string WrapYellow(string text) => $"{ColorYellow}{text}{ColorClose}";
    private static string WrapGrey(string text) => $"{ColorGrey}{text}{ColorClose}";
    private static string WrapWhite(string text) => $"{ColorWhite}{text}{ColorClose}";

    private static string FormatWithArguments(
        LocalizedString localizedString,
        Dictionary<string, object> arguments,
        string localizationKey)
    {
        if (localizedString == null)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] LocalizedString is null for key '{localizationKey}'");
            return $"[Null LocalizedString: {localizationKey}]";
        }

        try
        {
            return localizedString.GetLocalizedString(arguments);
        }
        catch (FormatException fe)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Smart string placeholder mismatch in '{localizationKey}': {fe.Message}");
            return $"[Format error: {localizationKey}]";
        }
        catch (Exception e)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Unexpected error formatting '{localizationKey}': {e.Message}");
            return $"[Error: {localizationKey}]";
        }
    }

    #endregion

    #region Debug Logging

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogArgumentsDebug(Dictionary<string, object> arguments, string displayMode)
    {
        Debug.Log($"======= {displayMode} Display Arguments =======");
        foreach (KeyValuePair<string, object> kvp in arguments)
        {
            Debug.Log($"  {kvp.Key} = {kvp.Value}");
        }
    }

    #endregion
}