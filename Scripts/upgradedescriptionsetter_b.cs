using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine.Localization;
using System.Linq;

public class UpgradeDescriptionSetterB : MonoBehaviour // B
{
    #region Public API

    /// <summary>
    /// Generates a localized, formatted upgrade description with stat changes.
    /// </summary>
    /// <param name="localizedString">The localization string template</param>
    /// <param name="upgradeManager">Reference to player's current upgrade stats</param>
    /// <param name="upgrade">The upgrade being described</param>
    /// <param name="entryKey">Localization table key for the description</param>
    /// <param name="inInventory">True: show total changes. False: show current->new transitions</param>
    /// <returns>Formatted description with rich text tags, or error message if generation fails</returns>
    public static string GetLocalizedDescription(
        LocalizedString localizedString,
        PlayerUpgradesManager upgradeManager,
        UpgradeToPlayer upgrade,
        string entryKey,
        bool inInventory)
    {
        if (!ValidateInputs(upgrade, entryKey))
        {
            return $"[Missing upgrade data for {entryKey}]";
        }

        var descriptionArgs = inInventory
            ? BuildInventoryDescription(upgradeManager, upgrade)
            : BuildSelectionDescription(upgradeManager, upgrade);

        LogDescriptionArgs(descriptionArgs, entryKey);

        return FormatLocalizedString(localizedString, descriptionArgs, entryKey);
    }

    #endregion

    #region Validation

