using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventMapFunction : SingleEvent {

    public List<MapFunctionComponent> mapFunctions;

    public override void TriggerSpecific() {
        // Attention, code dupliqué de MapFunctionEvent ! :'(
        // Idéalement il faudrait que tous les RandomsEvents actuels soient des SingleEvents.
        // Et RandomEvent triggererait uniquement les SingleEvents qu'il aurait linké :)
        foreach (MapFunctionComponent mapFunction in mapFunctions) {
            mapFunction.Initialize();
            mapFunction.Activate();
        }
    }
}
