using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainMap : MapManager {

	public int tailleX = 60; // La taille de la map en x
	public int tailleZ = 80; // La taille de la map en z
	public int pas = 5; // Le pas d'échantillonnage des points de contrôles (leurs distances quoi)
	public float hauteurMax = 5f; // La hauteur maximale que peut prendre un point de contrôle, à partir de 0
	public float proportionArbres = 0.0013f; // La quantité d'arbres par rapport à la surface de la map
    public bool roundCubesPositions = true; // Permet de savoir s'il faut arrondir les positions des cubes à l'entier le plus proche

    protected override void GenerateMap() {
		generatePlainsMap(tailleX, tailleZ, pas, hauteurMax);
    }

    // Génère une map avec une plaine interpolé
    void generatePlainsMap(int tailleX, int tailleZ, int pas, float hauteurMax) {

		// On génère les points de contrôles
		Vector3[,] pdc = GeneratePointsDeControle(tailleX, tailleZ, hauteurMax, pas);

		// On récupère les positions de tous les cubes
		List<Vector3> positions = Interpolation.SurfaceInterpolante(pdc, 1f / pas);

		// On dénormalise !
		for(int i = 0; i < positions.Count; i++) {
			Vector3 tmp = positions[i];
			tmp.x *= tailleX;
			tmp.z *= tailleZ;
			positions[i] = tmp;
		}

		// On instancie tous les cubes
		foreach(Vector3 pos in positions) {
            if(roundCubesPositions)
                AddCube(MathTools.RoundToInt(pos), Cube.CubeType.NORMAL);
            else
                AddCube(pos, Cube.CubeType.NORMAL);
		}

		//// Puis on crée tous les arbres
		//// On définit le nombre d'arbre en fonction de la surface de la map
		//int surfaceMap = tailleX * tailleZ;
		//int nbArbres = (int) (surfaceMap * proportionArbres);
		//List<Vector3> posArbresPossibles = positions;
		//for(int i = 0; i < nbArbres; i++) {
		//	int indice = Random.Range(0, posArbresPossibles.Count);
		//	generateArbre(posArbresPossibles[indice]);
		//	posArbresPossibles.RemoveAt(indice);
		//}

	}

	// Génère des points de contrôle sur une surface d'une certaine taille avec une certaine amplitude maximum de hauteur
	// Avec un certain pas pour les points de contrôle
	public static Vector3[,] GeneratePointsDeControle(int tailleX, int tailleZ, float hauteurMax, int pas) {
		int nbX = tailleX / pas + 1;
		int nbZ = tailleZ / pas + 1;
		Vector3[,] pdc = new Vector3[nbX, nbZ];

		for(int i = 0; i < nbX; i ++) {
			for(int j = 0; j < nbZ; j ++) {
				pdc[i, j] = new Vector3(i * pas, Random.Range(0, hauteurMax), j * pas);
			}
		}

		return pdc;
	}

    public override Vector3 GetPlayerStartPosition() {
        Vector3 res = base.GetPlayerStartPosition();
        while (!IsOverCube(res))
            res = base.GetPlayerStartPosition();
        return res;
    }

    public bool IsOverCube(Vector3 position) {
        RaycastHit hit;
        Ray ray = new Ray (position, Vector3.down);
        return Physics.Raycast(ray, out hit) && hit.collider.tag == "Cube";
    }
}
