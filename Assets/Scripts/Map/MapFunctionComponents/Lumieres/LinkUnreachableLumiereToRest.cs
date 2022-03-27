using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkUnreachableLumiereToRest : MapFunctionComponent {

    public bool onlyCheckInsideMapLumieres = false; // Because the reachableArea of the LinkPositionToReachableArea is only inside the map! So if a Data is outside, it won't be accessible, and this will make a hole in the map :)

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
            if (ShouldLinkLumiere(lumiere)) {
                map.LinkPositionToReachableArea(MathTools.Round(lumiere.transform.position), reachableArea);
            }
        }
    }

    public bool ShouldLinkLumiere(Lumiere lumiere) {
        return lumiere.IsAccessible() && (!onlyCheckInsideMapLumieres || map.IsInRegularMap(lumiere.transform.position));
    }
}
