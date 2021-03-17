using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevelObject : MonoBehaviour {

    public SelectorLevel level;

    public SelectorLevelObjectCube cube;
    public GameObject collision;
    public SelectorLevelObjectTitle title;

    public void Initialize(bool highlighted, GameObject selectorIconePrefab) {
        cube.Initialize(highlighted, selectorIconePrefab);
        title.Initialize();
    }
}
