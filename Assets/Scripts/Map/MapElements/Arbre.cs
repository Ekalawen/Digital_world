using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arbre : CubeEnsemble {
    public override string GetName() {
        return "Arbre";
    }
    public override void OnDeleteCube(Cube cube) {
    }
}
