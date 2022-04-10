using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputManager : MonoBehaviour {

    public enum KeybindingType {
        AZERTY,
        QWERTY,
        XBOX,
    };

    static InputManager _instance;
    public static InputManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<InputManager>()); } }

    public KeybindingType currentKeybindingType;

    protected List<InputController> inputControllers = new List<InputController>();
    protected InputController currentInputController;
    protected bool wasAControllerPlugin;
    protected bool isInGame = false;

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "InputManager";
        DontDestroyOnLoad(this);
        currentKeybindingType = GetKeybindingType();
        AddInputControllers();
        wasAControllerPlugin = GetJoystickName() != "";
    }

    protected void AddInputControllers() {
        inputControllers.Add(gameObject.AddComponent<InputControllerQWERTY>());
        inputControllers.Add(gameObject.AddComponent<InputControllerAZERTY>());
        inputControllers.Add(gameObject.AddComponent<InputControllerXbox>());
        foreach(InputController inputController in inputControllers) {
            inputController.Initialize(this);
        }
        currentInputController = inputControllers.Find(ic => ic.GetKeybindingType() == GetCurrentKeybindingType());
        EnsureDefaultKeyboardIsAlwaysFirst();
    }

    protected void EnsureDefaultKeyboardIsAlwaysFirst() {
        if (KeybindingDropdown.GetDefaultKeybinding() == KeybindingType.AZERTY) {
            InputController tmp = inputControllers[0];
            inputControllers[0] = inputControllers[1];
            inputControllers[1] = tmp;
        }
    }

    public void Update() {
        DetectPlugUnplugController();
        DetectUseOtherController();
    }

    protected int GetKeybindingTypeIndice() {
        KeybindingType defaultValue = KeybindingDropdown.GetDefaultKeybinding();
        return PrefsManager.GetInt(PrefsManager.KEYBINDING_INDICE_KEY, (int)defaultValue);
    }

    protected KeybindingType GetKeybindingType() {
        return (KeybindingType)GetKeybindingTypeIndice();
    }

    public KeybindingType GetCurrentKeybindingType() {
        return currentKeybindingType;
    }

    public InputController GetCurrentInputController() {
        return currentInputController;
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
        KeybindingType keybinding = (KeybindingType)keybindingIndice;
        currentKeybindingType = keybinding;
        currentInputController = inputControllers.Find(ic => ic.GetKeybindingType() == keybinding);

        KeybindingDropdown keybindingDropdown = FindObjectOfType<KeybindingDropdown>();
        if(keybindingDropdown != null) {
            keybindingDropdown.dropdown.SetValueWithoutNotify(keybindingIndice);
        }

        if(isInGame) {
            GameManager.Instance.console.UpdatePouvoirBindings();
        }
    }

    public KeybindingType GetDefaultKeybindingType() {
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0]) { // 0 is French
            return KeybindingType.AZERTY;
        }
        return KeybindingType.QWERTY;
    }

    public Vector2 GetMouseMouvement() {
        return currentInputController.GetMouseMouvement();
    } 

    public Vector3 GetCameraSelectorMouvement() {
        return currentInputController.GetCameraSelectorMouvement();
    }

    public Vector2 GetCameraMouvement() {
        Vector2 mouseMouvement = GetMouseMouvement();
        return new Vector2(- mouseMouvement.y, mouseMouvement.x);
    } 

    public bool GetJump() {
        return currentInputController.GetJump();
    }

    public bool GetJumpDown() {
        return currentInputController.GetJumpDown();
    }

    public bool GetJumpUp() {
        return currentInputController.GetJumpUp();
    }

    public bool GetShift() {
        return currentInputController.GetShift();
    }

    public bool GetShiftDown() {
        return currentInputController.GetShiftDown();
    }

    public bool GetShiftUp() {
        return currentInputController.GetShiftUp();
    }

    public bool GetPouvoirADown() {
        return Input.GetKeyDown(GetPouvoirAKeyCode());
    }

    public KeyCode GetPouvoirAKeyCode() {
        return currentInputController.GetPouvoirAKeyCode();
    }

    public bool GetPouvoirEDown() {
        return Input.GetKeyDown(GetPouvoirEKeyCode());
    }

    public KeyCode GetPouvoirEKeyCode() {
        return currentInputController.GetPouvoirEKeyCode();
    }

    public KeyCode GetPouvoirLeftClickKeyCode() {
        return currentInputController.GetPouvoirLeftClickKeyCode();
    }

    public bool GetPouvoirLeftClickDown() {
        return currentInputController.GetPouvoirLeftClickDown();
    }

    public bool GetMouseLeftClickDown() {
        return currentInputController.GetMouseLeftClickDown();
    }

    public bool GetMouseLeftClickUp() {
        return currentInputController.GetMouseLeftClickUp();
    }

    public bool GetPouvoirRightClickDown() {
        return Input.GetKeyDown(GetPouvoirRightClickKeyCode());
    }

    public KeyCode GetPouvoirRightClickKeyCode() {
        return currentInputController.GetPouvoirRightClickKeyCode();
    }

    public bool GetKeyDown(KeyCode binding) {
        return Input.GetKeyDown(binding);
    }

    public bool GetKeyUp(KeyCode binding) {
        return Input.GetKeyUp(binding);
    }

    public Vector3 GetHorizontalMouvement(bool rawAxis = false) {
        return currentInputController.GetHorizontalMouvement(rawAxis);
    }

    public float GetAxis(string axisName, bool rawAxis = false) {
        if(rawAxis) {
            return Input.GetAxisRaw(axisName);
        }
        return Input.GetAxis(axisName);
    }

    public bool GetRestartGame() {
        return currentInputController.GetRestartGame();
    }

    public bool GetPauseGame() {
        return inputControllers.Any(ic => ic.GetPauseGame());
    }

    public bool GetOptions() {
        return currentInputController.GetOptions();
    }

    public bool GetPauseReturnToMenu() {
        return currentInputController.GetPauseReturnToMenu();
    }

    protected void DetectUseOtherController() {
        if(!isInGame) {
            return;
        }
        if(GameManager.Instance.IsPaused()) {
            return;
        }

        foreach(InputController inputController in inputControllers) {
            if(inputController != currentInputController
            && !(inputController.IsKeyboard() && currentInputController.IsKeyboard())) { // Dont swap from a keyboard controller to another !
                if(inputController.AnyImportantKeyUsed()) {
                    SwapToOtherControllerInGame(inputController);
                }
            }
        }
    }

    protected void SwapToOtherControllerInGame(InputController inputController) {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) {
            GameManager.Instance.console.SwapToController(inputController);
            SetKeybindingType(inputController.GetKeybindingType());
        }
    }

    protected void DetectPlugUnplugController() {
        bool isAControllerPlugin = GetJoystickName() != "";
        if(isAControllerPlugin && !wasAControllerPlugin) {
            NotifyControllerPlugIn();
            SetKeybindingType(KeybindingType.XBOX);
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

    public bool GetAnyKeyOrButtonDown() {
        return Input.anyKeyDown;
    }

    public void SetInGame() {
        isInGame = true;
    }

    public void SetNotInGame() {
        isInGame = false;
    }
}
