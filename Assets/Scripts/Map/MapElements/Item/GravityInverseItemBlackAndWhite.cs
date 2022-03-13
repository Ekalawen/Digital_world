using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityInverseItemBlackAndWhite : GravityInverseItem {

    public float addedTime = 35.0f;

    protected override void Start() {
        base.Start();
    }

    public override void OnTrigger(Collider hit) {
        base.OnTrigger(hit);

        // Changer les deathcubes en specialcubes !
        DestroyDeathCubes();

        // Cancel toutes les corruptions !
        CancelAllCorruptions();

        // Changer les cubes normaux en cubes speciaux !
        ChangeNormalsInScpecials();

        // Change all colors !
        ChangeColor();

        // Change all colors !
        AddTime();
    }

    protected void ChangeColor() {
        // On indique qu'il faut changer de couleurs !
        ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
        colorManager.SwapTheme();

        // Change les couleurs du random filling
        gm.colorManager.ChangeTheme(colorManager.GetCurrentTheme());

        //// Change les couleurs de la surface interpolante !
        //List<Cube> cubes = gm.map.GetAllCubesOfType(Cube.CubeType.INDESTRUCTIBLE);
        //foreach (Cube cube in cubes) {
        //    cube.SetColor(colorManager.GetCurrentColor());
        //}

        // Change les couleurs des corrupted cubes !
        List<Cube> cubes = gm.map.GetAllCubesOfType(Cube.CubeType.SPECIAL);
        foreach (Cube cube in cubes) {
            cube.SetColor(ColorManager.GetColor(colorManager.GetCurrentTheme()));
        }
    }

    protected void DestroyDeathCubes() {
        List<Cube> cubes = gm.map.GetAllCubes();
        foreach (Cube cube in cubes) {
            if (cube.type == Cube.CubeType.DEATH) {
                Vector3 pos = cube.transform.position;
                cube.Destroy();
            }
        }
    }

    protected void LinkEverything() {
        // Link les lumières
        gm.map.LinkUnreachableLumiereToRest();

        // Link les items
        List<Vector3> reachableArea = gm.map.GetReachableArea();
        foreach(Item item in gm.itemManager.GetItems()) {
            gm.map.LinkPositionToReachableArea(item.transform.position, reachableArea);
        }
    }

    protected void CancelAllCorruptions() {
        foreach(Cube cube in gm.map.GetAllCubesOfType(Cube.CubeType.SPECIAL)) {
            CorruptedCubeOld corruptedCube = (CorruptedCubeOld)cube;
            corruptedCube.CancelCorruption();
        }
    }

    protected void ChangeNormalsInScpecials() {
        foreach(Cube cube in gm.map.GetAllCubesOfType(Cube.CubeType.NORMAL)) {
            Vector3 pos = cube.transform.position;
            cube.Destroy();
            Cube newCube = gm.map.AddCube(pos, Cube.CubeType.SPECIAL);
            ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
            newCube.SetColor(ColorManager.GetColor(colorManager.GetCurrentTheme()));
        }
    }

    protected void AddTime() {
        gm.timerManager.AddTime(addedTime);
    }
}
