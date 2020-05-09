using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TracerNoObstacleController : TracerController {
    protected override void ComputePath(Vector3 end) {
        Vector3 start = MathTools.Round(transform.position);
        end = MathTools.Round(end);
        path = gm.map.GetNoObstaclePath(start, end, bIsRandom: true);
        if(path == null) {
            SetState(TracerState.EMITING);
        }
    }

    // Permet de savoir si l'ennemi voit le joueur
    public override bool IsPlayerVisible() {
        return true;
    }
}
