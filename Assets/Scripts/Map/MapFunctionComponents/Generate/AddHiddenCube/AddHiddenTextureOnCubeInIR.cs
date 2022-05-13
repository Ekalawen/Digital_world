using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHiddenTextureOnCubeInIR : MonoBehaviour {

    public Texture letterTexture;
    public Vector2Int blockIndiceRange = new Vector2Int(10, 20);

    protected GameManager gm;
    protected int chosenIndice;

    public void Initialize() {
        gm = GameManager.Instance;
        chosenIndice = MathTools.RandBetween(blockIndiceRange);
    }

    public void ApplyOn(Block newBlock, int blockIndice) {
        if(blockIndice != chosenIndice) {
            return;
        }

        List<Cube> cubes = newBlock.GetCubes().FindAll(c => c.type == Cube.CubeType.NORMAL);
        if(cubes.Count == 0) {
            chosenIndice += 1;
            return;
        }

        cubes = cubes.FindAll(c => gm.map.GetVoisinsLibresAll(c.transform.position).Count > 0);
        if(cubes.Count == 0) {
            chosenIndice += 1;
            return;
        }

        Cube chosenCube = MathTools.ChoseOne(cubes);
        chosenCube.SetTexture(letterTexture);
    }
}
