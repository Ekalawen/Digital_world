using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceBouncyCubesOnVerticalSpaces : GenerateCubesMapFunction {

    [Header("LocateEmptySpaces")]
    public Vector3Int minSpacesSize = new Vector3Int(3, 5 ,3);
    public float commonPercentageToMerge = 0.8f;

    [Header("PlacesBouncyCubes")]
    public int nbCubesByAreaTranche = 28;
    public int hauteurEntreCubes = 4;
    public int hauteurFreeFromBounceCubesAbove = 6;
    public int offsetFromSides = 0;
    public bool removeAllCubesAbove = false;

    protected List<Cube> bouncyCubesPlaced = new List<Cube>();

    public override void Activate() {
        List<CubeInt> emptyCubes = GetMaxEmptySpacesLocations.Get(map.GetRegularCubes(), minSpacesSize);
        List<CubeInt> mergedEmptyCubes = GetMaxEmptySpacesLocations.CombineMergedSpaces(emptyCubes, commonPercentageToMerge);
        DisplayEmptySpaces(mergedEmptyCubes);

        List<Cube> bouncyCubesPlaced = new List<Cube>();
        foreach(CubeInt mergedEmptyCube in mergedEmptyCubes) {
            bouncyCubesPlaced.AddRange(PlaceBouncyCubesIn(mergedEmptyCube));
        }

        bouncyCubesPlaced = RemoveBouncyCubesAboveOthers(bouncyCubesPlaced);

        RemoveCubesAboveBouncyCubes(bouncyCubesPlaced);

        Debug.Log($"{bouncyCubesPlaced.Count} BoucyCubes ajoutés !");
    }

    public List<Cube> PlaceBouncyCubesIn(CubeInt mergedEmptyCube) {
        Transform cubesFolder = new GameObject("PlaceBouncyCubesOnVerticalSpaces").transform;
        cubesFolder.SetParent(map.cubesFolder.transform);
        int nbHauteurs = Mathf.Max(mergedEmptyCube.height / hauteurEntreCubes, 1);
        //RectInt tranche = mergedEmptyCube.GetTranche();
        int nbCubesParTranches = mergedEmptyCube.areaTranche / nbCubesByAreaTranche + 1;
        List<Vector2Int> positionsTaken = new List<Vector2Int>();
        List<Cube> cubesAdded = new List<Cube>();
        for(int i = 0; i < nbHauteurs; i++) {
            for (int j = 0; j < nbCubesParTranches; j++) {
                Vector3Int pos = GenerateBoucyCubePosition(mergedEmptyCube, i, positionsTaken);
                Cube newCube = map.AddCube(pos, cubeType, parent: cubesFolder);
                if (newCube != null) {
                    positionsTaken.Add(new Vector2Int(pos.x, pos.z));
                    cubesAdded.Add(newCube);
                } else {
                    Debug.Log($"Fail to create bouncy cube at position {pos}");
                }
            }
        }
        return cubesAdded;
    }

    private Vector3Int GenerateBoucyCubePosition(CubeInt mergedEmptyCube, int indiceHauteur, List<Vector2Int> positionsTaken) {
        Vector3Int pos = new Vector3Int();
        for (int k = 0; k < 100; k++) {
            int offsetHauteur = indiceHauteur * hauteurEntreCubes;
            int x = mergedEmptyCube.xMin + UnityEngine.Random.Range(offsetFromSides, mergedEmptyCube.width - offsetFromSides);
            int y = mergedEmptyCube.yMin + offsetHauteur;
            int z = mergedEmptyCube.zMin + UnityEngine.Random.Range(offsetFromSides, mergedEmptyCube.depth - offsetFromSides);
            pos = new Vector3Int(x, y, z);
            Vector2Int posHorizontal = new Vector2Int(x, z);
            if (!positionsTaken.Contains(posHorizontal)) {
                return pos;
            }
        }
        return pos;
    }

    protected void DisplayEmptySpaces(List<CubeInt> emptyCubes) {
        Debug.Log($"Il y a {emptyCubes.Count} emptyCubes !");
        foreach (CubeInt cube in emptyCubes) {
            List<ColorManager.Theme> theme = new List<ColorManager.Theme>() { ColorManager.GetRandomTheme() };
            PosVisualisator.DrawCube(cube, ColorManager.GetColor(theme));
        }
    }

    protected List<Cube> RemoveBouncyCubesAboveOthers(List<Cube> bouncyCubes) {
        List<Cube> cubes = bouncyCubes.OrderBy(c => c.transform.position.y).ToList(); // Need to sort if they are 3 cubes on top of each other !
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube1 = cubes[i];
            Vector3 pos1 = cube1.transform.position;
            for(int j = i + 1; j < cubes.Count; j++) {
                Cube cube2 = cubes[j];
                Vector3 pos2 = cube2.transform.position;
                if(pos1.x == pos2.x && pos1.z == pos2.z
                && pos2.y <= pos1.y + hauteurFreeFromBounceCubesAbove) {
                    map.DeleteCube(cube2);
                    cubes.RemoveAt(j);
                    j--;
                    Debug.Log($"Remove {pos2} à cause de {pos1}");
                }
            }
        }
        return cubes;
    }

    protected void RemoveCubesAboveBouncyCubes(List<Cube> bouncyCubes) {
        if (!removeAllCubesAbove)
            return;
        foreach(Cube bouncyCube in bouncyCubes) {
            float halfHeight = (float)hauteurFreeFromBounceCubesAbove / 2;
            Vector3 center = bouncyCube.transform.position + Vector3.up * halfHeight;
            Vector3 halfExtents = new Vector3(0.5f, halfHeight - 0.5f, 0.5f);
            List<Cube> aboveCubes = map.GetCubesInBox(center, halfExtents);
            foreach(Cube aboveCube in aboveCubes) {
                Debug.Log($"Remove {aboveCube} à car il était au-dessus de {bouncyCube}");
                map.DeleteCube(aboveCube);
            }
        }
    }
}
