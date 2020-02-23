using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arbre : CubeEnsemble {

    public int nbPalliers; // On génère un certain nombre de palliers (entre 2 et 8 de base)
    public Vector2Int taillePallierRange; // La taille en largeur des palliers (entre 3 et 10 de base)
    public Vector2Int hauteurPallierRange; // La hauteur d'un pallier (entre 8 et 20 de base)
    public int pasPdcPallier; // La distance entre les pdc dans un pallier (3 de base)
    public float hauteurPdcPallier; // La hauteur des pdc dans un pallier (3f de base)

    public List<Pont> ponts;
    //public List<Pallier> palliers;

    public Arbre(
        Vector2Int nbPalliersRange,
        Vector2Int taillePallierRange,
        Vector2Int hauteurPallierRange,
        int pasPdcPallier,
        float hauteurPdcPallier) {
        this.nbPalliers = MathTools.RandBetween(nbPalliersRange);
        this.taillePallierRange = taillePallierRange;
        this.hauteurPallierRange = hauteurPallierRange;
        this.pasPdcPallier = pasPdcPallier;
        this.hauteurPdcPallier = hauteurPdcPallier;
    }

    public override string GetName() {
        return "Arbre";
    }

	// Génère un arbre ! <3
	// Les arbres sont des structures verticales que le joueur devra escalader pour récupérer la récompense à son sommet ! <3
	void GenerateArbre(Vector3 racine) {
		// Pour chaque pallier
		for(int i = 0; i < nbPalliers; i++) {
            // On définit la taille de notre pallier =)
            Vector3 taillePallier = new Vector3(
                MathTools.RandBetween(taillePallierRange),
                MathTools.RandBetween(hauteurPallierRange),
                MathTools.RandBetween(taillePallierRange));

			// on détermine sa cime
			Vector3 cime = new Vector3(racine.x + Random.Range(-taillePallier.x, taillePallier.x),
									   racine.y + taillePallier.y,
									   racine.z + Random.Range(-taillePallier.z, taillePallier.z));

			// On crée le tronc entre la racine et la cime
			Pont pont = GenerateTronc(racine, cime);
            ponts.Add(pont);

			// On crée des escalateur pour pouvoir monter dans cet arbre !
            // TODO !

			// On crée le pallier autour de la cime
			//generatePallier(cime, taillePallier);
			GeneratePallierLagrange(cime, taillePallier);

			// On ouvre au niveau de la cime pour que le joueur puisse passer !
			if(i != 0) {
				OuvrirCime(racine);
			}

			// Puis la cime devient la nouvelle racine !
			racine = cime;
		}
		OuvrirCime(racine);
	}

	// Génère un tronc enre une racine et une cime
	Pont GenerateTronc(Vector3 racine, Vector3 cime) {
		Vector3 dir = Vector3.Normalize(cime - racine);
		int dist = (int)Vector3.Magnitude(cime - racine) + 1;
        Pont pont = new Pont(racine, dir, dist);
        return pont;
	}

	// Génère un pallier, tout simplement ! =)
	void GeneratePallier(Vector3 cime, Vector3 taillePallier) {
		// On choisit une direction horizontale
		Vector3 direction1 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
		int distance1 = (int)taillePallier.x;

		// Puis on récupère son orthogonalle, toujours à l'horizontale !
		Vector3 direction2 = Vector3.Cross(direction1, Vector3.up);
		int distance2 = (int)taillePallier.z;

		// On trouve le centre du pallier
		Vector3 coinPallier = cime - direction1 * distance1 / 2 - direction2 * distance2 / 2;

        // Puis on peut créer notre face ! <3
        Mur mur = new Mur(coinPallier, direction1, distance1 + 1, direction2, distance2 + 1);
	}

	// Génère un pallier, tout simplement ! =)
	void GeneratePallierLagrange(Vector3 cimePosition, Vector3 taillePallier) {
		int distance1 = (int)taillePallier.x;
		int distance2 = (int)taillePallier.z;
		distance1 += (pasPdcPallier - (distance1 % pasPdcPallier));
		distance2 += (pasPdcPallier - (distance2 % pasPdcPallier));

		// On génère les points de contrôle !
		// Il faudra régler le pas !
		Vector3[,] pdc = PlainMap.GeneratePointsDeControle(distance1, distance2, hauteurPdcPallier, pasPdcPallier);

		// On récupère les positions de tous les cubes
		List<Vector3> positions = Interpolation.SurfaceInterpolante(pdc, 1f / pasPdcPallier);

        // On dénormalise !
        float distMin = float.PositiveInfinity;
        float yOffset = 0f;
		for(int i = 0; i < positions.Count; i++) {
            // On récupère la hauteur du centre
            float dist = Vector2.Distance(new Vector2(positions[i].x, positions[i].z), new Vector2(0.5f, 0.5f));
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
		Vector3 coinPallier = cimePosition - Vector3.right * distance1 / 2 - Vector3.forward * distance2 / 2;
		for(int i = 0; i < positions.Count; i++) {
			Vector3 tmp = coinPallier + positions[i];
            tmp.y -= yOffset;
			positions[i] = tmp;
		}

		// On instancie tous les cubes
		foreach(Vector3 pos in positions) {
            CreateCube(pos);
		}
	}

	// Crée un trou pour que le joueur puisse monter et descendre de l'arbre !
	void OuvrirCime(Vector3 cime) {
        List<Cube> cubes = map.GetCubesInSphere(cime, 1f);
        foreach(Cube cube in cubes) {
            map.DeleteCube(cube);
        }
	}	
}
