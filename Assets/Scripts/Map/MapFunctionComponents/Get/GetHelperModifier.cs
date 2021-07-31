using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GetHelperModifier : MonoBehaviour {

    public virtual List<Cube> ModifyCubes(List<Cube> cubes) {
        return cubes.FindAll(c => IsInArea(c.transform.position));
    }

    public virtual List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return positions.FindAll(p => IsInArea(p));
    }

    public abstract bool IsInArea(Vector3 position);
}
