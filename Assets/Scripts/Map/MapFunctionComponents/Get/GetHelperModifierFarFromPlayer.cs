using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierFarFromPlayer : GetHelperModifier {

    public enum DistanceType { L2, CUBE, L_INFINI };

    public float minDistance = 3.0f;
    public DistanceType distanceType = DistanceType.L2;

    protected Player player;

    public override bool IsInArea(Vector3 position) {
        Vector3 center = GetPlayerPosition();
        return IfFarEnought(center, position);
    }

    protected Vector3 GetPlayerPosition() {
        if(player == null) {
            player = GameManager.Instance.player;
        }
        return player.transform.position;
    }

    protected bool IfFarEnought(Vector3 pos1, Vector3 pos2) {
        switch (distanceType) {
            case DistanceType.L2:
                return Vector3.SqrMagnitude(pos1 - pos2) >= minDistance * minDistance;
            case DistanceType.CUBE:
                return MathTools.CubeDistance(pos1, pos2) >= minDistance;
            case DistanceType.L_INFINI:
                return MathTools.DistanceLInfini(pos1, pos2) >= minDistance;
            default:
                Debug.LogError("Mauvais type de distance dans GetHelperModifierFarFromPlayer !! :p", this);
                return false;
        }
    }
}
