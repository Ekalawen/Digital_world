using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainMap : MapManager {
	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public int tailleX = 60; // La taille de la map en x
	public int tailleY = 80; // La taille de la map en y (en z en fait mais bon ^^)
	public int pas = 5; // Le pas d'échantillonnage des points de contrôles (leurs distances quoi)
	public float hauteurMax = 5f; // La hauteur maximale que peut prendre un point de contrôle, à partir de 0
	public float proportionArbres = 0.0013f; // La quantité d'arbres par rapport à la surface de la map

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉS
	//////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	public void Start() {
		base.Start();
	}

    public override void Initialize() {
        base.Initialize();
		
		generatePlainsMap(tailleX, tailleY, pas, hauteurMax);
    }


    // Génère une map avec une plaine interpolé
    void generatePlainsMap(int tailleX, int tailleY, int pas, float hauteurMax) {

		// On génère les points de contrôles
		Vector3[,] pdc = generatePointsDeControle(tailleX, tailleY, hauteurMax, pas);

		// On récupère les positions de tous les cubes
		List<Vector3> positions = Interpolation.surfaceInterpolante(pdc, 1f / pas);

		// On dénormalise !
		for(int i = 0; i < positions.Count; i++) {
			Vector3 tmp = positions[i];
			tmp.x *= tailleX;
			tmp.z *= tailleY;
			positions[i] = tmp;
		}

		// On instancie tous les cubes
		foreach(Vector3 pos in positions) {
			cubes.Add(Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject);
		}

		// Puis on crée tous les arbres
		// On définit le nombre d'arbre en fonction de la surface de la map
		int surfaceMap = tailleX * tailleY;
		int nbArbres = (int) (surfaceMap * proportionArbres);
		List<Vector3> posArbresPossibles = positions;
		for(int i = 0; i < nbArbres; i++) {
			int indice = Random.Range(0, posArbresPossibles.Count);
			generateArbre(posArbresPossibles[indice]);
			posArbresPossibles.RemoveAt(indice);
		}

	}

	// Génère des points de contrôle sur une surface d'une certaine taille avec une certaine amplitude maximum de hauteur
	// Avec un certain pas pour les points de contrôle
	Vector3[,] generatePointsDeControle(int tailleX, int tailleY, float hauteurMax, int pas) {
		int nbX = tailleX / pas + 1;
		int nbY = tailleY / pas + 1;
		Vector3[,] pdc = new Vector3[nbX, nbY];

		for(int i = 0; i < nbX; i ++) {
			for(int j = 0; j < nbY; j ++) {
				pdc[i, j] = new Vector3(i * pas, Random.Range(0, hauteurMax), j * pas);
			}
		}

		return pdc;
	}

	// Génère un arbre ! <3
	// Les arbres sont des structures verticales que le joueur devra escalader pour récupérer la récompense à son sommet ! <3
	void generateArbre(Vector3 racine) {
		// On génère un certain nombre de palliers
		int nbPalliers = Random.Range(2, 8);

		// Pour chaque pallier
		for(int i = 0; i < nbPalliers; i++) {
			// On définit la taille de notre pallier =)
			Vector3 taillePallier = new Vector3(Random.Range(3, 10), Random.Range(8, 20), Random.Range(3, 10));

			// on détermine sa cime
			Vector3 cime = new Vector3(racine.x + Random.Range(-taillePallier.x, taillePallier.x),
									   racine.y + taillePallier.y,
									   racine.z + Random.Range(-taillePallier.z, taillePallier.z));

			// On crée le tronc entre la racine et la cime
			generateTronc(racine, cime);

			// On crée des escalateur pour pouvoir monter dans cet arbre !

			// On crée le pallier autour de la cime
			//generatePallier(cime, taillePallier);
			generatePallierLagrange(cime, taillePallier);

			// On ouvre au niveau de la cime pour que le joueur puisse passer !
			if(i != 0) {
				ouvrirCime(racine);
			}

			// Puis la cime devient la nouvelle racine !
			racine = cime;
		}
		ouvrirCime(racine);
	}

	// Génère un tronc enre une racine et une cime
	void generateTronc(Vector3 racine, Vector3 cime) {
		Vector3 dir = Vector3.Normalize(cime - racine);
		int dist = (int)Vector3.Magnitude(cime - racine) + 1;
		remplirBridge(racine, dir, dist);
	}

	// Génère un pallier, tout simplement ! =)
	void generatePallier(Vector3 cime, Vector3 taillePallier) {
		// On choisit une direction horizontale
		Vector3 direction1 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
		int distance1 = (int)taillePallier.x;

		// Puis on récupère son orthogonalle, toujours à l'horizontale !
		Vector3 direction2 = Vector3.Cross(direction1, Vector3.up);
		int distance2 = (int)taillePallier.z;

		// On trouve le centre du pallier
		Vector3 coinPallier = cime - direction1 * distance1 / 2 - direction2 * distance2 / 2;

		// Puis on peut créer notre face ! <3
		remplirFace(coinPallier, direction1, distance1 + 1, direction2, distance2 + 1);
	}

	// Génère un pallier, tout simplement ! =)
	void generatePallierLagrange(Vector3 cime, Vector3 taillePallier) {
		int distance1 = (int)taillePallier.x;
		int distance2 = (int)taillePallier.z;
		int pas = 3;
		float hauteurMax = 3f;
		distance1 += (pas - (distance1 % pas));
		distance2 += (pas - (distance2 % pas));

		// On génère les points de contrôle !
		// Il faudra régler le pas !
		Vector3[,] pdc = generatePointsDeControle(distance1, distance2, hauteurMax, pas);

		// On récupère les positions de tous les cubes
		List<Vector3> positions = Interpolation.surfaceInterpolante(pdc, 1f / pas);

        // On dénormalise !
        float distMin = 10f;
        float yOffset = 0f;
		for(int i = 0; i < positions.Count; i++) {
            // On récupère la hauteur du centre
            float dist = Vector2.Distance(new Vector2(positions[i].x, positions[i].y), new Vector2(0.5f, 0.5f));
            if (dist < distMin) {
                distMin = dist;
                yOffset = positions[i].y;
            }

			Vector3 tmp = positions[i];
			tmp.x *= distance1;
			tmp.z *= distance2;
			positions[i] = tmp;
		}

		// On décale tous les points de contrôle pour être au bon endroit
		Vector3 coinPallier = cime - Vector3.right * distance1 / 2 - Vector3.forward * distance2 / 2;
		for(int i = 0; i < positions.Count; i++) {
			Vector3 tmp = positions[i];
			tmp = tmp + coinPallier;
            tmp.y -= yOffset;
			positions[i] = tmp;
		}

		// On instancie tous les cubes
		foreach(Vector3 pos in positions) {
			cubes.Add(Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject);
		}
	}

	// Crée un trou pour que le joueur puisse monter et descendre de l'arbre !
	void ouvrirCime(Vector3 cime) {
		Collider[] cubesProches = Physics.OverlapSphere(cime, 1f);
		foreach(Collider c in cubesProches) {
			DestroyImmediate(c.gameObject);
		}
	}	

}
