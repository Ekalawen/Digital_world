using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class KeybindingDropdown : MonoBehaviour {

    public enum KeybindingType {
        AZERTY,
        QWERTY,
    };

    public Dropdown dropdown;

    public void Start() {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(KeybindingType.AZERTY.ToString()));
        options.Add(new Dropdown.OptionData(KeybindingType.QWERTY.ToString()));
        dropdown.options = options;

        dropdown.value = GetKeybindingIndice();
        dropdown.onValueChanged.AddListener(LocaleSelected);
    }

    static void LocaleSelected(int index) {
        PrefsManager.SetInt(PrefsManager.KEYBINDING_INDICE_KEY, index);
    }

    public static int GetKeybindingIndice() {
        return PrefsManager.GetInt(PrefsManager.KEYBINDING_INDICE_KEY, 0);
    }

    public static KeybindingType GetKeybinding() {
        return (KeybindingType)GetKeybindingIndice();
    }
}