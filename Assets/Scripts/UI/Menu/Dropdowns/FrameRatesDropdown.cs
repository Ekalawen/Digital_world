using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class FrameRatesDropdown : MonoBehaviour {

    public enum FrameRate {
        AUTO,
        TRENTE,
        QUARENTE_CINQ,
        SOIXANTE,
        CENT_VINGT,
        CENT_QUARANTE_QUATRE,
    };

    public static int GetFrameRateInt(FrameRate frameRate) {
        switch (frameRate) {
            case FrameRate.AUTO:
                return -1;
            case FrameRate.TRENTE:
                return 30;
            case FrameRate.QUARENTE_CINQ:
                return 45;
            case FrameRate.SOIXANTE:
                return 60;
            case FrameRate.CENT_VINGT:
                return 120;
            case FrameRate.CENT_QUARANTE_QUATRE:
                return 144;
            default:
                return -1;
        }
    }

    public static int GetFrameRateInt(int frameRateIndice) {
        return GetFrameRateInt((FrameRate)frameRateIndice);
    }

    public Dropdown dropdown;
    public LocalizedString frameRateAutoLocalizedString;
    public LocalizedString frameRate30LocalizedString;
    public LocalizedString frameRate45LocalizedString;
    public LocalizedString frameRate60LocalizedString;
    public LocalizedString frameRate120LocalizedString;
    public LocalizedString frameRate144LocalizedString;

    public void Start() {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(frameRateAutoLocalizedString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(frameRate30LocalizedString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(frameRate45LocalizedString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(frameRate60LocalizedString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(frameRate120LocalizedString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(frameRate144LocalizedString.GetLocalizedString().Result));
        dropdown.options = options;

        dropdown.value = PrefsManager.GetInt(PrefsManager.FRAME_RATES_INDICE, MenuOptions.defaultFrameRateIndice);
        dropdown.onValueChanged.AddListener(FrameRateSelected);
    }

    static void FrameRateSelected(int index) {
        PrefsManager.SetInt(PrefsManager.FRAME_RATES_INDICE, index);
        if(GameManager.IsInGame) {
            GameManager.Instance.timerManager.SetFrameRate();
        }
    }
}