using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkUnreachableLumiereToRest : MapFunctionComponent {
    public override void Activate() {
        LinkAllUnreachableLumiereToRest();
    }

    public void LinkAllUnreachableLumiereToRest() {
        List<Vector3> reachableArea = map.GetReachableArea();

        // Vérifier si les lumières sont dans cette zone, si elles ne le sont pas, elles sont inaccessibles
        foreach(Lumiere lumiere in map.GetLumieres()) {
            if (!MathTools.IsRounded(lumiere.transform.position)) {
                Debug.LogWarning("Attention une lumière n'est pas à une position entière ! Peut engendrer des bugs dans le Link !");
            }
            map.LinkPositionToReachableArea(MathTools.Round(lumiere.transform.position), reachableArea);
        }
    }
}
