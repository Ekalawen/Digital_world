using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirLumiereLocalisation : IPouvoir {

	public GameObject trail; // Les trails à tracer quand le personnage est perdu !

    protected override void UsePouvoir() {
		if (Input.GetKeyDown (KeyCode.E)) {
            if (!player.CanUseLocalisation()) {
                gm.console.FailLocalisation();
                gm.soundManager.PlayFailActionClip();
                return;
            }

			// On trace les rayons ! =)
			GameObject[] lumieres = GameObject.FindGameObjectsWithTag ("Objectif");
			for (int i = 0; i < lumieres.Length; i++) {
				//Vector3 departRayons = transform.position - 0.5f * camera.transform.forward + 0.5f * Vector3.up;
				Vector3 departRayons = player.transform.position + 0.5f * Vector3.up;
				GameObject tr = Instantiate (trail, departRayons, Quaternion.identity) as GameObject;
				tr.GetComponent<Trail> ().setTarget (lumieres [i].transform.position);
			}

			// Un petit message
			gm.console.RunLocalisation();

			// Et on certifie qu'on a appuyé sur E
			gm.console.UpdateLastLumiereAttrapee();
		}
    }
}
