using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TimedMessageStruct {
    public string message = "Je suis un message qui apparaitra au bout de 5 secondes !";
    public Console.TypeText type = Console.TypeText.ALLY_TEXT;
    public float duree = 2f;
    public float timing = 5f;

    public void Initialize(string message, Console.TypeText type, float duree, float timing) {
        this.message = message;
        this.type = type;
        this.duree = duree;
        this.timing = timing;
    }
}
