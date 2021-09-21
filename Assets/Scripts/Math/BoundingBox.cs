using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoundingBox : CubeInt {

    public BoundingBox(Vector3Int min, Vector3Int max) : base(min, max - min) {
    }
}
