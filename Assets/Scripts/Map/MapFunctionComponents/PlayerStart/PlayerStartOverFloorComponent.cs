using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartOverFloorComponent : PlayerStartComponent {

    protected int kmax = 1000;

    public override Vector3 GetPlayerStartPosition() {
        int k = 0;
        while (true) {
            Vector3 pos = map.GetFreeRoundedLocation();
            if (IsOverFloor(pos) || k > kmax) {
                if (k > kmax)
                    Debug.LogWarning("On a pas trouvé de position au desssu du sol !");
                return pos;
            }
            k++;
        }
    }

    public bool IsOverFloor(Vector3 pos) {
        pos = MathTools.Round(pos);
        for(int i = 1; i <= pos.y; i++) {
            Cube cube = map.GetCubeAt(pos - i * Vector3.down);
            if (cube != null)
                return true;
        }
        return false;
    }

}
