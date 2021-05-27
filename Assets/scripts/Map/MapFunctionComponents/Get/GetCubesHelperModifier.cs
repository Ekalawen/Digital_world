using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GetCubesHelperModifier : MonoBehaviour {

    public abstract List<Cube> Modify(List<Cube> cubes);
}
