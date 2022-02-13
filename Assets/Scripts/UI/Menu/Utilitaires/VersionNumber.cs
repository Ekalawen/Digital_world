using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class VersionNumber : MonoBehaviour {

    public TMP_Text text;
    public bool isInGame = false;

    public void Start()
    {
        text.text = $"v{Application.version}";
        if (IsDemo()) {
            text.text += ".demo";
        }
    }

    protected bool IsDemo() {
        return !isInGame ? MenuManager.Instance.IsDemo() : GameManager.Instance.console.IsDemo();
    }
}
