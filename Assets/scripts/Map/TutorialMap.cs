using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de créer la map du tutoriel !
public class TutorialMap : MapManager {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉS
	//////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	public void Start() {
		base.Start();
		
		generateTutorialMap();
	}

	// Génère la map du tutoriel ! =)
	void generateTutorialMap() {
		// Première zone : Arrivée
		remplirFace(Vector3.zero, Vector3.right, 20, Vector3.forward, 10); // sol
		remplirFace(Vector3.zero, Vector3.right, 20, Vector3.up, 10); // les 3 cotés
		remplirFace(Vector3.forward * 10, Vector3.right, 20, Vector3.up, 10);
		remplirFace(Vector3.zero, Vector3.up, 10, Vector3.forward, 10);
		nbLumieres++;
		Instantiate (objectif, new Vector3(20, 1, 5), Quaternion.identity);
		
		// Un pont pour aller à la deuxième =)
		remplirBridge(new Vector3(20, 0, 5), Vector3.right, 10); // première ligne droite
		remplirBridge(new Vector3(30, 0, 5), Vector3.forward, 3); // puis un virage
		remplirBridge(new Vector3(30, 0, 8), Vector3.right, 3); // puis un virage
		remplirBridge(new Vector3(33, 0, 8), new Vector3(1, 0, -1), 5); // puis une diagonale
		remplirBridge(new Vector3(38, 0, 3), new Vector3(1f / 3f, -1f / 3f, 0f), 30); // puis une descente
		nbLumieres++;
		Instantiate (objectif, new Vector3(50, -9, 3), Quaternion.identity);

		// Deuxième zone : Saut
		remplirFace(new Vector3(48, -10, 1), Vector3.right, 10, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(58, -9, 1), Vector3.right, 15, Vector3.forward, 5); // on change de hauteur
		remplirFace(new Vector3(64, -8, 1), Vector3.right, 2, Vector3.forward, 5); // un petit saut
		nbLumieres++;
		Instantiate (objectif, new Vector3(66, -6, 3), Quaternion.identity);
		remplirCubePlein(new Vector3(73, -9, 1), Vector3.right, 5, Vector3.forward, 5, Vector3.up, 5);
		// et puis on creuse pour créer un passage ! =)
		foreach(Collider c in Physics.OverlapBox(new Vector3(73.5f, -6f, 2f), new Vector3(0.5f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(74f, -5f, 3.5f), new Vector3(0.3f, 2f, 0.5f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(75f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -5f, 3f), new Vector3(0.3f, 2f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -4f, 2f), new Vector3(0.3f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -3f, 4f), new Vector3(0.3f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		nbLumieres++;
		Instantiate (objectif, new Vector3(77, -3, 3), Quaternion.identity);

		// Un pont
		remplirBridge(new Vector3(78, -5, 3), Vector3.right, 4); // première ligne droite
		remplirBridge(new Vector3(82, -5, 3), new Vector3(1f / 3f, 1f / 3f, 0f), 15); // puis une montée

		// Troisième zone : Aggripage
		remplirFace(new Vector3(87, 0, 1), Vector3.right, 15, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(92, 0, 1), Vector3.up, 5, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(102, 0, 5), Vector3.right, 7, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(109, 0, 1), Vector3.right, 6, Vector3.forward, 5); // une plus petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(105.5f, 3f, 4f), Quaternion.identity); // Un objectif à attraper en plein vol ! <3
		remplirFace(new Vector3(115, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(120, 0, 5), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(125, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(130, 0, 1), Vector3.right, 5, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.forward, 3); // une plus petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(134, 1, 3), Quaternion.identity);
		remplirFace(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		remplirFace(new Vector3(135, 0, 4), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		remplirBridge(new Vector3(137, 14, 4), new Vector3(0, 0, 1), 16); // puis un pont latéral
		remplirFace(new Vector3(135, 0, 20), Vector3.right, 5, Vector3.forward, 5); // une petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(137, 1, 22), Quaternion.identity);

		// Quatrième zone : Shift
		remplirFace(new Vector3(140, 0, 20), new Vector3(1, 1, 0), 10, new Vector3(0, 0, 1), 5); // un escalier montant
		remplirCubePlein(new Vector3(150, 10, 20), new Vector3(1, 0, 0), 5, new Vector3(0, 0, 1), 5, new Vector3(0, -1, 0), 10); // un trou géant !
		foreach(Collider c in Physics.OverlapBox(new Vector3(152f, 12f, 22f), new Vector3(0.3f, 20f, 0.3f))) // colonne
			Destroy(c.gameObject);

		// Cinquième zone : Localisation
		remplirFace(new Vector3(150, -5, 20), Vector3.right, 25, Vector3.forward, 5); // une petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 20), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 22), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 24), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(170, -4, 22), Quaternion.identity);

		// Sixième zone : ennemi !
		remplirFace(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.forward, 25); // une grande zone
		remplirFace(new Vector3(175, -5, 0), Vector3.up, 10, Vector3.forward, 20); // et 4 murs pour protéger
		remplirFace(new Vector3(175, -4, 20), Vector3.up, 1, Vector3.forward, 5); // et une petite rembarde pour empêcher le joueur de mourir d'un coup :D
		remplirFace(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		remplirFace(new Vector3(200, -5, 0), Vector3.up, 10, Vector3.forward, 26); // et 4 murs pour protéger
		remplirFace(new Vector3(175, -5, 25), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		Instantiate(ennemiPrefabs, new Vector3(187, -4, 12), Quaternion.identity);
		remplirFace(new Vector3(200, 5, 10), Vector3.right, 5, Vector3.forward, 5); // la zone finale !
		nbLumieres++;
		Instantiate (objectif, new Vector3(202, 6, 12), Quaternion.identity);
	}
}
