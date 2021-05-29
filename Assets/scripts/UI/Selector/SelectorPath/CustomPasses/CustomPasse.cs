using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public abstract class CustomPasse : MonoBehaviour {
    public abstract string GetPasse(SelectorPath selectorPath);
}
