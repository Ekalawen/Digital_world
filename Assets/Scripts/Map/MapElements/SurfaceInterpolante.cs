using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceInterpolante : CubeEnsemble {

    protected Vector3[,] pointsFinaux; // Les points finaux qui nous intéressent !
    protected Vector3[,] pdc; // Les points de contrôle
    protected int nbPointsEntrePdc; // L'écart en nombre de points entre chaque point de contrôle
    protected int n, m; // Le nombre de points de contrôle
    protected int sizeGrilleX, sizeGrilleY; // Le nombre de points finaux
    protected Vector2 tailleXZ; // La taille que devra couvrir nos pointsFinaux

	// Renvoie la liste des points de la surface interpolante générés par les points de contrôle et le pas donné
	// Les points de contrôle doivent être placé selon un damier rectangulaire
	// Il est important que les points soient ordonnées <3
	// Le pas doit être un diviseur de 1. (ex: 0.2 ou 0.5 mais pas 0.3 ou 0.4 !)
	// Le résultat sera une grille sur [0, 1]
	// Les hauteurs doivent être dans [0, 1];
	public SurfaceInterpolante(Vector3[,] pdc, int nbPointsEntrePdc, Vector2 tailleXZ, bool roundCubePosition) {
        this.pdc = pdc;
        this.nbPointsEntrePdc = nbPointsEntrePdc;
        this.tailleXZ = tailleXZ;

		// On récupère la taille de la surface
		n = pdc.GetLength(0);
		m = pdc.GetLength(1);

		// On récupère la base de la grille
        sizeGrilleX = (n - 1) * nbPointsEntrePdc + 1; // (int)((n - 1) / pas + 1);
        sizeGrilleY = (m - 1) * nbPointsEntrePdc + 1; // (int)((m - 1) / pas + 1);
		Vector2[,] grilleXY = new Vector2[sizeGrilleX, sizeGrilleY]; // Pour chaque point que l'on va créer sur la surface, contient une valeur de son avancement entre 0 et 1
		for(int i = 0; i < sizeGrilleX; i++) {
			for(int j = 0; j < sizeGrilleY; j++) {
				grilleXY[i, j] = new Vector2((float)i / (sizeGrilleX-1), (float)j / (sizeGrilleY-1));
			}
		}

        // L'ensemble des points à renvoyer
        pointsFinaux = new Vector3[sizeGrilleX, sizeGrilleY];

		// On va créer la surface
		float[,] P = ExtraireHauteursPoints(pdc, n, m); // les hauteurs des pdc
		for(int i = 0; i < sizeGrilleX; i++) {
			for(int j = 0; j < sizeGrilleY; j++) {
				float s = grilleXY[i, j].y; // avancement sur [0, 1] des z (oui il y marqué y parce que c'est un vector 2 ...)
				float t = grilleXY[i, j].x; // avancement sur [0, 1] des x

				float[] X = ExtraireAvancementsN(n); // extrait tous les avancements possibles sur [0, 1] de x
				float[] Z = ExtraireAvancementsN(m); // extrait tous les avancements possibles sur [0, 1] de z

				float y = SurfaceProduitTensoriel(P, X, Z, s, t); // On calcul la hauteur de notre point ! <3
				float x = t * tailleXZ.x;
				float z = s * tailleXZ.y;
                Vector3 pos = new Vector3(x, y, z);
                if (roundCubePosition)
                    pos = MathTools.RoundToInt(pos);
                pointsFinaux[i, j] = pos;
			}
		}
	}

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.SURFACE_INTERPOLANTE;
    }

	// Génère des points de contrôle sur une surface d'une certaine taille avec une certaine amplitude maximum de hauteur
	// Avec un certain pas pour les points de contrôle
	public static Vector3[,] GeneratePointsDeControle(Vector2Int tailleSurface, float hauteurMaxPdc, int ecartsPdc) {
		int nbX = (int)(tailleSurface.x / ecartsPdc) + 1;
        int nbZ = (int)(tailleSurface.y / ecartsPdc) + 1;
		Vector3[,] pdc = new Vector3[nbX, nbZ];

		for(int i = 0; i < nbX; i ++) {
			for(int j = 0; j < nbZ; j ++) {
				pdc[i, j] = new Vector3(i * ecartsPdc, Random.Range(0, hauteurMaxPdc), j * ecartsPdc);
			}
		}

		return pdc;
	}

    public void GenerateCubes() {
        for(int i = 0; i < sizeGrilleX; i++) {
            for(int j = 0; j < sizeGrilleY; j++) {
                // On crée le cube à l'endroit indiqué !
                CreateCube(pointsFinaux[i, j]);
            }
        }
    }

    public Vector3[,] GetAllPositions() {
        return pointsFinaux;
    }

    public void AddOffset(Vector3 offset) {
        for(int i = 0; i < pointsFinaux.GetLength(0); i++) {
            for (int j = 0; j < pointsFinaux.GetLength(1); j++) {
                pointsFinaux[i, j] += offset;
            }
        }
    }

    public override string GetName() {
        return "SurfaceInterpolante";
    }

	// Permet d'extraire les Y des Vector3
	public static float[,] ExtraireHauteursPoints(Vector3[,] pdc, int n, int m) {
		float[,] Y = new float[n, m];
		for(int i = 0; i < n ; i++) {
			for(int j = 0; j < m ; j++) {
				Y[i, j] = pdc[i, j].y;
			}
		}
		return Y;
	}

	// Permet d'extraire les X des Vector3 en sélectionnant une ligne
	// Fonctionne aussi pour Z
	public static float[] ExtraireAvancementsN(int n) {
		float[] X = new float[n];
		for(int i = 0; i < n; i++) {
			X[i] = (float)i / (n-1);
		}
		return X;
	}

	// P sont les hauteurs des points de la surface
	public static float SurfaceProduitTensoriel(float[,] P, float[] X, float[] Y, float s, float t) {
		int n = P.GetLength(0);
		int m = P.GetLength(1); // à utiliser ... !

		float z = 0;
		for(int i = 0; i < n ; i++) {
			float[] Pi = ExtraireLigneP(P, m, i); // à vérifier si c'est pas dans l'autre sens
			float Pit = PolynomeDeLagrangeEvaluation(Y, Pi, t);
			float Lis = PolynomeUnitaireDegreeN(X, n, i, s);
			z = z + Pit * Lis;
		}

		return z;
	}

	// Permet d'extraire les une ligne de P
	public static float[] ExtraireLigneP(float[,] P, int n, int ligne) {
		float[] Y = new float[n];
		for(int i = 0; i < n ; i++) {
			Y[i] = P[ligne, i];
		}
		return Y;
	}

	// Renvoie l'évaluation d'un polynôme de lagrange en x où les points de contrôles sont les (X(i), Y(i))
	public static float PolynomeDeLagrangeEvaluation(float[] X, float[] Y, float x) {
		// La taille des points
		float n = X.Length;

		// On applique la formule
		// C'est la somme des hauteurs des points multipliés lix
		// Où Lix est le polynome de langrage numéro i en x
		float y = 0;
		for(int i = 0; i < n; i++) {
			float yi = Y[i];
			float lix = 1;
			for(int j = 0; j < n; j++) {
				if(j != i) {
					lix = lix * (x - X[j]) / (X[i] - X[j]);
				}
			}
			y = y + yi * lix;
		}

		return y;
	}


	// Retourne l'évaluation d'un polynome unitaire de degrée n d'indice i en u
	// Dont les points de contrôle sont les X(i)
	public static float PolynomeUnitaireDegreeN(float[] X, int n, int i, float u) {
		float[] Y = new float[n];
		Y[i] = 1;

		float z = PolynomeDeLagrangeEvaluation(X, Y, u);
		return z;
	}
}
