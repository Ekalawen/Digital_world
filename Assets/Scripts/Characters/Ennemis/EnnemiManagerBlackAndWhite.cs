using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiManagerBlackAndWhite : EnnemiManager {
    protected bool nextPopOnTop = true;

    public override Ennemi PopEnnemi(GameObject ennemiPrefab) {
        BlackAndWhiteMap map = (BlackAndWhiteMap)gm.map;
        Vector3 pos;
        while (true) {
            pos = map.GetFreeRoundedLocation();
            if ((nextPopOnTop && map.IsInTopPart(pos))
             || (!nextPopOnTop && !map.IsInTopPart(pos)))
                break;
        }
        nextPopOnTop = !nextPopOnTop;
        return GenerateEnnemiFromPrefab(ennemiPrefab, pos);
    }
}
