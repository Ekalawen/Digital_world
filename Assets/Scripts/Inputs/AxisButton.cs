using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisButton {

    public string name;
    public bool currentState;
    public bool lastState;

    public AxisButton(string axisName) {
        name = axisName;
        lastState = false;
        currentState = Input.GetAxisRaw(name) != 0;
    }

    public void Update() {
        lastState = currentState;
        currentState = Input.GetAxisRaw(name) != 0;
    }

    public bool Get() {
        return currentState;
    }

    public bool GetDown() {
        return currentState && !lastState;
    }

    public bool GetUp() {
        return !currentState && lastState;
    }
}

