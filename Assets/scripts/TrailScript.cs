using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailScript : MonoBehaviour {

	public float vitesse; // La vitesse de déplacement du trail

	private Vector3 target; // La où le trail doit aller
	private float lifeTime; // La durée de vie du trail après avoir atteint son objectif
	private bool cibleAtteinte; // Permet de savoir qu'on a atteint la cible
	private float timeCibleAtteinte;

	// Use this for initialization
	void Start () {
		cibleAtteinte = false;
	}
	
	// Update is called once per frame
	void Update () {

		// On se déplace dans la direction de la cible
		Vector3 direction = target - transform.position;
		direction.Normalize ();
		if (Vector3.Magnitude (direction * Time.deltaTime * vitesse) > Vector3.Distance (target, transform.position)) {
			transform.Translate (target - transform.position);
			cibleAtteinte = true;
			timeCibleAtteinte = Time.time;
		} else {
			transform.Translate (direction * Time.deltaTime * vitesse);
		}

		// Si ça fait suffisament longtemps qu'on a atteint la cible, on disparait
		if (cibleAtteinte && Time.time - timeCibleAtteinte > lifeTime) {
			Destroy (this.gameObject);
		}
	}

	public void setTarget(Vector3 newTarget) {
		target = newTarget;

		// Et on calcul et on met à jour le life time
		lifeTime = Vector3.Distance(transform.position, target) / vitesse;
		this.GetComponent<TrailRenderer> ().time = lifeTime;
	}
}
