using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBlock : Cave {

    public FullBlock(Vector3 depart, Vector3Int nbCubesParAxe, bool bMakeSpaceArround = false) :
        base(depart, nbCubesParAxe, bMakeSpaceArround, bDigInside: false) {
    }

    public override string GetName() {
        return "FullBlock";
    }
}
