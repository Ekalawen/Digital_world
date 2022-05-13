using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHiddenTextureOnCubeInRegular : MapFunctionComponent {

    public Texture letterTexture;

    public override void Activate() {
        List<Cube> cubes = map.GetAllCubesOfType(Cube.CubeType.NORMAL);
        if(cubes.Count == 0) {
            Debug.LogError("Il devrait y avoir au moins un cube normal dans cette map !!! x)");
            return;
        }

        cubes = cubes.FindAll(c => map.GetVoisinsLibresInMap(c.transform.position).Count > 0);
        if(cubes.Count == 0) {
            Debug.LogError("Il devrait y avoir au moins un cube avec un côté libre dans cette map !!! x)");
            return;
        }

        Cube chosenCube = MathTools.ChoseOne(cubes);
        chosenCube.SetTexture(letterTexture);
        //chosenCube.SetColor(gm.colorManager.GetNotBlackColorForPosition(chosenCube.transform.position));
    }
}
