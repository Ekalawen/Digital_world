using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using System;
using UnityEngine.Assertions;

public class GenerateRandomFillingModifier_SetOneMainBuilderCube : GenerateRandomFillingModifier {
    public bool setOneBuilderCubeMain = true;
    public bool setLinky = true;

    public override void InitializeSpecific(GenerateRandomFilling generateRandomFilling) {
        base.InitializeSpecific(generateRandomFilling);
        Assert.AreEqual(generateRandomFilling.cubeType, Cube.CubeType.BUILDER);
    }

    public override FullBlock ModifyFullBlock(FullBlock fullBlock) {
        if(setOneBuilderCubeMain) {
            SetOneMainBuilderCube(fullBlock);
        }
        if(setLinky) {
            SetAllCubesLinky(fullBlock);
        }
        return fullBlock;
    }

    protected void SetAllCubesLinky(FullBlock fullBlock) {
        foreach(Cube cube in fullBlock.GetCubes()) {
            cube.SetLinky();
        }
    }

    protected void SetOneMainBuilderCube(FullBlock fullBlock) {
        List<Cube> cubes = fullBlock.GetCubes();
        if(cubes.Count == 0) {
            return;
        }
        BuilderCube mainBuilderCube = MathTools.ChoseOne(cubes).GetComponent<BuilderCube>();
        mainBuilderCube.useCustomClusters = false;
    }
}
