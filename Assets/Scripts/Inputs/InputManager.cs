using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputManager : MonoBehaviour {

    public enum KeybindingType {
        AZERTY,
        QWERTY,
        CONTROLLER,
    };

    static InputManager _instance;
    public static InputManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<InputManager>()); } }

    public KeybindingType currentKeybindingType;

    protected AxisButton controllerLeftTrigger;
    protected AxisButton controllerRightTrigger;
    protected bool wasAControllerPlugin;

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "InputManager";
        DontDestroyOnLoad(this);
        currentKeybindingType = GetKeybindingType();
        controllerRightTrigger = new AxisButton("Jump_CONTROLLER");
        controllerLeftTrigger = new AxisButton("Shift_CONTROLLER");
    }

    public void Update() {
        controllerLeftTrigger.Update();
        controllerRightTrigger.Update();
        DetectPlugUnplugController();
    }

    protected int GetKeybindingTypeIndice() {
        return PrefsManager.GetInt(PrefsManager.KEYBINDING_INDICE_KEY, 0);
    }

    protected KeybindingType GetKeybindingType() {
        return (KeybindingType)GetKeybindingTypeIndice();
    }

    public KeybindingType GetCurrentKeybindingType() {
        return currentKeybindingType;
    }

    public int GetCurrentKeybindingTypeIndice() {
        return (int)currentKeybindingType;
    }

    public void SetKeybindingType(KeybindingType keybindingType) {
        int index = (int)keybindingType;
        SetKeybindingTypeByIndice(index);
    }

    public void SetKeybindingTypeByIndice(int keybindingIndice) {
        int precedentKeybindingIndice = PrefsManager.GetInt(PrefsManager.KEYBINDING_PRECEDENT_INDICE_KEY, (int)GetDefaultKeybindingType());
        if (precedentKeybindingIndice != keybindingIndice) {
            PrefsManager.SetInt(PrefsManager.KEYBINDING_PRECEDENT_INDICE_KEY, (int)currentKeybindingType);
        }

        PrefsManager.SetInt(PrefsManager.KEYBINDING_INDICE_KEY, keybindingIndice);
        currentKeybindingType = (KeybindingType)keybindingIndice;

        KeybindingDropdown keybindingDropdown = FindObjectOfType<KeybindingDropdown>();
        if(keybindingDropdown != null) {
            keybindingDropdown.dropdown.SetValueWithoutNotify(keybindingIndice);
        }
    }

    public KeybindingType GetDefaultKeybindingType() {
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0]) { // 0 is French
            return KeybindingType.AZERTY;
        }
        return KeybindingType.QWERTY;
    }

    public Vector2 GetMouseMouvement() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            float xRot = Input.GetAxis("Mouse_X");
            float yRot = Input.GetAxis("Mouse_Y");
            return new Vector2(xRot, yRot);
        } else {
            float xRot = Input.GetAxis("Mouse_X_CONTROLLER");
            float yRot = Input.GetAxis("Mouse_Y_CONTROLLER");
            return new Vector2(xRot, yRot);
        }
    } 

    public Vector2 GetCameraMouvement() {
        Vector2 mouseMouvement = GetMouseMouvement();
        return new Vector2(- mouseMouvement.y, mouseMouvement.x);
    } 

    public bool GetJump() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKey(KeyCode.Space);
        }
        return controllerRightTrigger.Get();
    }

    public bool GetJumpDown() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKeyDown(KeyCode.Space);
        }
        return controllerRightTrigger.GetDown();
    }

    public bool GetJumpUp() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKeyUp(KeyCode.Space);
        }
        return controllerRightTrigger.GetUp();
    }

    public bool GetShift() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKey(KeyCode.LeftShift);
        }
        return controllerLeftTrigger.Get();
    }

    public bool GetPouvoirADown() {
        return Input.GetKeyDown(GetPouvoirAKeyCode());
    }

    public KeyCode GetPouvoirAKeyCode() {
        switch (GetCurrentKeybindingType()) {
            case KeybindingType.AZERTY:
                return KeyCode.A;
            case KeybindingType.QWERTY:
                return KeyCode.Q;
            case KeybindingType.CONTROLLER:
                return KeyCode.JoystickButton2;
            default:
                return KeyCode.None;
        }
    }

    public bool GetPouvoirEDown() {
        return Input.GetKeyDown(GetPouvoirEKeyCode());
    }

    public KeyCode GetPouvoirEKeyCode() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return KeyCode.E;
        }
        return KeyCode.JoystickButton3;
    }

    public bool GetPouvoirLeftClickDown() {
        return Input.GetKeyDown(GetPouvoirLeftClickKeyCode());
    }

    public KeyCode GetPouvoirLeftClickKeyCode() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return KeyCode.Mouse0;
        }
        return KeyCode.JoystickButton0;
    }

    public bool GetPouvoirRightClickDown() {
        return Input.GetKeyDown(GetPouvoirRightClickKeyCode());
    }

    public KeyCode GetPouvoirRightClickKeyCode() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return KeyCode.Mouse1;
        }
        return KeyCode.JoystickButton1;
    }

    public Vector3 GetHorizontalMouvement() {
        switch (GetCurrentKeybindingType()) {
            case KeybindingType.AZERTY:
                return new Vector3(Input.GetAxis("HorizontalAZERTY"), 0, Input.GetAxis("VerticalAZERTY"));
            case KeybindingType.QWERTY:
                return new Vector3(Input.GetAxis("HorizontalQWERTY"), 0, Input.GetAxis("VerticalQWERTY"));
            case KeybindingType.CONTROLLER:
                float xJoystick = Input.GetAxis("HorizontalCONTROLLER_JOYSTICK");
                float xArrow = Input.GetAxis("HorizontalCONTROLLER_ARROW");
                float yJoystick = Input.GetAxis("VerticalCONTROLLER_JOYSTICK");
                float yArrow = Input.GetAxis("VerticalCONTROLLER_ARROW");
                float x = Math.Abs(xJoystick) >= Mathf.Abs(xArrow) ? xJoystick : xArrow;
                float y = Math.Abs(yJoystick) >= Mathf.Abs(yArrow) ? yJoystick : yArrow;
                return new Vector3(x, 0, y);
            default:
                return Vector3.zero;
        }
    }

    public bool GetRestartGame() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKeyDown(KeyCode.R);
        }
        return Input.GetKeyDown(KeyCode.JoystickButton6);
    }

    public bool GetPauseGame() {
        // On peut toujours accéder aux options quel que soit le keybinding ! :)
        return Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.Escape);
    }

    public bool GetOptions() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKeyDown(KeyCode.O);
        }
        return false;
    }

    public bool GetPauseReturnToMenu() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetKeyDown(KeyCode.Space);
        }
        return false;
    }

    protected void DetectPlugUnplugController() {
        bool isAControllerPlugin = GetJoystickName() != "";
        if(isAControllerPlugin && !wasAControllerPlugin) {
            NotifyControllerPlugIn();
            SetKeybindingType(KeybindingType.CONTROLLER);
        } else if (!isAControllerPlugin && wasAControllerPlugin) {
            NotifyControllerPlugOut();
            SetKeybindingType(GetDefaultKeybindingType());
        }
        wasAControllerPlugin = isAControllerPlugin;
    }

    public void NotifyControllerPlugIn() {
        GameManager gm = FindObjectOfType<GameManager>();
        SelectorManager sm = FindObjectOfType<SelectorManager>();
        if(gm != null) {
            gm.console.ControllerPlugIn();
        } else if (sm != null) {
            sm.NotifyControllerPlugIn();
        }
    }

    public void NotifyControllerPlugOut() {
        GameManager gm = FindObjectOfType<GameManager>();
        SelectorManager sm = FindObjectOfType<SelectorManager>();
        if (gm != null) {
            gm.console.ControllerPlugOut();
        } else if (sm != null) {
            sm.NotifyControllerPlugOut();
        }
    }

    public string GetJoystickName() {
        string[] joystickNames = Input.GetJoystickNames();
        return joystickNames.Length > 0 ? joystickNames[0] : "";
    }
}
