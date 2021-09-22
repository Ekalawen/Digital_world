using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddLumieresInMapBordures : GenerateLumieresMapFunction {

    public int nbLumieres = 1;
    public bool useAllBordures = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureHaut = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureBas = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureGauche = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureDroite = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureAvant = true;
    [ConditionalHide("!useAllBordures")]
    public bool useBordureArriere = true;

    public override void Activate() {
        CreateLumieresInMapBordure();
    }

    protected void CreateLumieresInMapBordure() {
        List<Vector3> borduresPositions = GetBorduresPositions();

        int nbLumieresToAdd = Mathf.Min(nbLumieres, borduresPositions.Count);
        for (int i = 0; i < nbLumieresToAdd; i++) {
            Vector3 posLumiere = MathTools.ChoseOne(borduresPositions);
            Debug.Log($"Lumiere in MapBordure = {posLumiere}");
            map.CreateLumiere(posLumiere, lumiereType);
            borduresPositions.Remove(posLumiere);
        }
    }

    protected List<Vector3> GetBorduresPositions() {
        List<Vector3> borduresPositions = new List<Vector3>();
        if (useAllBordures) {
            borduresPositions = map.GetAllPositionsOnBordures();
        } else {
            if (useBordureHaut) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresHaut());
            }
            if (useBordureBas) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresBas());
            }
            if (useBordureGauche) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresGauche());
            }
            if (useBordureDroite) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresDroite());
            }
            if (useBordureAvant) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresAvant());
            }
            if (useBordureArriere) {
                borduresPositions.AddRange(map.GetAllPositionsOnBorduresArriere());
            }
        }
        borduresPositions = borduresPositions.FindAll(p => !map.IsCubeAt(p));
        return borduresPositions;
    }
}
