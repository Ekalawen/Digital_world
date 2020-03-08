using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirLumiereLocalisation : IPouvoir {

	public GameObject trailLumieresPrefab; // Les trails à tracer pour retrouver les lumières
	public GameObject trailItemsPrefab; // Les trails à tracer pour retrouver les items

    protected override bool UsePouvoir() {
		if (Input.GetKeyDown (KeyCode.E)) {
            if (!player.CanUseLocalisation()) {
                gm.console.FailLocalisation();
                gm.soundManager.PlayFailActionClip();
                return false;
            }

			// On trace les rayons ! =)
            List<Lumiere> lumieres = gm.map.GetLumieres();
			for (int i = 0; i < lumieres.Count; i++) {
                Vector3 departRayons = player.transform.position + 0.5f * gm.gravityManager.Up();
                Vector3 derriere = player.transform.position - player.camera.transform.forward.normalized;
                Vector3 devant = player.transform.position + player.camera.transform.forward.normalized;
                Vector3 target = lumieres[i].transform.position;
				GameObject tr = Instantiate (trailLumieresPrefab, derriere, Quaternion.identity) as GameObject;
                tr.GetComponent<Trail>().SetTarget(lumieres[i].transform.position);
			}

            // On trace les rayons des items
            List<Item> items = gm.itemManager.GetItems();
			for (int i = 0; i < items.Count; i++) {
                Vector3 departRayons = player.transform.position + 0.5f * gm.gravityManager.Up();
                Vector3 derriere = player.transform.position - player.camera.transform.forward.normalized;
                Vector3 devant = player.transform.position + player.camera.transform.forward.normalized;
                Vector3 target = items[i].transform.position;
				GameObject tr = Instantiate (trailItemsPrefab, derriere, Quaternion.identity) as GameObject;
                tr.GetComponent<Trail>().SetTarget(items[i].transform.position);
			}

			// Un petit message
			gm.console.RunLocalisation();

			// Et on certifie qu'on a appuyé sur E
			gm.console.UpdateLastLumiereAttrapee();
		}

        return true;
    }
}
