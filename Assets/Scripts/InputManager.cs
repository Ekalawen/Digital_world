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

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "InputManager";
        DontDestroyOnLoad(this);
        currentKeybindingType = GetKeybindingType();
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
        return Input.GetButton("Jump");
    }

    public bool GetJumpDown() {
        return Input.GetButtonDown("Jump");
    }

    public bool GetJumpUp() {
        return Input.GetButtonUp("Jump");
    }

    public bool GetShift() {
        return Input.GetKey(KeyCode.LeftShift);
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
                return Vector3.zero;
            default:
                return Vector3.zero;
        }
    }
}
