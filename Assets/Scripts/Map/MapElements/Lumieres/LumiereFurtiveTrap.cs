using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFurtiveTrap : LumiereFurtive {

    public GameObject ennemiToGeneratePrefab;
    public float timeBeforeEnnemisActivation = 3.0f;
    public GameObject lumiereGeneratedPrefab;
    public int nbLumieresToGenerate = 1;

    protected override void OnTriggerEnterSpecific() {
        base.OnTriggerEnterSpecific();

        // Faire poper des ennemis à tous les coins du niveau, et une au centre !
        List<Vector3> allCorners = gm.map.GetAllInsidedCorners();
        allCorners.Add(gm.map.GetCenter());
        foreach(Vector3 corner in allCorners) {
            Ennemi ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiToGeneratePrefab, corner);
            ennemi.tempsInactifDebutJeu = Time.timeSinceLevelLoad + timeBeforeEnnemisActivation;
        }

        // Ainsi qu'une autre lumiere furtive ! (mais juste furtive cette fois-ci ^^)
        for (int i = 0; i < nbLumieresToGenerate; i++) {
            Vector3 posLumiere = gm.map.GetFarRoundedLocation(gm.player.transform.position);
            gm.map.CreateLumiere(posLumiere, LumiereType.SPECIAL);
        }

        gm.console.AnalyzeLevelDeuxiemeSalve();
        gm.soundManager.PlayReceivedMessageClip();
    }
}
