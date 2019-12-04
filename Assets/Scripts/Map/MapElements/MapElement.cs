using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapElement  {

    protected MapManager map;

    protected MapElement() {
        map = GameObject.FindObjectOfType<MapManager>();
        map.mapElements.Add(this);
    }

    public abstract void OnDeleteCube(Cube cube);
}
