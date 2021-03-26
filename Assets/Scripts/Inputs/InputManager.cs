using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        PrefsManager.SetInt(PrefsManager.KEYBINDING_INDICE_KEY, keybindingIndice);
        currentKeybindingType = (KeybindingType)keybindingIndice;
    }

    public Vector2 GetMouseMouvement() {
        float xRot = Input.GetAxis("Mouse X");
        float yRot = Input.GetAxis("Mouse Y");
        return new Vector2(xRot, yRot);
    } 

    public Vector2 GetCameraMouvement() {
        Vector2 mouseMouvement = GetMouseMouvement();
        return new Vector2(- mouseMouvement.y, mouseMouvement.x);
    } 

    public bool GetJump() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetButton("Jump");
        }
        return controllerRightTrigger.Get();
    }

    public bool GetJumpDown() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetButtonDown("Jump");
        }
        return controllerRightTrigger.GetDown();
    }

    public bool GetJumpUp() {
        if (GetCurrentKeybindingType() != KeybindingType.CONTROLLER) {
            return Input.GetButtonUp("Jump");
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
        if (GetCurrentKeybindingType() == KeybindingType.AZERTY) {
            return KeyCode.A;
        } else {
            return KeyCode.Q;
        }
    }

    public bool GetPouvoirEDown() {
        return Input.GetKeyDown(GetPouvoirEKeyCode());
    }

    public KeyCode GetPouvoirEKeyCode() {
        return KeyCode.E;
    }

    public bool GetPouvoirLeftClickDown() {
        return Input.GetMouseButtonDown(0);
    }

    public KeyCode GetPouvoirLeftClickKeyCode() {
        return KeyCode.Mouse0;
    }

    public bool GetPouvoirRightClickDown() {
        return Input.GetMouseButtonDown(1);
    }

    public KeyCode GetPouvoirRightClickKeyCode() {
        return KeyCode.Mouse1;
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
}
