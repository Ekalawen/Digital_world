using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartComponent : MonoBehaviour {

    protected GameManager gm;
    protected MapManager map;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
    }


    public virtual Vector3 GetPlayerStartPosition() {
        return map.GetFreeRoundedLocation();
    }

    public virtual Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        Vector3 direction = Vector3.ProjectOnPlane((map.GetCenter() - playerStartPosition), Vector3.up).normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        return new Vector2(90, angle);
    }
}
