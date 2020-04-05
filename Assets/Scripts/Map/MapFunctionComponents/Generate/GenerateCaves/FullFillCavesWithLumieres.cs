using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFillCavesWithLumieres : MapFunctionComponent {

    public Lumiere.LumiereType lumiereType;

    public override void Activate() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        foreach (Cave cave in caves)
            cave.AddAllLumiereInside(lumiereType);
    }
}
