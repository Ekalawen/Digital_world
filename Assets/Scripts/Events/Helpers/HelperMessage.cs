using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HelperMessage {

    public TimedMessageStruct timedMessage;
    public bool useSound = true;

    protected bool hasBeenDisplayed = false;

    public void DisplayMessage() {
        if (!hasBeenDisplayed) {
            hasBeenDisplayed = true;
            GameManager gm = GameManager.Instance;
            gm.console.AjouterMessageImportant(timedMessage.message, timedMessage.type, timedMessage.duree);
            if (useSound) {
                gm.soundManager.PlayReceivedMessageClip();
            }
        }
    }

    public float GetTiming() {
        return timedMessage.timing;
    }
}
