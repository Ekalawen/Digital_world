using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputControllerSwitch : InputController {

    public override Vector3 GetCameraSelectorMouvement() {
        return Vector3.zero;

    }

    public override Vector3 GetHorizontalMouvement(bool rawAxis = false) {
        float xJoystick = inputManager.GetAxis("HorizontalSwitch_JOYSTICK", rawAxis);
        float yJoystick = inputManager.GetAxis("VerticalSwitch_JOYSTICK", rawAxis);
        return new Vector3(xJoystick, 0, yJoystick);
    }

    public override bool GetJump() {
        return Input.GetKey(KeyCode.JoystickButton7);
    }

    public override bool GetJumpDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton7);
    }

    public override bool GetJumpUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton7);
    }

    public override InputManager.KeybindingType GetKeybindingType() {
        return InputManager.KeybindingType.SWITCH;
    }

    public override bool GetMouseLeftClickDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public override bool GetMouseLeftClickUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton0);
    }

    public override Vector2 GetMouseMouvement() {
        float xRot = Input.GetAxis("Mouse_X_Switch");
        float yRot = Input.GetAxis("Mouse_Y_Switch");
        return new Vector2(xRot, yRot);
    }

    public override bool GetOptions() {
        return false;
    }

    public override bool GetPauseGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton9)
            || Input.GetKeyDown(KeyCode.JoystickButton12);
    }

    public override bool GetRestartGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton8)
            || Input.GetKeyDown(KeyCode.JoystickButton13);
    }

    public override bool GetPauseReturnToMenu() {
        return false;
    }

    public override KeyCode GetPouvoirAKeyCode() {
        return KeyCode.JoystickButton1;
    }

    public override KeyCode GetPouvoirEKeyCode() {
        return KeyCode.JoystickButton0;
    }

    public override KeyCode GetPouvoirLeftClickKeyCode() {
        return KeyCode.JoystickButton6;
    }

    public override bool GetPouvoirLeftClickDown() {
        return Input.GetKeyDown(GetPouvoirLeftClickKeyCode());
    }

    public override KeyCode GetPouvoirRightClickKeyCode() {
        return KeyCode.JoystickButton4;
    }

    public override bool GetShift() {
        return Input.GetKey(KeyCode.JoystickButton10);
    }

    public override bool GetShiftDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton10);
    }

    public override bool GetShiftUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton10);
    }

    public override bool IsController() {
        return true;
    }

    public override string GetName() {
        return "Switch";
    }
} 