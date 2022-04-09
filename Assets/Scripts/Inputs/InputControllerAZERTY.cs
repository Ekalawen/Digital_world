using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputControllerAZERTY : InputController {

    public override Vector3 GetCameraSelectorMouvement() {
        float x = inputManager.GetAxis("CameraSelector_X_AZERTY");
        float y = inputManager.GetAxis("CameraSelector_Y");
        float z = inputManager.GetAxis("CameraSelector_Z_AZERTY");
        return new Vector3(x, y, z);
    }

    public override Vector3 GetHorizontalMouvement(bool rawAxis = false) {
        float x = inputManager.GetAxis("HorizontalAZERTY", rawAxis);
        float y = 0;
        float z = inputManager.GetAxis("VerticalAZERTY", rawAxis);
        return new Vector3(x, y, z);
    }

    public override bool GetJump() {
        return Input.GetKey(KeyCode.Space);
    }

    public override bool GetJumpDown() {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public override bool GetJumpUp() {
        return Input.GetKeyUp(KeyCode.Space);
    }

    public override InputManager.KeybindingType GetKeybindingType() {
        return InputManager.KeybindingType.AZERTY;
    }

    public override bool GetMouseLeftClickDown() {
        return Input.GetKeyDown(KeyCode.Mouse0);
    }

    public override bool GetMouseLeftClickUp() {
        return Input.GetKeyUp(KeyCode.Mouse0);
    }

    public override Vector2 GetMouseMouvement() {
        float xRot = inputManager.GetAxis("Mouse_X");
        float yRot = inputManager.GetAxis("Mouse_Y");
        return new Vector2(xRot, yRot);
    }

    public override bool GetOptions() {
        return Input.GetKeyDown(KeyCode.O);
    }

    public override bool GetPauseGame() {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    public override bool GetPauseReturnToMenu() {
        return Input.GetKeyDown(KeyCode.F1);
    }

    public override KeyCode GetPouvoirAKeyCode() {
        return KeyCode.A;
    }

    public override KeyCode GetPouvoirEKeyCode() {
        return KeyCode.E;
    }

    public override KeyCode GetPouvoirLeftClickKeyCode() {
        return KeyCode.Mouse0;
    }

    public override KeyCode GetPouvoirRightClickKeyCode() {
        return KeyCode.Mouse1;
    }

    public override bool GetRestartGame() {
        return Input.GetKeyDown(KeyCode.R);
    }

    public override bool GetShift() {
        return Input.GetKey(KeyCode.LeftShift);
    }

    public override bool GetShiftDown() {
        return Input.GetKeyDown(KeyCode.LeftShift);
    }

    public override bool GetShiftUp() {
        return Input.GetKeyUp(KeyCode.LeftShift);
    }
} 