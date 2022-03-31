using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

public class RandomEventGenerator : IGenerator {

    [Header("Random Event")]
    public GameObject randomEventPrefab;
    public TimedLocalizedMessage messageOnStop;

    protected RandomEvent randomEvent;

    public override void Initialize() {
        base.Initialize();
        randomEvent = gm.eventManager.AddRandomEvent(randomEventPrefab);
    }

    public override void DestroyIn(float duree) {
        base.DestroyIn(duree);
        gm.eventManager.RemoveEvent(randomEvent);
        gm.console.AjouterMessageImportant(messageOnStop);
    }

    protected override void GenerateOneSpecific(Vector3 position) {
        // Do nothing
    }

    protected override void TryGenerate() {
        // Do nothing too !
    }

    protected override void ComputePrecomputedPositions() {
        // Do nothing again !
    }

    protected override bool IsValidPosition(Vector3 p) {
        return true;
    }
}
