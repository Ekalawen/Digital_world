using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirGiverItem : Item {

    public enum PouvoirBinding { A, E, LEFT_CLICK, RIGHT_CLICK };

    public GameObject pouvoirPrefab;
    public PouvoirBinding pouvoirBinding;

    public override void OnTrigger(Collider hit) {
        GivePouvoir(gm, pouvoirPrefab, pouvoirBinding);
    }

    public static void GivePouvoir(GameManager gm, GameObject pouvoirPrefab, PouvoirBinding pouvoirBinding) {
        gm.player.SetPouvoir(pouvoirPrefab, pouvoirBinding);
        if(pouvoirPrefab != null) {
            IPouvoir pouvoir = pouvoirPrefab.GetComponent<IPouvoir>();
            gm.console.CapturePouvoirGiverItem(pouvoir.nom, pouvoirBinding);
            gm.soundManager.PlayGetItemClip(gm.player.transform.position);
            gm.pointeur.Initialize();
        }
    }
}
