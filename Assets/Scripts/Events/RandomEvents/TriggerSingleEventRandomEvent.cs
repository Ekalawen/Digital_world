using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerSingleEventRandomEvent : RandomEvent {

    public SingleEvent singleEvent;

    protected override void StartEventConsoleMessage() {
        singleEvent.Initialize();
        singleEvent.Trigger();
    }

    public override void StopEvent() {
    }

    protected override void EndEvent() {
    }

    protected override void StartEvent() {
    }
}

