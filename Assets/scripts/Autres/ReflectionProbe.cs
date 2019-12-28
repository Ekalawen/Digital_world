using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProbe : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public float distanceDeRafraichissement = 5f;
	public int frequenceDeRafraichissement = 1000;

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	private GameObject player;
	private int dernierRafraichissement;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// On récupère le joueur		
		player = GameObject.Find("Joueur");
		if (player == null) {
			Debug.Log ("Player non trouvé !!!");
		}
	}

	void Update () {
        // On ne s'update que si le joueur est suffisamment proche !
        if (player == null)
            player = GameManager.Instance.player.gameObject;
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
