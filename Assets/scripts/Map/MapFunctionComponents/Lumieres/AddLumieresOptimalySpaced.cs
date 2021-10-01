using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddLumieresOptimalySpaced : GenerateLumieresMapFunction {

    public int nbLumieres = 1;
    public int nbTriesByLumiere = 100;
    public GetOptimalySpacedPositions.Mode mode = GetOptimalySpacedPositions.Mode.MAX_MIN_DISTANCE;
    public int offsetFromSides = 0;

    public override void Activate() {
        CreateLumieresOptimalySpaced();
    }

    protected void CreateLumieresOptimalySpaced() {
        for(int i = 0; i < nbLumieres; i++) {
            CreateLumiereOptimalyFarFromOtherLumieres();
        }
    }

    protected void CreateLumiereOptimalyFarFromOtherLumieres() {
        List<Vector3> otherLumieresPositions = map.GetLumieres().Select(l => l.transform.position).ToList();
        Vector3 position = GetOptimalySpacedPositions.GetOneSpacedPosition(map, otherLumieresPositions, nbTriesByLumiere, mode, offsetFromSides);
        map.CreateLumiere(position, lumiereType);
    }
}
