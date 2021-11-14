using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFurtiveTrap : LumiereFurtive {

    [Header("Ennemis générés")]
    public GameObject ennemiToGeneratePrefab;
    public float timeBeforeEnnemisActivation = 3.0f;
    public GeoData geoDataToEnnemi;

    [Header("Lumières générées")]
    public GameObject lumiereGeneratedPrefab;
    public int nbLumieresToGenerate = 1;
    public GeoData geoDataToLumiere;

    [Header("Pouvoir donné")]
    public GameObject pouvoirDashPrefab;
    public PouvoirGiverItem.PouvoirBinding pouvoirBinding = PouvoirGiverItem.PouvoirBinding.LEFT_CLICK;

    protected override void NotifyEventManagerLumiereCapture() {
        // Don't notify the EventManager !
    }

    protected override void NotifyConsoleLumiereCatpure() {
        // Don't notify the console !
    }

    protected override void CapturedSpecific() {
        base.CapturedSpecific();
        LumiereFurtiveTrapApplier applier = new GameObject("FurtiveTrapApplier").AddComponent<LumiereFurtiveTrapApplier>();
        applier.ApplyTrap(
            ennemiToGeneratePrefab,
            geoDataToEnnemi,
            timeBeforeEnnemisActivation,
            lumiereGeneratedPrefab,
            nbLumieresToGenerate,
            geoDataToLumiere,
            pouvoirDashPrefab,
            pouvoirBinding);
    }
}
