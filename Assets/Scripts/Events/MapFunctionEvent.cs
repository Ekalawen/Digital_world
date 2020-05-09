using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFunctionEvent : RandomEvent {

    public MapFunctionComponent mapFunction;
    [SerializeField]
    public TimedMessage startEventMessage;
    [SerializeField]
    public TimedMessage startEventMessageImportant;

    public override void StartEventConsoleMessage() {
        gm.console.AjouterMessage(startEventMessage);
        gm.console.AjouterMessageImportant(startEventMessageImportant, bAfficherInConsole: false);
    }

    public override void StartEvent() {
        mapFunction.Initialize();
        mapFunction.Activate();
    }

    public override void EndEvent() {
    }
}
