using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDetection : IPouvoir {

    public bool canDetectLumieres = true;
    public bool canDetectItems = true;
	public GameObject trailLumieresPrefab; // Les trails à tracer pour retrouver les lumières
	public GameObject trailItemsPrefab; // Les trails à tracer pour retrouver les items

    protected bool atLeastOneRay;

    protected override bool UsePouvoir() {
        if (!player.CanUseLocalisation()) {
            gm.console.FailLocalisation();
            gm.soundManager.PlayFailActionClip();
            return false;
        }

        atLeastOneRay = false;
        int nbLumieres = 0;
        int nbItems = 0;

        if (canDetectLumieres) {
            nbLumieres = DrawLumieresRays();
        }

        if (canDetectItems) {
            nbItems = DrawItemsRays();
            NotifyOnlyVisibleOnTriggerItems();
        }

        gm.console.RunLocalisation(nbLumieres, nbItems);

        gm.console.UpdateLastLumiereAttrapee();

        return atLeastOneRay;
    }

    protected int DrawLumieresRays() {
        // On trace les rayons ! =)
        List<Lumiere> lumieres = GetLumieresToLocate();
        for (int i = 0; i < lumieres.Count; i++) {
            Vector3 departRayons = player.transform.position + 0.5f * gm.gravityManager.Up();
            Vector3 derriere = player.transform.position - player.camera.transform.forward.normalized;
            Vector3 devant = player.transform.position + player.camera.transform.forward.normalized;
            Vector3 target = lumieres[i].transform.position;
            GameObject tr = Instantiate (trailLumieresPrefab, derriere, Quaternion.identity) as GameObject;
            tr.GetComponent<Trail>().SetTarget(lumieres[i].transform.position);
            atLeastOneRay = true;
        }
        return lumieres.Count;
    }

    protected int DrawItemsRays() {
        // On trace les rayons des items
        List<Item> items = gm.itemManager.GetItems();
        for (int i = 0; i < items.Count; i++) {
            Vector3 departRayons = player.transform.position + 0.5f * gm.gravityManager.Up();
            Vector3 derriere = player.transform.position - player.camera.transform.forward.normalized;
            Vector3 devant = player.transform.position + player.camera.transform.forward.normalized;
            Vector3 target = items[i].transform.position;
            GameObject tr = Instantiate (trailItemsPrefab, derriere, Quaternion.identity) as GameObject;
            tr.GetComponent<Trail>().SetTarget(items[i].transform.position);
            atLeastOneRay = true;
        }
        return items.Count;
    }

    protected void NotifyOnlyVisibleOnTriggerItems() {
        foreach(Item item in gm.itemManager.GetItems()) {
            OnlyVisibleOnTrigger component = item.GetComponent<OnlyVisibleOnTrigger>();
            if(component != null && component.enabled) {
                component.Activate();
            }
        }
    }

    protected virtual List<Lumiere> GetLumieresToLocate() {
        List<Lumiere> res = new List<Lumiere>();
        foreach (Lumiere lumiere in gm.map.GetLumieres()) res.Add(lumiere);
        for(int i = 0; i < res.Count; i++) {
            LumiereSwitchable ls = res[i].GetComponent<LumiereSwitchable>();
            if(ls != null && ls.GetState() == LumiereSwitchable.LumiereSwitchableState.OFF) {
                res.RemoveAt(i);
                i--;
            }
        }
        return res;
    }
}
