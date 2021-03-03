using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratesCavesWhileTrue : GenerateCaves {

    public Lumiere.LumiereType lumiereType = Lumiere.LumiereType.NORMAL;
    public bool fullFillFloorExceptOne = false;

    protected override Cave GenerateCave() {
        // On définit la taille de la cave
        Vector3Int size = Vector3Int.zero;
        size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

        // On définit sa position sur la carte
        Vector3 position = GetPositionCave(size, caveOffsetFromSidesXZandY);

        Cave cave = new Cave(position, size, bMakeSpaceArround: makeSpaceArround, bDigInside: true);
        cave.FulfillFloor(exceptOne: fullFillFloorExceptOne);
        cave.AddAllLumiereInside(lumiereType);

        return cave;
    }

}
