using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProbeScript : MonoBehaviour {

	public float distanceDeRafraichissement = 5f;
	public int frequenceDeRafraichissement = 1000;

	private GameObject player;
	private int dernierRafraichissement;

	// Use this for initialization
	void Start () {
		// On récupère le joueur		
		player = GameObject.Find("Joueur");
		if (player == null) {
			Debug.Log ("Player non trouvé !!!");
		}
	}
	
	// Update is called once per frame
	void Update () {
		// On ne s'update que si le joueur est suffisamment proche !
		if (Vector3.Distance (player.transform.position, transform.position) < distanceDeRafraichissement) {
			// On ne refraichit qu'une frame sur frequenceDeRafraichissement
			dernierRafraichissement = (dernierRafraichissement + 1) % frequenceDeRafraichissement;
			if (dernierRafraichissement == 0) {
				// On raffraichit pas ! ... parce que on peut pas se permettre de faire laguer le jeu pour ça !
				//this.GetComponent<ReflectionProbe> ().RenderProbe ();
			}
		} else {
			dernierRafraichissement = -1;
		}
	}
}
