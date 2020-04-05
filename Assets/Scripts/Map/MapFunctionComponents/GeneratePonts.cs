using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePonts : GenerateCubesMapFunction {

    public float proportionSourcePont = 0.0f;

    public override void Activate() {
        // On veut créer des passerelles entre les sources ! <3
        // On définit les cubes qui seront à l'origine de passerelles
        List<Cube> sourcesPonts = GenerateSourcesPonts();

        // Puis on les relis 2 à 2
        GeneratePont(sourcesPonts);
    }

    protected List<Cube> GenerateSourcesPonts() {
        List<Cube> sourcesPonts = new List<Cube>();
        int N = map.GetAllCubes().Count;
        float P = proportionSourcePont;
        int nbSources = (int)Mathf.Round(GaussianGenerator.Next(N * P, N * P * (P - 1), 0, N));
        Debug.Log("nombre de sources de ponts = " + nbSources);
        for (int i = 0; i < nbSources; i++)
        {
            Cube cube = map.GetAllCubes()[Random.Range(0, N)];
            sourcesPonts.Add(cube);
        }
        return sourcesPonts;
    }

	// Génère des ponts entre les sources ! =)
	protected void GeneratePont(List<Cube> sources) {
        while(sources.Count >= 2) {
			// On récupère les deux sources qui nous intéressent
			int n = Random.Range (0, sources.Count);
			Cube c1 = sources[n];
			sources.Remove (c1);
			n = Random.Range (0, sources.Count);
			Cube c2 = sources[n];
			sources.Remove (c2);

			// Si le pont n'est pas dans un mur ...
			if (!BridgeInWall(c1.transform.position, c2.transform.position)) {
                Vector3 direction = (c2.transform.position - c1.transform.position);
                int nbCubes = (int)Mathf.Round(direction.magnitude);
                Pont pont = new Pont(c1.transform.position, direction.normalized, nbCubes);
			}
		}
	}

	// Permet de savoir si un pont est dans un mur ou non
	bool BridgeInWall(Vector3 debut, Vector3 fin) {
        // TO DO AGAIN car ça ne sera plus à jour !
		if((debut.x == 0 && fin.x == 0)
			|| (debut.x == map.tailleMap.x && fin.x == map.tailleMap.x)
			|| (debut.y == 0 && fin.y == 0)
			|| (debut.y == map.tailleMap.y && fin.y == map.tailleMap.y)
			|| (debut.z == 0 && fin.z == 0)
			|| (debut.z == map.tailleMap.z && fin.z == map.tailleMap.z)) {
			return true;
		} else {
			return false;
		}
	}

}
