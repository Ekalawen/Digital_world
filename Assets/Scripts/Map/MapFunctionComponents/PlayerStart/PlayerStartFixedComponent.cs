using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartFixedComponent : PlayerStartComponent {

    public Transform start;

    public override Vector3 GetPlayerStartPosition() {
        return start.position;
    }

    public override Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        Vector3 direction = Vector3.ProjectOnPlane(start.forward, Vector3.up).normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        return new Vector2(90, angle);
    }
}
