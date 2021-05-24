using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventMapFunction : SingleEvent {

    public List<MapFunctionComponent> mapFunctions;
    [SerializeField]
    public TimedLocalizedMessage startEventMessage;
    [SerializeField]
    public TimedLocalizedMessage startEventMessageImportant;

    public override void Trigger()
    {
        // Attention, code dupliqué de MapFunctionEvent ! :'(
        // Idéalement il faudrait que tous les RandomsEvents actuels soient des SingleEvents.
        // Et RandomEvent triggererait uniquement les SingleEvents qu'il aurait linké :)
        // Ya quand même des trucs bien à garder dans les RandomEvents, tel que le PlayStartSound et StartEventConsoleMessage et autres :)
        TriggerConsoleMessages();
        gm.soundManager.PlayEventStartClip();
        foreach (MapFunctionComponent mapFunction in mapFunctions) {
            mapFunction.Initialize();
            mapFunction.Activate();
        }
    }

    protected void TriggerConsoleMessages() {
        if (startEventMessage.localizedString != null) {
            gm.console.AjouterMessage(startEventMessage);
        }
        if (startEventMessageImportant.localizedString != null) {
            gm.console.AjouterMessageImportant(startEventMessageImportant, bAfficherInConsole: false);
        }
    }
}
