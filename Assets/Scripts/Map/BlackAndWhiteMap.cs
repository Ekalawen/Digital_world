using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackAndWhiteMap : PlainMap {

    public override Vector3 GetPlayerStartPosition() {
        while (true) {
            Vector3 pos = GetFreeRoundedLocation();
            if (IsInTopPart(pos))
                return pos;
        }
    }

    // /!\ Se base sur le fait que le milieu de la map est découpé par des cubes indestructibles !
    public bool IsInTopPart(Vector3 pos) {
        Ray ray = new Ray(pos, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach(RaycastHit hit in hits) {
            Cube cube = hit.collider.gameObject.GetComponent<Cube>();
            if(cube != null && cube.type == Cube.CubeType.INDESTRUCTIBLE) {
                return true;
            }
        }
        return false;
    }
}