    private static bool ValidateInputs(UpgradeToPlayer upgrade, string entryKey)
    {
        if (upgrade == null)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] UpgradeToPlayer is NULL for key {entryKey}");
            return false;
        }

        if (string.IsNullOrEmpty(entryKey))
        {
            Debug.LogError("[UpgradeDescriptionSetter] Entry key is null or empty");
            return false;
        }

        return true;
    }

    #endregion

    #region Inventory Mode (Total Changes)

    /// <summary>
    /// Builds description showing total accumulated changes from all upgrades.
    /// Example: "+25% damage" or "+5 projectiles"
    /// </summary>
    private static Dictionary<string, object> BuildInventoryDescription(
        PlayerUpgradesManager upgradeManager,
        UpgradeToPlayer upgrade)
    {
        var descriptionArgs = new Dictionary<string, object>();
        var originalValues = upgradeManager.originalValues;

        foreach (FieldInfo upgradeField in GetNumericFields(typeof(UpgradeToPlayer)))
        {
            float upgradeFieldValue = GetFieldValueAsFloat(upgradeField, upgrade);

            // Skip fields that this upgrade doesn't modify
            if (upgradeFieldValue == 0f)
            {
                continue;
            }

            FieldInfo managerField = FindMatchingManagerField(upgradeField);
            if (managerField == null)
            {
                continue;
            }

            float currentValue = GetFieldValueAsFloat(managerField, upgradeManager);
            float originalValue = GetFieldValueAsFloat(managerField, originalValues);
            float totalChange = currentValue - originalValue;

            string formattedChange = FormatInventoryValue(upgradeField.Name, totalChange);
            descriptionArgs[upgradeField.Name] = formattedChange;
        }

        return descriptionArgs;
    }

    /// <summary>
    /// Formats a value for inventory display with + prefix and color.
    /// </summary>
    private static string FormatInventoryValue(string fieldName, float changeValue)
    {
        bool isPercentageField = IsPercentageField(fieldName);

        if (isPercentageField)
        {
            float percentageChange = Mathf.Round(changeValue * 100f);
            string prefix = percentageChange > 0 ? "+" : "";
            return $"<color=yellow>{prefix}{percentageChange}%</color>";
        }
        else
        {
            float roundedChange = Mathf.Round(changeValue * 100f) / 100f;
            string prefix = roundedChange > 0 ? "+" : "";
            return $"<color=yellow>{prefix}{roundedChange}</color>";
        }
    }

    #endregion

    #region Selection Mode (Current -> New Transitions)

    /// <summary>
    /// Builds description showing transition from current value to new value.
    /// Example: "5 -> 8" or "20% -> 45%"
    /// </summary>
    private static Dictionary<string, object> BuildSelectionDescription(
        PlayerUpgradesManager upgradeManager,
        UpgradeToPlayer upgrade)
    {
        var descriptionArgs = new Dictionary<string, object>();
        var originalValues = upgradeManager.originalValues;

        foreach (FieldInfo upgradeField in GetNumericFields(typeof(UpgradeToPlayer)))
        {
            float upgradeFieldValue = GetFieldValueAsFloat(upgradeField, upgrade);

            // Skip fields that this upgrade doesn't modify
            if (upgradeFieldValue == 0f)
            {
                continue;
            }

            FieldInfo managerField = FindMatchingManagerField(upgradeField);
            if (managerField == null)
            {
                continue;
            }

            float currentValue = GetFieldValueAsFloat(managerField, upgradeManager);
            float originalValue = GetFieldValueAsFloat(managerField, originalValues);
            float newValue = currentValue + upgradeFieldValue;

            string formattedTransition = FormatSelectionTransition(
                upgradeField.Name,
                currentValue,
                originalValue,
                upgradeFieldValue
            );

            descriptionArgs[upgradeField.Name] = formattedTransition;
        }

        return descriptionArgs;
    }

    /// <summary>
    /// Formats a stat transition for selection display.
    /// Shows "current -> new" if stat has been previously modified, otherwise just shows "+change".
    /// </summary>
    private static string FormatSelectionTransition(
        string fieldName,
        float currentValue,
        float originalValue,
        float upgradeValue)
    {
        bool isPercentageField = IsPercentageField(fieldName);
        bool hasBeenModifiedBefore = !Mathf.Approximately(currentValue, originalValue);

        string formattedValue = "";

        // Show previous value if this stat has been modified before
        if (hasBeenModifiedBefore)
        {
            float displayCurrentValue = currentValue;

            if (isPercentageField)
            {
                // Upgrades that start at 100% should not be tracked like 120% but like +20% instead or 20% -> 40%.
                if (Mathf.Approximately(originalValue, 1f))
                {
                    displayCurrentValue -= 1f;
                }

                displayCurrentValue = Mathf.Round(displayCurrentValue * 100f);
                formattedValue = $"<color=grey>{displayCurrentValue}%</color> <color=white>-></color> ";
            }
            else
            {
                formattedValue = $"<color=grey>{displayCurrentValue}</color> <color=white>-></color> ";
            }
        }

        // Calculate and display new value
        if (isPercentageField)
        {
            float newPercentageValue = CalculateNewPercentageValue(
                currentValue,
                originalValue,
                upgradeValue,
                hasBeenModifiedBefore
            );

            string prefix = (!hasBeenModifiedBefore && newPercentageValue > 0) ? "+" : "";
            formattedValue += $"<color=yellow>{prefix}{Mathf.Round(newPercentageValue)}%</color>";
        }
        else
        {
            float newValue = currentValue + upgradeValue;
            float roundedNewValue = Mathf.Round(newValue * 100f) / 100f;
            string prefix = (!hasBeenModifiedBefore && roundedNewValue > 0) ? "+" : "";
            formattedValue += $"<color=yellow>{prefix}{roundedNewValue}</color>";
        }

        return formattedValue;
    }

    /// <summary>
    /// Calculates the new percentage value for display, handling base-100% stats correctly.
    /// </summary>
    private static float CalculateNewPercentageValue(
        float currentValue,
        float originalValue,
        float upgradeValue,
        bool hasBeenModifiedBefore)
    {
        if (!hasBeenModifiedBefore)
        {
            return upgradeValue * 100f;
        }

        float displayCurrentValue = currentValue;

        // For stats that start at 100% (1.0), show relative change
        if (Mathf.Approximately(originalValue, 1f))
        {
            displayCurrentValue -= 1f;
        }

        return (displayCurrentValue * 100f) + (upgradeValue * 100f);
    }

    #endregion

    #region Field Reflection Helpers

    /// <summary>
    /// Gets all numeric fields (float, int, bool) from a type.
    /// </summary>
    private static IEnumerable<FieldInfo> GetNumericFields(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(field => IsNumericType(field.FieldType));
    }

    /// <summary>
    /// Finds the corresponding PlayerUpgradesManager field for an UpgradeToPlayer field.
    /// UpgradeToPlayer fields are prefixed with "add" (e.g., "addDmgModifierFlat" -> "dmgModifierFlat").
    /// </summary>
    private static FieldInfo FindMatchingManagerField(FieldInfo upgradeField)
    {
        string managerFieldName = upgradeField.Name.Replace("add", "");

        FieldInfo managerField = typeof(PlayerUpgradesManager)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(field =>
                IsNumericType(field.FieldType) &&
                field.Name.Equals(managerFieldName, StringComparison.OrdinalIgnoreCase)
            );

        if (managerField == null)
        {
            Debug.LogWarning($"[UpgradeDescriptionSetter] No matching manager field found for '{upgradeField.Name}'");
        }

        return managerField;
    }

    /// <summary>
    /// Safely extracts a numeric field value as float, handling int and bool conversions.
    /// </summary>
    private static float GetFieldValueAsFloat(FieldInfo field, object source)
    {
        if (source == null)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Attempted to get field '{field.Name}' from null source");
            return 0f;
        }

        object rawValue = field.GetValue(source);

        if (rawValue == null)
        {
            Debug.LogWarning($"[UpgradeDescriptionSetter] Field '{field.Name}' returned null value");
            return 0f;
        }

        try
        {
            return Convert.ToSingle(rawValue);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Failed to convert field '{field.Name}' value '{rawValue}' to float: {ex.Message}");
            return 0f;
        }
    }

    /// <summary>
    /// Overload for extracting from a dictionary (used for originalValues).
    /// </summary>
    private static float GetFieldValueAsFloat(FieldInfo field, Dictionary<string, object> dictionary)
    {
        if (!dictionary.ContainsKey(field.Name))
        {
            Debug.LogWarning($"[UpgradeDescriptionSetter] Dictionary missing key '{field.Name}', defaulting to 0");
            return 0f;
        }

        object rawValue = dictionary[field.Name];

        try
        {
            return Convert.ToSingle(rawValue);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Failed to convert dictionary value for '{field.Name}': {ex.Message}");
            return 0f;
        }
    }

    /// <summary>
    /// Checks if a field represents a percentage-based stat by name convention.
    /// </summary>
    private static bool IsPercentageField(string fieldName)
    {
        return fieldName.Contains("Mult") ||
               fieldName.Contains("Chance") ||
               fieldName.Contains("Odds") ||
               fieldName.Contains("Percent");
    }

    /// <summary>
    /// Determines if a Type is numeric (int, float, etc.).
    /// </summary>
    public static bool IsNumericType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Localization Formatting

    /// <summary>
    /// Applies the description arguments to the localized string template.
    /// </summary>
    private static string FormatLocalizedString(
        LocalizedString localizedString,
        Dictionary<string, object> descriptionArgs,
        string entryKey)
    {
        try
        {
            return localizedString.GetLocalizedString(descriptionArgs);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UpgradeDescriptionSetter] Smart string format error in '{entryKey}': {ex.Message}");
            return $"[Formatting error: {entryKey}]";
        }
    }

    #endregion

    #region Debug Logging

    /// <summary>
    /// Logs all description arguments for debugging purposes.
    /// </summary>
    private static void LogDescriptionArgs(Dictionary<string, object> args, string entryKey)
    {
        Debug.Log($"[UpgradeDescriptionSetter] Generated description for '{entryKey}':");
        foreach (var kvp in args)
        {
            Debug.Log($"  {kvp.Key} = {kvp.Value}");
        }
    }

    #endregion
}