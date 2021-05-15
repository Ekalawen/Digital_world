using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateCaves : GenerateCubesMapFunction {

    [Header("Generating Mode")]
    public GenerateMode generateMode = GenerateMode.PROPORTION;
    [ConditionalHide("generateMode", GenerateMode.PROPORTION)]
	public float proportionCaves = 0.3f;
    [ConditionalHide("generateMode", GenerateMode.COUNT)]
    public int nbCaves = 0;

    [Header("Parameters")]
    public int tailleMinCave = 3;
    public int tailleMaxCave = 10;
    public bool makeSpaceArround = false;
    public bool dontCollideWithOthersCaves = false;
    [ConditionalHide("dontCollideWithOthersCaves")]
    public int distanceWithOthersCaves = 0;
    [ConditionalHide("makeSpaceArround")]
    public bool preserveMapBordure = false;
    public int nbLumieresPerCaves = 1;
    public int offsetLumieresFromCenter = 1;
    public Vector2Int caveOffsetFromSidesXZandY = new Vector2Int(2, 2);

    [Header("Fixed offsets")]
    public bool useCaveFixedOffset = false;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetX = false;
    [ConditionalHide("useCaveFixedOffsetX")]
    public int caveFixedOffsetX;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetY = false;
    [ConditionalHide("useCaveFixedOffsetY")]
    public int caveFixedOffsetY;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetZ = false;
    [ConditionalHide("useCaveFixedOffsetZ")]
    public int caveFixedOffsetZ;

    public override void Activate() {
        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        GenerateAllCaves();
    }

	protected List<Cave> GenerateAllCaves() {
        List<Cave> caves = null;
        if (generateMode == GenerateMode.PROPORTION)
            caves = GenerateAllCavesWithProportion();
        else if (generateMode == GenerateMode.COUNT)
            caves = GenerateAllCavesWithNbCaves();
        AddLumieresInAllCaves(caves);

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

    protected void AddLumieresInAllCaves(List<Cave> caves) {
        // Il faut une deuxième passe, sinon les premières lumières seront écrasés par les caves suivantes !
        foreach(Cave cave in caves) {
            cave.AddNLumiereInside(nbLumieresPerCaves, offsetLumieresFromCenter);
        }
    }

    protected virtual Cave GenerateCave() {
        // On définit la taille de la cave
        Vector3Int size = Vector3Int.zero;
        size.x = UnityEngine.Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.y = UnityEngine.Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.z = UnityEngine.Random.Range(tailleMinCave, tailleMaxCave + 1);

        // On définit sa position sur la carte
        Vector3 position = GetPositionCave(size, caveOffsetFromSidesXZandY);

        Cave cave = new Cave(position, size, makeSpaceArround, bDigInside: true, preserveMapBordure);

        return cave;
    }

    protected virtual Vector3 GetPositionCave(Vector3Int sizeCave, Vector2Int caveOffsetFromSidesXZandY)
    {
        Vector3 position;
        if(!dontCollideWithOthersCaves)
            position = GetPositionCaveWithoutCollisions(sizeCave, caveOffsetFromSidesXZandY);
        else
            position = GetPositionCaveWithCollisions(sizeCave, caveOffsetFromSidesXZandY);
        return position;
    }

    protected Vector3 GetPositionCaveWithCollisions(Vector3Int sizeCave, Vector2Int caveOffsetFromSidesXZandY) {
        List<Cave> othersCaves = map.GetMapElementsOfType<Cave>();
        int nbTriesMax = 1000;
        for(int nbTries = 0; nbTries < nbTriesMax; nbTries++) {
            Vector3 position = GetPositionCaveWithoutCollisions(sizeCave, caveOffsetFromSidesXZandY);
            if (CaveDontCollideWithOthersCaves(position, sizeCave, othersCaves))
                return position;
        }
        Debug.LogWarning("On a pas réussi à trouver une position qui ne rentre en collision avec aucun autre caves dans GetPositionCaveWithCollisions() !");
        return GetPositionCaveWithoutCollisions(sizeCave, caveOffsetFromSidesXZandY);
    }

    protected bool CaveDontCollideWithOthersCaves(Vector3 position, Vector3Int sizeCave, List<Cave> othersCaves) {
        Vector3 center = position + (Vector3)sizeCave / 2;
        Vector3 halfExtents = (Vector3)sizeCave / 2 + Vector3.one * distanceWithOthersCaves;
        foreach(Cave otherCave in othersCaves) {
            if (MathTools.AABB_AABB(center, halfExtents, otherCave.GetCenter(), otherCave.GetHalfExtents()))
                return false;
        }
        return true;
    }

    private Vector3 GetPositionCaveWithoutCollisions(Vector3Int sizeCave, Vector2Int caveOffsetFromSidesXZandY) {
        Vector3 position = Vector3.zero;
        position.x = useCaveFixedOffset && useCaveFixedOffsetX ? caveFixedOffsetX :
            UnityEngine.Random.Range(caveOffsetFromSidesXZandY.x, map.tailleMap.x - sizeCave.x + 2 - caveOffsetFromSidesXZandY.x);
        position.y = useCaveFixedOffset && useCaveFixedOffsetY ? caveFixedOffsetY :
            UnityEngine.Random.Range(caveOffsetFromSidesXZandY.y, map.tailleMap.y - sizeCave.y + 2 - caveOffsetFromSidesXZandY.y);
        position.z = useCaveFixedOffset && useCaveFixedOffsetZ ? caveFixedOffsetZ :
            UnityEngine.Random.Range(caveOffsetFromSidesXZandY.x, map.tailleMap.z - sizeCave.z + 2 - caveOffsetFromSidesXZandY.x);
        return position;
    }
}

