using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public abstract class SingleEvent : MonoBehaviour {

    public TimedLocalizedMessage messageConsole;
    public TimedLocalizedMessage messageImportant;

    protected GameManager gm;
    protected EventManager eventManager;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        eventManager = gm.eventManager;
    }

    public void Trigger() {
        TriggerSpecific();
        TriggerConsoleMessage();
        TriggerSound();
    }

    protected virtual void TriggerSound() {
        gm.soundManager.PlayEventStartClip();
    }

    protected virtual void TriggerConsoleMessage() {
        if(!messageImportant.localizedString.IsEmpty) {
            gm.console.AjouterMessageImportant(messageImportant, bAfficherInConsole: false);
        }
        if(!messageConsole.localizedString.IsEmpty) {
            gm.console.AjouterMessage(messageConsole);
        }
    }

    public abstract void TriggerSpecific();
}
