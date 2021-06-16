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


    public Vector3 GetPlayerStartPosition() {
        int kmax = 1000;
        for (int k = 0; k < kmax; k++) {
            Vector3 playerStartPosition = GetRawPlayerStartPosition();
            if(!gm.ennemiManager.IsInCaptureRange(playerStartPosition)) {
                return playerStartPosition;
            }
        }
        return GetRawPlayerStartPosition();
    }

    public virtual Vector3 GetRawPlayerStartPosition() {
        return map.GetFreeRoundedLocation();
    }

    public virtual Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        Vector3 direction = Vector3.ProjectOnPlane((map.GetCenter() - playerStartPosition), Vector3.up).normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        return new Vector2(0, angle);
    }
}
