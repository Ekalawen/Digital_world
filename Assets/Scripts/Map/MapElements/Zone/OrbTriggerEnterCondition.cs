using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class OrbTriggerEnterCondition : MonoBehaviour {

    public TimedLocalizedMessage notFullfilledMessage;

    protected GameManager gm;
    protected OrbTrigger orbTrigger;

    public virtual void Initialize(OrbTrigger orbTrigger) {
        gm = GameManager.Instance;
        this.orbTrigger = orbTrigger;
    }

    public abstract bool IsFullfilled();

    public void DisplayNotFullfilledMessage() {
        gm.console.AjouterMessageImportant(notFullfilledMessage);
    }

    public virtual void OnTrigger() {
    }
}
