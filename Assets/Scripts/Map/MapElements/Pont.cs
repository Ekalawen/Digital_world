using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pont : CubeEnsemble {

    public Vector3 depart;
    public Vector3 direction;
    public int nbCubes;

    public Pont(Vector3 depart, Vector3 direction, int nbCubes) : base() {
        this.depart = depart;
        this.direction = direction;
        this.nbCubes= nbCubes;

        GenererPont();
    }

    public Pont CreatePontWithPoints(Vector3 depart, Vector3 arriveeDirection, int nbCubes) {
        return new Pont(
            depart,
            (arriveeDirection - depart) / (float)(nbCubes - 1),
            nbCubes);
    }

    // Génère un pont qui part d'un point de départ dans une direction et sur une certaine distance !
    public void GenererPont() {
		for(int i = 0; i < nbCubes; i++) {
			Vector3 pos = depart + direction * i;
            CreateCube(pos);
		}
	}
}
