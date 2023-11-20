using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputControllerPlayStation : InputController {

    protected AxisButton controllerLeftBumper;

    public override void Initialize(InputManager inputManager) {
        base.Initialize(inputManager);
        controllerLeftBumper = new AxisButton("GripDash_PlayStation");
    }

    private void Update() {
        controllerLeftBumper.Update();
    }

    public override Vector3 GetCameraSelectorMouvement() {
        return Vector3.zero;
    }

    public override Vector3 GetHorizontalMouvement(bool rawAxis = false) {
        float xJoystick = inputManager.GetAxis("HorizontalPlayStation_JOYSTICK", rawAxis);
        float xArrow = inputManager.GetAxis("HorizontalPlayStation_ARROW", rawAxis);
        float yJoystick = inputManager.GetAxis("VerticalPlayStation_JOYSTICK", rawAxis);
        float yArrow = inputManager.GetAxis("VerticalPlayStation_ARROW", rawAxis);
        float x = Math.Abs(xJoystick) >= Mathf.Abs(xArrow) ? xJoystick : xArrow;
        float y = Math.Abs(yJoystick) >= Mathf.Abs(yArrow) ? yJoystick : yArrow;
        return new Vector3(x, 0, y);
    }

    public override bool GetJump() {
        return Input.GetKey(KeyCode.JoystickButton5);
    }

    public override bool GetJumpDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton5);
    }

    public override bool GetJumpUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton5);
    }

    public override InputManager.KeybindingType GetKeybindingType() {
        return InputManager.KeybindingType.PLAYSTATION;
    }

    public override bool GetMouseLeftClickDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public override bool GetMouseLeftClickUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton0);
    }

    public override Vector2 GetMouseMouvement() {
        float xRot = Input.GetAxis("Mouse_X_PlayStation");
        float yRot = Input.GetAxis("Mouse_Y_PlayStation");
        return new Vector2(xRot, yRot);
    }

    public override bool GetOptionsInput() {
        return false;
    }

    public override bool GetSkillTreeInput() {
        return false;
    }

    public override bool GetPauseGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton7);
    }

    public override bool GetRestartGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton6);
    }

    public override bool GetPauseReturnToMenu() {
        return false;
    }

    public override KeyCode GetPouvoirAKeyCode() {
        return KeyCode.JoystickButton0;
    }

    public override KeyCode GetPouvoirEKeyCode() {
        return KeyCode.JoystickButton1;
    }

    public override KeyCode GetPouvoirLeftClickKeyCode() {
        return KeyCode.JoystickButton4;
    }

    public override bool GetPouvoirLeftClickDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton4);
    }

    public override KeyCode GetPouvoirRightClickKeyCode() {
        return KeyCode.JoystickButton4; // Bof ==> Non-utilisé ! :)
    }

    public override bool GetPouvoirRightClickDown() {
        return controllerLeftBumper.GetDown();
    }

    public override bool GetShift() {
        return Input.GetKey(KeyCode.JoystickButton8);
    }

    public override bool GetShiftDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton8);
    }

    public override bool GetShiftUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton8);
    }

    public override bool IsController() {
        return true;
    }

    public override string GetName() {
        return "PlayStation";
    }
} 