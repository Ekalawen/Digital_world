using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class VersionNumber : MonoBehaviour {

    public TMP_Text text;

    public void Start() {
        text.text = $"v{Application.version}";
    }
}
