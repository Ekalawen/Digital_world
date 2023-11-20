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

    public abstract bool IsController();

    public bool IsKeyboard() {
        return !IsController();
    }

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

    public virtual bool GetPouvoirLeftClickDown() {
        return Input.GetKeyDown(GetPouvoirLeftClickKeyCode());
    }

    public abstract KeyCode GetPouvoirRightClickKeyCode();

    public virtual bool GetPouvoirRightClickDown() {
        return Input.GetKeyDown(GetPouvoirRightClickKeyCode());
    }

    public abstract Vector3 GetHorizontalMouvement(bool rawAxis = false);

    public abstract bool GetRestartGame();

    public abstract bool GetPauseGame();

    public abstract bool GetOptionsInput();

    public abstract bool GetSkillTreeInput();

    public abstract bool GetPauseReturnToMenu();

    // This function is used to know when a not-used controller is used so that it takes the lead ! :)
    public bool AnyImportantKeyUsed() {
        return GetJumpDown()
            || GetShiftDown()
            || Input.GetKeyDown(GetPouvoirAKeyCode())
            || Input.GetKeyDown(GetPouvoirEKeyCode())
            //|| Input.GetKeyDown(GetPouvoirLeftClickKeyCode()) // Because we don't want to switch to keyboard if we click on the screen !
            //|| Input.GetKeyDown(GetPouvoirRightClickKeyCode())
            || GetHorizontalMouvement() != Vector3.zero
            || GetRestartGame()
            || GetPauseGame();
    }

    public abstract string GetName();

    protected string GetStringForLocalizedStringReference(string reference, params object[] arguments) {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("InputBindings", $"{GetName()}_{reference}", null, FallbackBehavior.UseProjectSettings, arguments).Result;
    }

    public string GetStringForBinding(MessageZoneBindingParameters.Bindings bindings) {
        switch (bindings) {
            case MessageZoneBindingParameters.Bindings.MOUVEMENT:
                return GetStringForLocalizedStringReference("Mouvement");
            case MessageZoneBindingParameters.Bindings.CAMERA:
                return GetStringForLocalizedStringReference("Camera");
            case MessageZoneBindingParameters.Bindings.JUMP:
                return GetStringForLocalizedStringReference("Jump");
            case MessageZoneBindingParameters.Bindings.SHIFT:
                return GetStringForLocalizedStringReference("Shift");
            case MessageZoneBindingParameters.Bindings.POUVOIR_A:
                return GetStringForLocalizedStringReference("PouvoirA");
            case MessageZoneBindingParameters.Bindings.POUVOIR_E:
                return GetStringForLocalizedStringReference("PouvoirE");
            case MessageZoneBindingParameters.Bindings.POUVOIR_LEFT:
                return GetStringForLocalizedStringReference("PouvoirLeft");
            case MessageZoneBindingParameters.Bindings.POUVOIR_RIGHT:
                return GetStringForLocalizedStringReference("PouvoirRight");
            case MessageZoneBindingParameters.Bindings.RESTART:
                return GetStringForLocalizedStringReference("Restart");
            case MessageZoneBindingParameters.Bindings.PAUSE:
                return GetStringForLocalizedStringReference("Pause");
            default:
                throw new Exception("Unknowed Binding in GetStringForBinding !");
        }
    }
}
