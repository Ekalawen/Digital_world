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
    public int offsetFromSides = 0;

    public override void Activate() {
        List<CubeInt> emptyCubes = GetMaxEmptySpacesLocations.Get(map.GetRegularCubes(), minSpacesSize);
        List<CubeInt> mergedEmptyCubes = GetMaxEmptySpacesLocations.CombineMergedSpaces(emptyCubes, commonPercentageToMerge);
        DisplayEmptySpaces(mergedEmptyCubes);
        int nbBouncyCubesPlaced = 0;
        foreach(CubeInt mergedEmptyCube in mergedEmptyCubes) {
            int nbBouncyCubesAdded = PlaceBouncyCubesIn(mergedEmptyCube);
            nbBouncyCubesPlaced += nbBouncyCubesAdded;
        }
        Debug.Log($"{nbBouncyCubesPlaced} BoucyCubes ajoutés !");
    }

    public int PlaceBouncyCubesIn(CubeInt mergedEmptyCube) {
        Transform cubesFolder = new GameObject("PlaceBouncyCubesOnVerticalSpaces").transform;
        cubesFolder.SetParent(map.cubesFolder.transform);
        int nbHauteurs = mergedEmptyCube.height / hauteurEntreCubes;
        RectInt tranche = mergedEmptyCube.GetTranche();
        int nbCubesParTranches = mergedEmptyCube.areaTranche / nbCubesByAreaTranche + 1;
        int nbCubesAdded = 0;
        List<Vector2Int> positionsTaken = new List<Vector2Int>();
        for(int i = 0; i < nbHauteurs; i++) {
            for (int j = 0; j < nbCubesParTranches; j++) {
                Vector3Int pos = GenerateBoucyCubePosition(mergedEmptyCube, i, positionsTaken);
                Cube newCube = map.AddCube(pos, cubeType, parent: cubesFolder);
                if (newCube != null) {
                    positionsTaken.Add(new Vector2Int(pos.x, pos.z));
                    nbCubesAdded += 1;
                }
            }
        }
        return nbCubesAdded;
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
        foreach (CubeInt cube in emptyCubes) {
            List<ColorManager.Theme> theme = new List<ColorManager.Theme>() { ColorManager.GetRandomTheme() };
            PosVisualisator.DrawCube(cube, ColorManager.GetColor(theme));
            //Debug.Log($"Cube = {cube}");
        }
    }
}
