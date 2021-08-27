using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartJustOverFloorComponent : PlayerStartComponent {

    public bool onlyOnFirstFloor = true;
    public bool lookAtHeight = true;
    [ConditionalHide("lookAtHeight")]
    public float heightPercentage = 0.5f;

    public override Vector3 GetRawPlayerStartPosition() {
        int kmax = 1000;
        for(int k = 0; k < kmax; k++) {
            Vector3 pos = map.GetFreeRoundedLocation();
            if (IsJustOverFloor(pos)) {
                return pos + 0.25f * Vector3.down;
            }
        }
        Debug.LogWarning("On a pas trouvé de position juste au desssu du sol !");
        return map.GetFreeRoundedLocation();
    }

    public bool IsJustOverFloor(Vector3 pos) {
        pos = MathTools.Round(pos);
        bool isJustOver = map.IsEnabledCubeAt(pos + Vector3.down);
        bool isOnFirstFloor = pos.y == 1;
        return isJustOver && (!onlyOnFirstFloor || isOnFirstFloor);
    }

    public override Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition)
    {
        Vector2 orientation = base.GetPlayerStartOrientationXY(playerStartPosition);
        float verticalAngle = AdjustHeightOfLook(playerStartPosition);
        return new Vector2(verticalAngle, orientation.y);
    }

    private float AdjustHeightOfLook(Vector3 playerStartPosition) {
        if(!lookAtHeight) {
            return 0;
        }
        Vector3 mapCenter = gm.map.GetCenter() * heightPercentage * 2;
        Vector3 playerToCenter = mapCenter - playerStartPosition;
        Vector3 playerToCenterHorizontal = Vector3.ProjectOnPlane(playerToCenter, Vector3.up);
        Vector3 cross = Vector3.Cross(playerToCenterHorizontal, playerToCenter);
        float verticalAngle = Vector3.SignedAngle(playerToCenter, playerToCenterHorizontal, cross);
        return verticalAngle;
    }
}
