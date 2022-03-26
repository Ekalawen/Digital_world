using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddLumieresOptimalySpaced : GenerateLumieresMapFunction {

    public enum PositionsMode { OFFSETS_FROM_SIDES, FROM_GET_HELPER };

    public int nbLumieres = 1;
    public int nbTriesByLumiere = 100;
    public GetOptimalySpacedPositions.Mode mode = GetOptimalySpacedPositions.Mode.MAX_MIN_DISTANCE;
    public PositionsMode positionsMode = PositionsMode.OFFSETS_FROM_SIDES;
    [ConditionalHide("positionsMode", PositionsMode.OFFSETS_FROM_SIDES)]
    public int offsetFromSides = 0;
    [ConditionalHide("positionsMode", PositionsMode.FROM_GET_HELPER)]
    public GetEmptyPositionsHelper getPositionsHelper;

    public override void Activate() {
        CreateLumieresOptimalySpaced();
    }

    protected void CreateLumieresOptimalySpaced() {
        for(int i = 0; i < nbLumieres; i++) {
            CreateLumiereOptimalyFarFromOtherLumieres();
        }
    }

    protected void CreateLumiereOptimalyFarFromOtherLumieres() {
        if (positionsMode == PositionsMode.OFFSETS_FROM_SIDES) {
            CreateLumiereWithOffset();
        } else {
            CreateLumiereWithHelper();
        }
    }

    protected void CreateLumiereWithHelper() {
        List<Vector3> otherLumieresPositions = map.GetLumieres().Select(l => l.transform.position).ToList();
        List<Vector3> candidatePositions = getPositionsHelper.Get();
        Vector3 position = GetOptimalySpacedPositions.GetOneSpacedPositionWithCandidatePositions(candidatePositions, otherLumieresPositions, nbTriesByLumiere, mode);
        map.CreateLumiere(position, lumiereType);
    }

    protected void CreateLumiereWithOffset() {
        List<Vector3> otherLumieresPositions = map.GetLumieres().Select(l => l.transform.position).ToList();
        Vector3 position = GetOptimalySpacedPositions.GetOneSpacedPosition(map, otherLumieresPositions, nbTriesByLumiere, mode, offsetFromSides);
        map.CreateLumiere(position, lumiereType);
    }
}
