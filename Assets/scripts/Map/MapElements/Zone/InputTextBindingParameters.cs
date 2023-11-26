﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InputTextBindingParameters : MonoBehaviour {

    public enum Bindings {
        MOUVEMENT,
        CAMERA,
        JUMP,
        SHIFT,
        POUVOIR_A,
        POUVOIR_E,
        POUVOIR_LEFT,
        POUVOIR_RIGHT,
        RESTART,
        PAUSE,
    };

    public List<Bindings> bindingParameter;

    public string GetParameterFor(int indice) {
        return InputManager.Instance.GetCurrentInputController().GetStringForBinding(bindingParameter[indice]);
    }

    public object[] GetArguments() {
        return Enumerable.Range(0, bindingParameter.Count).Select(i => GetParameterFor(i)).ToArray();
    }
}
