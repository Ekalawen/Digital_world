using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceBouncyCubesOnVerticalSpaces : GenerateCubesMapFunction {

    public Vector3Int minSpacesSize = new Vector3Int(3, 3 ,3);

    public override void Activate() {
        List<CubeInt> emptyCubes = GetMaxEmptySpacesLocations.Get(map.GetRegularCubes(), minSpacesSize);
        DisplayEmptySpaces(emptyCubes);
    }

    protected void DisplayEmptySpaces(List<CubeInt> emptyCubes) {
        foreach (CubeInt cube in emptyCubes) {
            List<ColorManager.Theme> theme = new List<ColorManager.Theme>() { ColorManager.GetRandomTheme() };
            PosVisualisator.CreateCube(cube, ColorManager.GetColor(theme));
            Debug.Log($"Cube = {cube}");
        }
    }
}
