using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirGiverItem : Item {

    public enum PouvoirBinding { A, E, LEFT_CLICK, RIGHT_CLICK };

    public GameObject pouvoirPrefab;
    public PouvoirBinding pouvoirBinding;

    public override void OnTrigger(Collider hit) {
        gm.player.SetPouvoir(pouvoirPrefab, pouvoirBinding);
        IPouvoir pouvoir = pouvoirPrefab.GetComponent<IPouvoir>();
        gm.console.CapturePouvoirGiverItem(pouvoir.nom, pouvoirBinding);
    }
}
