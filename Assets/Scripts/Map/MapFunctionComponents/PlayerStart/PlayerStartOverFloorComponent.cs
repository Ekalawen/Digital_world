using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartOverFloorComponent : PlayerStartComponent {

    public bool ofSpecificCubeType = false;
    [ConditionalHide("ofSpecificCubeType")]
    public Cube.CubeType specificCubeType = Cube.CubeType.NORMAL;

    public override Vector3 GetRawPlayerStartPosition() {
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
            Vector3 currentPos = pos + i * Vector3.down;
            if (!ofSpecificCubeType) {
                if (map.IsEnabledCubeAt(currentPos)) {
                    return true;
                }
            } else {
                Cube cube = map.GetCubeAt(currentPos);
                if (cube != null && cube.IsEnabled() && cube.type == specificCubeType) {
                    return true;
                }
            }
        }
        return false;
    }
}
