using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagerBlackAndWhite : ItemManager {

    protected bool shouldPopOnTop = false;

    public override void Initialize() {
        base.Initialize();
    }

    public override void PopItem(GameObject itemPrefab) {
        BlackAndWhiteMap map = (BlackAndWhiteMap)gm.map;
        Vector3 pos;
        while (true) {
            pos = map.GetFreeRoundedLocation();
            if ((shouldPopOnTop && map.IsInTopPart(pos))
             || (!shouldPopOnTop && !map.IsInTopPart(pos)))
                break;
        }
        shouldPopOnTop = !shouldPopOnTop;
        GenerateItemFromPrefab(itemPrefab, pos);
    }
}
