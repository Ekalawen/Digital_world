using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class KeybindingDropdown : MonoBehaviour {

    public Dropdown dropdown;

    public void Start() {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(InputManager.KeybindingType.AZERTY.ToString()));
        options.Add(new Dropdown.OptionData(InputManager.KeybindingType.QWERTY.ToString()));
        options.Add(new Dropdown.OptionData(InputManager.KeybindingType.CONTROLLER.ToString()));
        dropdown.options = options;

        dropdown.value = InputManager.Instance.GetCurrentKeybindingTypeIndice();
        dropdown.onValueChanged.AddListener(LocaleSelected);
    }

    static void LocaleSelected(int index) {
        InputManager.Instance.SetKeybindingTypeByIndice(index);
    }
}