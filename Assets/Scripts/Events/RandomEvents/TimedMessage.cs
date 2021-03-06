﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization;

[Serializable]
public class TimedMessage : MonoBehaviour {
    public string message = "Je suis un message qui apparaitra au bout de 5 secondes !";
    public Console.TypeText type = Console.TypeText.ALLY_TEXT;
    public float timing = 0f;
    public float duree = 2f;

    public void Initialize(string message, Console.TypeText type, float timing, float duree) {
        this.message = message;
        this.type = type;
        this.timing = timing;
        this.duree = duree;
    }
}

[Serializable]
public class TimedLocalizedMessage {
    public LocalizedString localizedString;
    public Console.TypeText type = Console.TypeText.ALLY_TEXT;
    public float timing = 0f;
    public float duree = 2f;

    public void Initialize(LocalizedString localizedString, Console.TypeText type, float timing, float duree) {
        this.localizedString = localizedString;
        this.type = type;
        this.timing = timing;
        this.duree = duree;
    }
}
