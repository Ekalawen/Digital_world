using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartOverFloorComponent : PlayerStartComponent {

    public override Vector3 GetPlayerStartPosition() {
        int kmax = 1000;
        for(int k = 0; k < kmax; k++) {
            Vector3 pos = map.GetFreeRoundedLocation();
            if (IsOverFloor(pos)) {
                return pos;
            }
        }
        Debug.LogWarning("On a pas trouvé de position au desssu du sol !");
        return map.GetFreeRoundedLocation();
    }

    public bool IsOverFloor(Vector3 pos) {
        pos = MathTools.Round(pos);
        for(int i = 1; i <= pos.y; i++) {
            if(map.IsEnabledCubeAt(pos + i * Vector3.down)) {
                return true;
            }
        }
        return false;
    }
}
