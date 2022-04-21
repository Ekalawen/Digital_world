using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PouvoirDisplay : MonoBehaviour {

    public enum PouvoirType { DASH, PATHFINDER, HACK, RESET, DEFAULT, GRIP_DASH };

    public TMPro.TMP_Text textName;
    public TMPro.TMP_Text textDescription;
    public MessageZoneBindingParameters.Bindings keyBinding;
    public LocalizedString levelToUnlockPouvoirName;
    public LocalizedString levelToUnlockPouvoirInDemoName;
    public Image image;
    public Image bordure;
    public Color bordureColorDash;
    public Color bordureColorPathfinder;
    public Color bordureColorHack;
    public Color bordureColorReset;
    public Color bordureColorDefault;

    public void Initialize(string name, string keyName, string description, Sprite sprite, PouvoirType pouvoirType) {
        InitTexts(name, keyName, description);
        SetBordureColor(pouvoirType);
        SetTooltip();
        this.image.sprite = sprite;
        if (sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }

    protected void InitTexts(string name, string keyName, string description) {
        string keyString = UIHelper.SurroundWithColor(keyName, UIHelper.GREEN);
        string nameString = UIHelper.SurroundWithColor(name, UIHelper.RED);
        textName.text = $"{keyString}\n{nameString}";
        //textName.text = textName.text.Replace("%PouvoirName%", name);
        //textName.text = textName.text.Replace("%ToucheName%", keyName);
        textDescription.text = description;
    }

    private void SetBordureColor(PouvoirType pouvoirType)
    {
        switch (pouvoirType)
        {
            case PouvoirType.DASH:
                bordure.color = bordureColorDash;
                break;
            case PouvoirType.GRIP_DASH:
                bordure.color = bordureColorDash;
                break;
            case PouvoirType.PATHFINDER:
                bordure.color = bordureColorPathfinder;
                break;
            case PouvoirType.HACK:
                bordure.color = bordureColorHack;
                break;
            case PouvoirType.RESET:
                bordure.color = bordureColorReset;
                break;
            case PouvoirType.DEFAULT:
                bordure.color = bordureColorDefault;
                break;
        }
    }

    public static string GetNullName() {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Pouvoirs", "PouvoirNullName").Result;
    }

    public static string GetNullDescription(string levelName) {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Pouvoirs", "PouvoirNullDescription", null, FallbackBehavior.UseProjectSettings, new object[] { levelName }).Result;
    }

    public string GetKeyName() {
        return InputManager.Instance.GetCurrentInputController().GetStringForBinding(keyBinding);
    }

    protected void SetTooltip() {
        TooltipActivator tooltipActivator = GetComponent<TooltipActivator>();
        tooltipActivator.localizedMessage.Arguments = new object[] { GetKeyName() };
    }

    public string GetLevelToUnlockPouvoirName() {
        if (SelectorManager.Instance.isDemo) {
            return levelToUnlockPouvoirInDemoName.GetLocalizedString().Result;
        }
        return levelToUnlockPouvoirName.GetLocalizedString().Result;
    }
}
