using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de générer une map avec un sol plat, pratique pour tester des trucs !
public class GroundMap : MapManager {


	public void Start() {
		base.Start();
		
		generateGroundMap();
	}

	// Génère juste une map pour pouvoir tester des trucs !
	void generateGroundMap() {
		// On initialise la position des coins !
		Vector3[] pos = new Vector3[8];
		pos [0] = new Vector3 (0, 0, 0);
		pos [1] = new Vector3 (tailleMap, 0, 0);
		pos [2] = new Vector3 (tailleMap, tailleMap, 0);
		pos [3] = new Vector3 (0, tailleMap, 0);
		pos [4] = new Vector3 (0, 0, tailleMap);
		pos [5] = new Vector3 (tailleMap, 0, tailleMap);
		pos [6] = new Vector3 (tailleMap, tailleMap, tailleMap);
		pos [7] = new Vector3 (0, tailleMap, tailleMap);

		// On veut créer les faces du cube !
		// remplirFace(0, 1, 2, 3, pos); // coté 1
		// remplirFace(4, 5, 6, 7, pos); // coté 2
		// remplirFace(0, 4, 7, 3, pos); // coté 3
		remplirFace(0, 1, 5, 4, pos); // sol !
		// remplirFace(1, 2, 6, 5, pos); // coté 4
		//remplirFace(2, 3, 7, 6); // plafond !
	}
}
