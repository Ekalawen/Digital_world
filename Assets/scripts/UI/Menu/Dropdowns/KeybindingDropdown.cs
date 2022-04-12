﻿using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class KeybindingDropdown : MonoBehaviour {

    public Dropdown dropdown;
    public LocalizedString xboxControllerString;
    public LocalizedString switchControllerString;
    public LocalizedString playstationControllerString;

    public void Start() {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(InputManager.KeybindingType.AZERTY.ToString()));
        options.Add(new Dropdown.OptionData(InputManager.KeybindingType.QWERTY.ToString()));
        options.Add(new Dropdown.OptionData(xboxControllerString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(switchControllerString.GetLocalizedString().Result));
        options.Add(new Dropdown.OptionData(playstationControllerString.GetLocalizedString().Result));
        dropdown.options = options;

        dropdown.value = InputManager.Instance.GetCurrentKeybindingTypeIndice();
        dropdown.onValueChanged.AddListener(KeybindingSelected);
    }

    static void KeybindingSelected(int index) {
        InputManager.Instance.SetKeybindingTypeByIndice(index);
    }

    public static InputManager.KeybindingType GetDefaultKeybinding() {
        if (SteamManager.Initialized) {
            string steamLocale = SteamApps.GetCurrentGameLanguage();
            switch (steamLocale) {
                case "french":
                    return InputManager.KeybindingType.AZERTY;
                case "english":
                    return InputManager.KeybindingType.QWERTY;
                default:
                    return InputManager.KeybindingType.QWERTY;
            }
        }
        return InputManager.KeybindingType.QWERTY;
    }
}