using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization;

[Serializable]
public class TimedMessageStruct {
    public LocalizedString message;
    public Console.TypeText type = Console.TypeText.ALLY_TEXT;
    public float duree = 2f;
    public float timing = 5f;

    public void Initialize(LocalizedString message, Console.TypeText type, float duree, float timing) {
        this.message = message;
        this.type = type;
        this.duree = duree;
        this.timing = timing;
    }
}
