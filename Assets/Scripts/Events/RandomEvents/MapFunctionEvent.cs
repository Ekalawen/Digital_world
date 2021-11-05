using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFunctionEvent : RandomEvent {

    public List<MapFunctionComponent> mapFunctions;
    [SerializeField]
    public TimedLocalizedMessage startEventMessage;
    [SerializeField]
    public TimedLocalizedMessage startEventMessageImportant;

    protected override void StartEventConsoleMessage() {
        if (!startEventMessage.localizedString.IsEmpty)
        {
            gm.console.AjouterMessage(startEventMessage);
        }
        if(!startEventMessageImportant.localizedString.IsEmpty) {
            gm.console.AjouterMessageImportant(startEventMessageImportant, bAfficherInConsole: false);
        }
    }

    protected override void StartEvent() {
        foreach (MapFunctionComponent mapFunction in mapFunctions) {
            mapFunction.Initialize();
            mapFunction.Activate();
        }
    }

    protected override void EndEvent() {
    }

    public override void StopEvent() {
    }
}
