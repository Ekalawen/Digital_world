using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateMap : MapManager {

	public Vector2Int taille = new Vector2Int(60, 60); // La taille de la map en x et z

    protected override void GenerateMap() {
		GeneratePlateMap();
    }

    // Génère une map avec une plaine interpolé
    void GeneratePlateMap() {
        Mur sol = new Mur(Vector3.zero, Vector3.right, taille.x, Vector3.forward, taille.y);
    }
}
