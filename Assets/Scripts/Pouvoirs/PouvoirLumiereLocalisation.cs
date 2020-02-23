using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirLumiereLocalisation : IPouvoir {

	public GameObject trailPrefab; // Les trails à tracer quand le personnage est perdu !

    protected override bool UsePouvoir() {
		if (Input.GetKeyDown (KeyCode.E)) {
            if (!player.CanUseLocalisation()) {
                gm.console.FailLocalisation();
                gm.soundManager.PlayFailActionClip();
                return false;
            }

			// On trace les rayons ! =)
			GameObject[] lumieres = GameObject.FindGameObjectsWithTag ("Objectif");
			for (int i = 0; i < lumieres.Length; i++) {
                //Vector3 departRayons = transform.position - 0.5f * camera.transform.forward + 0.5f * Vector3.up;
                Vector3 departRayons = player.transform.position + 0.5f * gm.gravityManager.Up();
                Vector3 derriere = player.transform.position - player.camera.transform.forward.normalized;
                Vector3 devant = player.transform.position + player.camera.transform.forward.normalized;
                Vector3 target = lumieres[i].transform.position;
				GameObject tr = Instantiate (trailPrefab, derriere, Quaternion.identity) as GameObject;
                tr.GetComponent<Trail>().SetTarget(lumieres[i].transform.position);

                //// Tentative de BezierTrails !
                //BezierTrail trail = tr.GetComponent<BezierTrail>();
                //Debug.Log("count = " + trail.curve.pointCount);
                //Debug.DrawRay(derriere, devant - derriere, Color.red);
                //Debug.Log("derrier = " + derriere);
                //Debug.Log("devant = " + devant);
                //Debug.Log("target = " + target);
                //trail.AddPoint(derriere);
                //Debug.Log("count = " + trail.curve.pointCount);
                //BezierPoint connectionPoint = trail.AddPoint(devant);
                ////connectionPoint.handle1 = player.transform.position - gm.gravityManager.Down() - connectionPoint.position;
                //trail.AddPoint(target);
                //Debug.Log("count = " + trail.curve.pointCount);
			}

			// Un petit message
			gm.console.RunLocalisation();

			// Et on certifie qu'on a appuyé sur E
			gm.console.UpdateLastLumiereAttrapee();
		}

        return true;
    }
}
