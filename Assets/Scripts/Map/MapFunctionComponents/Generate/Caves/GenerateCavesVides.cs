using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCavesVides : GenerateCaves {

    public int nbOuverturesOnSides = 2;
    public bool allowOuverturesInCorners = false;

    protected override Cave GenerateCave() {
        // On définit la taille de la cave
        Vector3Int size = Vector3Int.zero;
        size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

        // On définit sa position sur la carte
        Vector3 position = GetPositionCave(size, caveOffsetFromSidesXZandY);

        Cave cave = new Cave(position, size, bMakeSpaceArround: makeSpaceArround, bDigInside: false, preserveMapBordure);

        // On creuse la cave !
        cave.RemoveAllCubesInside(offset: 1);

        // On ajouter des ouvertures sur les cotes ! :)
        cave.AddOuverturesOnSides(nbOuverturesOnSides, allowOuverturesInCorners: allowOuverturesInCorners);

        // On y rajoute la lumière !
        cave.AddNLumiereInside(nbLumieresPerCaves, offsetLumieresFromCenter);

        return cave;
    }
}
