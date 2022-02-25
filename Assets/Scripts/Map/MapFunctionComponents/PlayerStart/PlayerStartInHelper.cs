using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartInHelper : PlayerStartComponent {

    public GetEmptyPositionsHelper getHelper;
    public bool lookAtSpecificPosition = false;
    [ConditionalHide("lookAtSpecificPosition")]
    public Vector3 specificPosition;

    public override Vector3 GetRawPlayerStartPosition() {
        return MathTools.ChoseOne(getHelper.Get());
    }

    public override Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        if (!lookAtSpecificPosition) {
            return base.GetPlayerStartOrientationXY(playerStartPosition);
        }
        Vector3 direction = Vector3.ProjectOnPlane(specificPosition - playerStartPosition, Vector3.up).normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        return new Vector2(0, angle);
    }
}
