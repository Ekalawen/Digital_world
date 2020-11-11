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

    protected override void StartEventConsoleMessage() {
        gm.console.AjouterMessage(startEventMessage);
        gm.console.AjouterMessageImportant(startEventMessageImportant, bAfficherInConsole: false);
    }

    protected override void StartEvent() {
        mapFunction.Initialize();
        mapFunction.Activate();
    }

    protected override void EndEvent() {
    }

    public override void StopEvent() {
    }
}
