using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputControllerXbox : InputController {

    protected AxisButton controllerLeftTrigger;
    protected AxisButton controllerRightTrigger;

    public override void Initialize(InputManager inputManager) {
        base.Initialize(inputManager);
        controllerRightTrigger = new AxisButton("Jump_Xbox");
        controllerLeftTrigger = new AxisButton("Shift_Xbox");
    }

    private void Update() {
        controllerLeftTrigger.Update();
        controllerRightTrigger.Update();
    }

    public override Vector3 GetCameraSelectorMouvement() {
        //float x = inputManager.GetAxis("CameraSelector_X_Xbox");
        //float y = inputManager.GetAxis("CameraSelector_Y_Xbox");
        //float z = controllerRightTrigger.Get() ? 1 : (controllerLeftTrigger.Get() ? -1 : 0);
        //return new Vector3(x, y, z);
        return Vector3.zero;

    }

    public override Vector3 GetHorizontalMouvement(bool rawAxis = false) {
        float xJoystick = inputManager.GetAxis("HorizontalXbox_JOYSTICK", rawAxis);
        float xArrow = inputManager.GetAxis("HorizontalXbox_ARROW", rawAxis);
        float yJoystick = inputManager.GetAxis("VerticalXbox_JOYSTICK", rawAxis);
        float yArrow = inputManager.GetAxis("VerticalXbox_ARROW", rawAxis);
        float x = Math.Abs(xJoystick) >= Mathf.Abs(xArrow) ? xJoystick : xArrow;
        float y = Math.Abs(yJoystick) >= Mathf.Abs(yArrow) ? yJoystick : yArrow;
        return new Vector3(x, 0, y);
    }

    public override bool GetJump() {
        return controllerRightTrigger.Get();
    }

    public override bool GetJumpDown() {
        return controllerRightTrigger.GetDown();
    }

    public override bool GetJumpUp() {
        return controllerRightTrigger.GetUp();
    }

    public override InputManager.KeybindingType GetKeybindingType() {
        return InputManager.KeybindingType.XBOX;
    }

    public override bool GetMouseLeftClickDown() {
        return Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public override bool GetMouseLeftClickUp() {
        return Input.GetKeyUp(KeyCode.JoystickButton0);
    }

    public override Vector2 GetMouseMouvement() {
        float xRot = Input.GetAxis("Mouse_X_Xbox");
        float yRot = Input.GetAxis("Mouse_Y_Xbox");
        return new Vector2(xRot, yRot);
    }

    public override bool GetOptions() {
        return false;
    }

    public override bool GetPauseGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton7);
    }

    public override bool GetPauseReturnToMenu() {
        return false;
    }

    public override KeyCode GetPouvoirAKeyCode() {
        //return KeyCode.JoystickButton2;
        return KeyCode.JoystickButton0;
    }

    public override KeyCode GetPouvoirEKeyCode() {
        //return KeyCode.JoystickButton3;
        return KeyCode.JoystickButton1;
    }

    public override KeyCode GetPouvoirLeftClickKeyCode() {
        //return KeyCode.JoystickButton0;
        //return KeyCode.JoystickButton8;
        return KeyCode.JoystickButton9; // Bof
    }

    public override bool GetPouvoirLeftClickDown() {
        return controllerLeftTrigger.GetDown();
    }

    public override KeyCode GetPouvoirRightClickKeyCode() {
        //return KeyCode.JoystickButton1;
        return KeyCode.JoystickButton9;
    }

    public override bool GetRestartGame() {
        return Input.GetKeyDown(KeyCode.JoystickButton6);
    }

    public override bool GetShift() {
        //return controllerLeftTrigger.Get();
        return Input.GetKey(KeyCode.JoystickButton8);
    }

    public override bool GetShiftDown() {
        //return controllerLeftTrigger.GetDown();
        return Input.GetKeyDown(KeyCode.JoystickButton8);
    }

    public override bool GetShiftUp() {
        //return controllerLeftTrigger.GetUp();
        return Input.GetKeyUp(KeyCode.JoystickButton8);
    }

    public override bool IsController() {
        return true;
    }

    public override string GetName() {
        return "Xbox";
    }
} 