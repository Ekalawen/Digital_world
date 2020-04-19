﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCaves : GenerateCubesMapFunction {

	public float proportionCaves = 0.3f;
    public bool useNbCaves = false;
    public int nbCaves = 0;
    public int tailleMinCave = 3;
    public int tailleMaxCave = 10;
    public bool makeSpaceArround = false;
    public bool preserveMapBordure = false;
    public int nbLumieresPerCaves = 1;
    public int offsetLumieresFromCenter = 1;
    public int caveOffsetFromSides = 2;

    public override void Activate() {
        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        GenerateAllCaves();
    }

	protected List<Cave> GenerateAllCaves() {
        List<Cave> caves = null;
        if (!useNbCaves)
            caves = GenerateAllCavesWithProportion();
        else
            caves = GenerateAllCavesWithNbCaves();

        return caves;
	}

    protected List<Cave> GenerateAllCavesWithProportion() {
        List<Cave> caves = new List<Cave>();
        float currentProportion = 0.0f;
        float volumeCaves = 0;
        while(currentProportion < proportionCaves) {
            Cave cave = GenerateCave();
            caves.Add(cave);
            volumeCaves += cave.GetVolume();
            currentProportion = volumeCaves / map.GetVolume();
		}
        return caves;
    }

    protected List<Cave> GenerateAllCavesWithNbCaves() {
        List<Cave> caves = new List<Cave>();
        for(int i = 0; i < nbCaves; i++) {
            Cave cave = GenerateCave();
            caves.Add(cave);
		}
        return caves;
    }


    protected virtual Cave GenerateCave() {
        // On définit la taille de la cave
        Vector3Int size = Vector3Int.zero;
        size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

        // On définit sa position sur la carte
        Vector3 position = GetPositionCave(size, caveOffsetFromSides);

        Cave cave = new Cave(position, size, makeSpaceArround, bDigInside: true, preserveMapBordure);

        // On y rajoute la lumière !
        cave.AddNLumiereInside(nbLumieresPerCaves, offsetLumieresFromCenter);

        return cave;
    }

    protected virtual Vector3 GetPositionCave(Vector3Int sizeCave, int caveOffsetFromSides) {
        //if (caveOffsetSides) {
        //    return new Vector3(Random.Range(2, map.tailleMap.x - sizeCave.x),
        //        Random.Range(2, map.tailleMap.y - sizeCave.y),
        //        Random.Range(2, map.tailleMap.z - sizeCave.z));
        //} else {
        //    return new Vector3(Random.Range(0, map.tailleMap.x - sizeCave.x + 2),
        //        Random.Range(0, map.tailleMap.y - sizeCave.y + 2),
        //        Random.Range(0, map.tailleMap.z - sizeCave.z + 2));
        //}
        return new Vector3(Random.Range(caveOffsetFromSides, map.tailleMap.x - sizeCave.x + 2 - caveOffsetFromSides),
            Random.Range(caveOffsetFromSides, map.tailleMap.y - sizeCave.y + 2 - caveOffsetFromSides),
            Random.Range(caveOffsetFromSides, map.tailleMap.z - sizeCave.z + 2 - caveOffsetFromSides));
    }
}
