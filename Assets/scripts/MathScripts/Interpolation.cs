using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interpolation {

	// Renvoie la liste des points de la surface interpolante générés par les points de contrôle et le pas donné
	// Les points de contrôle doivent être placé selon un damier rectangulaire
	// Il est important que les points soient ordonnées <3
	// Le pas doit être un diviseur de 1.
	// Le résultat sera une grille sur [0, 1]
	// Les hauteurs doivent être dans [0, 1];
	public static List<Vector3> surfaceInterpolante(Vector3[,] pdc, float pas) {

		// On récupère la taille de la surface
		int n = pdc.GetLength(0);
		int m = pdc.GetLength(1);

		// On récupère la base de la grille
		float bugX = (n-1) / pas + 1;
		float bugY = (m-1) / pas + 1;
		int sizeGrilleX = (int) bugX;
		int sizeGrilleY = (int) bugY;
		Vector2[,] grilleXY = new Vector2[sizeGrilleX, sizeGrilleY];
		for(int i = 0; i < sizeGrilleX; i++) {
			for(int j = 0; j < sizeGrilleY; j++) {
				grilleXY[i, j] = new Vector2((float)i / (sizeGrilleX-1), (float)j / (sizeGrilleY-1));
			}
		}

		// L'ensemble des points à renvoyer
		List<Vector3> pointsFinaux = new List<Vector3>();

		// On va créer la surface
		float[,] P = extraireHauteursPoints(pdc, n, m); // les hauteurs des pdc
		for(int i = 0; i < sizeGrilleX; i++) {
			for(int j = 0; j < sizeGrilleY; j++) {
				float s = grilleXY[i, j].y; // avancement sur [0, 1] des z (oui il y marqué y parce que c'est un vector 2 ...)
				float t = grilleXY[i, j].x; // avancement sur [0, 1] des x

				float[] X = extraireX(n); // extrait tous les avancements possibles sur [0, 1] de x
				float[] Z = extraireX(m); // extrait tous les avancements possibles sur [0, 1] de z

				float y = surfaceProduitTensoriel(P, X, Z, s, t); // On calcul la hauteur de notre point ! <3
				float x = t;
				float z = s;
				pointsFinaux.Add(new Vector3(x, y, z));
			}
		}


		return pointsFinaux;
	}

	// Permet d'extraire les Y des Vector3
	public static float[,] extraireHauteursPoints(Vector3[,] pdc, int n, int m) {
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
	public static float[] extraireX(int n) {
		float[] X = new float[n];
		for(int i = 0; i < n; i++) {
			X[i] = (float)i / (n-1);
		}
		return X;
	}

	// P sont les hauteurs des points de la surface
	public static float surfaceProduitTensoriel(float[,] P, float[] X, float[] Y, float s, float t) {
		int n = P.GetLength(0);
		int m = P.GetLength(1); // à utiliser ... !

		float z = 0;
		for(int i = 0; i < n ; i++) {
			float[] Pi = extraireLigneP(P, m, i); // à vérifier si c'est pas dans l'autre sens
			float Pit = polynomeDeLagrangeEvaluation(Y, Pi, t);
			float Lis = polynomeUnitaireDegreeN(X, n, i, s);
			z = z + Pit * Lis;
		}

		return z;
	}

	// Permet d'extraire les une ligne de P
	public static float[] extraireLigneP(float[,] P, int n, int ligne) {
		float[] Y = new float[n];
		for(int i = 0; i < n ; i++) {
			Y[i] = P[ligne, i];
		}
		return Y;
	}

	// Renvoie l'évaluation d'un polynôme de lagrange en x où les points de contrôles sont les (X(i), Y(i))
	public static float polynomeDeLagrangeEvaluation(float[] X, float[] Y, float x) {
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
	public static float polynomeUnitaireDegreeN(float[] X, int n, int i, float u) {
		float[] Y = new float[n];
		Y[i] = 1;

		float z = polynomeDeLagrangeEvaluation(X, Y, u);
		return z;
	}


}
