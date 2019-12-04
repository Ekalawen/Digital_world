using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de générer une map avec un sol plat, pratique pour tester des trucs !
public class GroundMap : MapManager {

    protected override void GenerateMap() {
        generateGroundMap();
    }

    // Génère juste une map pour pouvoir tester des trucs !
    void generateGroundMap() {
        Vector3 depart = Vector3.zero;
        Vector3 direction1 = Vector3.right;
        Vector3 direction2 = Vector3.forward;
        Mur mur = new Mur(depart, direction1, tailleMap, direction2, tailleMap);
	}
}
