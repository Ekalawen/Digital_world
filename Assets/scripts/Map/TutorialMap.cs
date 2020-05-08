using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de créer la map du tutoriel !
public class TutorialMap : MapManager {

    protected override void GenerateMap() {
		GenerateTutorialMap();
    }

    // Génère la map du tutoriel ! =)
    void GenerateTutorialMap() {
		//// Première zone : Arrivée
  //      /*
		//CreateMur(Vector3.zero, Vector3.right, 20, Vector3.forward, 10); // sol
		//CreateMur(Vector3.zero, Vector3.right, 20, Vector3.up, 10); // les 3 cotés
		//CreateMur(Vector3.forward * 10, Vector3.right, 20, Vector3.up, 10);
		//CreateMur(Vector3.zero, Vector3.up, 10, Vector3.forward, 10);
  //      */
  //      CreateLumiere(new Vector3(20, 1, 5), Lumiere.LumiereType.NORMAL);
		
		//// Un pont pour aller à la deuxième =)
  //      /*
		//CreatePont(new Vector3(20, 0, 5), Vector3.right, 10); // première ligne droite
		//CreatePont(new Vector3(30, 0, 5), Vector3.forward, 3); // puis un virage
		//CreatePont(new Vector3(30, 0, 8), Vector3.right, 3); // puis un virage
		//CreatePont(new Vector3(33, 0, 8), new Vector3(1, 0, -1), 5); // puis une diagonale
		//CreatePont(new Vector3(38, 0, 3), new Vector3(1f / 3f, -1f / 3f, 0f), 30); // puis une descente
  //      */
  //      CreateLumiere(new Vector3(50, -9, 3), Lumiere.LumiereType.NORMAL);

		//// Deuxième zone : Saut
  //      /*
		//CreateMur(new Vector3(48, -10, 1), Vector3.right, 10, Vector3.forward, 5); // une plus petite zone
		//CreateMur(new Vector3(58, -9, 1), Vector3.right, 15, Vector3.forward, 5); // on change de hauteur
		//CreateMur(new Vector3(64, -8, 1), Vector3.right, 2, Vector3.forward, 5); // un petit saut
  //      CreateLumiere(new Vector3(66, -6, 3), Lumiere.LumiereType.NORMAL);
  //      Cave cave = new Cave(new Vector3(73, -9, 1), new Vector3Int(5, 8, 5), bDigInside: false);
		//// et puis on creuse pour créer un passage ! =)
		//foreach(Collider c in Physics.OverlapBox(new Vector3(73.5f, -6f, 2f), new Vector3(0.5f, 2f, 0.3f))) // colonne
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(74f, -5f, 3.5f), new Vector3(0.3f, 2f, 0.5f))) // colonne
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(75f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -5f, 3f), new Vector3(0.3f, 2f, 0.3f))) // trou unique
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -5.5f, 2f), new Vector3(0.3f, 0.5f, 0.3f))) // trou double
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -1f, 2f), new Vector3(0.3f, 2f, 0.3f))) // colonne
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -3f, 3f), new Vector3(0.3f, 2f, 0.3f))) // colonne
		//	Destroy(c.gameObject);
		//foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -3f, 4f), new Vector3(0.3f, 2f, 0.3f))) // colonne
		//	Destroy(c.gameObject);
  //          */
  //      CreateLumiere(new Vector3(77, -1, 3), Lumiere.LumiereType.NORMAL);

		//// Un pont
  //      /*
		//CreatePont(new Vector3(78, -5, 3), Vector3.right, 4); // première ligne droite
		//CreatePont(new Vector3(82, -5, 3), new Vector3(1f / 3f, 1f / 3f, 0f), 15); // puis une montée
  //      */

		//// Troisième zone : Aggripage
  //      /*
		//CreateMur(new Vector3(87, 0, 1), Vector3.right, 15, Vector3.forward, 5); // une plus petite zone
		//CreateMur(new Vector3(92, 0, 1), Vector3.up, 5, Vector3.forward, 5); // une plus petite zone
		//CreateMur(new Vector3(102, 0, 5), Vector3.right, 7, Vector3.up, 5); // un mur horizontal !
		//CreateMur(new Vector3(109, 0, 1), Vector3.right, 6, Vector3.forward, 5); // une plus petite zone
  //      */
  //      CreateLumiere(new Vector3(105.5f, 3f, 4f), Lumiere.LumiereType.NORMAL);
  //      /*
		//CreateMur(new Vector3(115, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		//CreateMur(new Vector3(120, 0, 5), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		//CreateMur(new Vector3(125, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		//CreateMur(new Vector3(130, 0, 1), Vector3.right, 5, Vector3.forward, 5); // une plus petite zone
		//CreateMur(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.forward, 3); // une plus petite zone
  //      */
  //      CreateLumiere(new Vector3(134, 1, 3), Lumiere.LumiereType.NORMAL);
  //      /*
		//CreateMur(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		//CreateMur(new Vector3(135, 0, 4), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		//CreatePont(new Vector3(137, 14, 4), new Vector3(0, 0, 1), 16); // puis un pont latéral
		//CreateMur(new Vector3(135, 0, 20), Vector3.right, 5, Vector3.forward, 5); // une petite zone
  //      */
  //      CreateLumiere(new Vector3(137, 1, 22), Lumiere.LumiereType.NORMAL);

		//// Quatrième zone : Shift
  //      /*
		//CreateMur(new Vector3(140, 0, 20), new Vector3(1, 1, 0), 10, new Vector3(0, 0, 1), 5); // un escalier montant
  //      cave = new Cave(new Vector3(150, 0, 20), new Vector3Int(5, 10, 5), bDigInside: false); // BIEN VERIFIER ICI !
		////remplirCubePlein(new Vector3(150, 10, 20), new Vector3(1, 0, 0), 5, new Vector3(0, 0, 1), 5, new Vector3(0, -1, 0), 10); // un trou géant !
		//foreach(Collider c in Physics.OverlapBox(new Vector3(152f, 12f, 22f), new Vector3(0.3f, 20f, 0.3f))) // colonne
		//	Destroy(c.gameObject);
  //      */

		//// Cinquième zone : Localisation
  //      /*
		//CreateMur(new Vector3(150, -5, 20), Vector3.right, 25, Vector3.forward, 5); // une petite zone
  //      */
  //      CreateLumiere(new Vector3(160, -4, 20), Lumiere.LumiereType.NORMAL);
  //      CreateLumiere(new Vector3(160, -4, 22), Lumiere.LumiereType.NORMAL);
  //      CreateLumiere(new Vector3(160, -4, 24), Lumiere.LumiereType.NORMAL);
  //      CreateLumiere(new Vector3(170, -4, 22), Lumiere.LumiereType.NORMAL);

		//// Sixième zone : ennemi !
  //      /*
		//CreateMur(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.forward, 25); // une grande zone
		//CreateMur(new Vector3(175, -5, 0), Vector3.up, 10, Vector3.forward, 20); // et 4 murs pour protéger
		//CreateMur(new Vector3(175, -4, 20), Vector3.up, 1, Vector3.forward, 5); // et une petite rembarde pour empêcher le joueur de mourir d'un coup :D
		//CreateMur(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		//CreateMur(new Vector3(200, -5, 0), Vector3.up, 10, Vector3.forward, 26); // et 4 murs pour protéger
		//CreateMur(new Vector3(175, -5, 25), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		//CreateMur(new Vector3(200, 5, 10), Vector3.right, 5, Vector3.forward, 5); // la zone finale !
  //      */
  //      GameObject sondePrefab = gm.ennemiManager.ennemisPrefabs[0];
		//Instantiate(sondePrefab, new Vector3(187, -4, 12), Quaternion.identity);
  //      CreateLumiere(new Vector3(202, 6, 12), Lumiere.LumiereType.NORMAL);
	}

    private void CreateMur(Vector3 depart, Vector3 dir1, int dist1, Vector3 dir2, int dist2) {
        Mur mur = new Mur(depart, dir1, dist1, dir2, dist2);
    }
    private void CreatePont(Vector3 depart, Vector3 dir1, int dist1) {
        Pont pont = new Pont(depart, dir1, dist1);
    }

    public override Vector3 GetPlayerStartPosition() {
        return new Vector3(5, 5, 5);
        //return new Vector3(184, 10, 22);
    }

    public override Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        return new Vector2(90, 90);
    }

    public override Vector3 GetCenter() {
        return new Vector3(107, 10, 12);
    }
}
