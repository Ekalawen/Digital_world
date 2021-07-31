using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GetHelperModifier : MonoBehaviour {

    public abstract List<Cube> ModifyCubes(List<Cube> cubes);
    public abstract List<Vector3> ModifyEmpties(List<Vector3> positions);
}
