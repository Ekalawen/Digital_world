using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    static InputManager _instance;
    public static InputManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<InputManager>()); } }

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "InputManager";
        DontDestroyOnLoad(this);
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
        if (KeybindingDropdown.GetKeybinding() == KeybindingDropdown.KeybindingType.AZERTY) {
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
        if (KeybindingDropdown.GetKeybinding() == KeybindingDropdown.KeybindingType.AZERTY) {
            return new Vector3(Input.GetAxis("HorizontalAZERTY"), 0, Input.GetAxis("VerticalAZERTY"));
        } else {
            return new Vector3(Input.GetAxis("HorizontalQWERTY"), 0, Input.GetAxis("VerticalQWERTY"));
        }
    }
}
