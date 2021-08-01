using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagerBlackAndWhite : ItemManager {

    public float minDistanceFromSurface = 3.0f;

    protected bool shouldPopOnTop = false;

    public override void Initialize() {
        base.Initialize();
    }

    public override Item PopItem(GameObject itemPrefab) {
        BlackAndWhiteMap map = (BlackAndWhiteMap)gm.map;
        List<Cube> surface = map.GetAllCubesOfType(Cube.CubeType.INDESTRUCTIBLE);
        Vector3 pos;
        while (true) {
            pos = map.GetFarFromEnsemble(surface, minDistanceFromSurface);
            if ((shouldPopOnTop && map.IsInTopPart(pos))
             || (!shouldPopOnTop && !map.IsInTopPart(pos)))
                break;
        }
        shouldPopOnTop = !shouldPopOnTop;
        return GenerateItemFromPrefab(itemPrefab, pos);
    }
}
