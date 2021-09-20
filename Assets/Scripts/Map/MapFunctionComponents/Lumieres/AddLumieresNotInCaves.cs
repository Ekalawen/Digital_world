using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddLumieresNotInCaves : GenerateLumieresMapFunction {

    public int nbLumieres = 1;

    public override void Activate() {
        CreatesLumieresNotInCaves();
    }

    protected void CreatesLumieresNotInCaves() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        List<Vector3> emptyPositions = map.GetAllEmptyPositions();
        List<Vector3> cavesEmptyPositions = caves.SelectMany(c => c.GetAllFreeLocations()).ToList();
        List<Vector3> existingLumieresPositions = map.GetAllLumieresPositions();
        List<Vector3> emptyPositionsNotInCaves = emptyPositions.FindAll(p => !cavesEmptyPositions.Contains(p) && !existingLumieresPositions.Contains(p));

        int nbLumieresToAdd = Mathf.Min(nbLumieres, emptyPositionsNotInCaves.Count);
        for(int i = 0; i < nbLumieresToAdd; i++) {
            Vector3 posLumiere = MathTools.ChoseOne(emptyPositionsNotInCaves);
            map.CreateLumiere(posLumiere, lumiereType);
            emptyPositionsNotInCaves.Remove(posLumiere);
        }
    }
}
