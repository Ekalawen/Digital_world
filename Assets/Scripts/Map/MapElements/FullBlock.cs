using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBlock : Cave {

    public FullBlock(Vector3 depart, Vector3Int nbCubesParAxe, bool makeSpaceArround = false) :
        base(depart, nbCubesParAxe, makeSpaceArround, bDigInside: false) {
    }

    public override string GetName() {
        return "FullBlock";
    }
}
