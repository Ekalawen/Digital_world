using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public abstract class InputController : MonoBehaviour {

    protected InputManager inputManager;

    public virtual void Initialize(InputManager inputManager) {
        this.inputManager = inputManager;
    }

    public abstract InputManager.KeybindingType GetKeybindingType();

    public abstract Vector2 GetMouseMouvement();

    public abstract bool GetMouseLeftClickDown();

    public abstract bool GetMouseLeftClickUp();

    public abstract Vector3 GetCameraSelectorMouvement();

    public abstract bool GetJump();

    public abstract bool GetJumpDown();

    public abstract bool GetJumpUp();

    public abstract bool GetShift();

    public abstract bool GetShiftDown();

    public abstract bool GetShiftUp();

    public abstract KeyCode GetPouvoirAKeyCode();

    public abstract KeyCode GetPouvoirEKeyCode();

    public abstract KeyCode GetPouvoirLeftClickKeyCode();

    public abstract KeyCode GetPouvoirRightClickKeyCode();

    public abstract Vector3 GetHorizontalMouvement(bool rawAxis = false);

    public abstract bool GetRestartGame();

    public abstract bool GetPauseGame();

    public abstract bool GetOptions();

    public abstract bool GetPauseReturnToMenu();
}
