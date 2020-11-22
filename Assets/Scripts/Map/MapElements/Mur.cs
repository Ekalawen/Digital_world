using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remplit un mur qui part d'un point de départ dans 2 directions avec une certaine taille selon les 2 directions // C'est clair ? Non ? Bah lit le code <3
public class Mur : CubeEnsemble {
    public Vector3 depart;
    public Vector3 direction1;
    public int nbCubesInDirection1;
    public Vector3 direction2;
    public int nbCubesInDirection2;

    public Mur(Vector3 depart, Vector3 direction1, int nbCubesInDirection1,
                               Vector3 direction2, int nbCubesInDirection2) : base() {
        this.depart = depart;
        this.direction1 = direction1;
        this.nbCubesInDirection1 = nbCubesInDirection1;
        this.direction2 = direction2;
        this.nbCubesInDirection2 = nbCubesInDirection2;

        GenererMur();
    }

    public override string GetName() {
        return "Mur";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.MUR;
    }

    public static Mur CreateMurWithPoints(Vector3 depart, Vector3 arriveeDirection1, int nbCubesInDirection1,
                                                          Vector3 arriveeDirection2, int nbCubesInDirection2)
    {
        return new Mur(
            depart,
            (arriveeDirection1 - depart) / (float)(nbCubesInDirection1 - 1),
            nbCubesInDirection1,
            (arriveeDirection2 - depart) / (float)(nbCubesInDirection2 - 1),
            nbCubesInDirection2);
    }

    protected void GenererMur() {
		// On remplit tout
		for(int i = 0; i < nbCubesInDirection1; i++) {
			for(int j = 0; j < nbCubesInDirection2; j++) {
                Vector3 pos = depart + direction1 * i + direction2 * j;
                CreateCube(pos);
			}
		}
	}
}
