using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de générer une carte à l'intérieur d'un grand cube !
public class CubeMap : MapManager {

	public float proportionCaves;
    public float proportionSourcePont;
    public int tailleMinCave = 3;
    public int nbLumieresPerCaves = 1;

	[HideInInspector] public int volumeMap;

    protected override void GenerateMap() {
		volumeMap = (int) Mathf.Pow (tailleMap, 3);
		GenerateCubeMap();
    }

    // Crée une map en forme de Cube
    void GenerateCubeMap() {
        // On crée le contour de la map !
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap, tailleMap, tailleMap));

        // On veut créer des passerelles entre les sources ! <3
        // On définit les cubes qui seront à l'origine de passerelles
        List<Cube> sourcesPonts = new List<Cube>();
        int N = mapContainer.GetCubes().Count;
        float P = proportionSourcePont;
        int nbSources = (int)Mathf.Round(GaussianGenerator.Next(N * P, N * P * (P - 1), 0, N));
        Debug.Log("nombre de sources de ponts = " + nbSources);
        for (int i = 0; i < nbSources; i++)
        {
            Cube cube = mapContainer.GetCubes()[Random.Range(0, N)];
            sourcesPonts.Add(cube);
        }
        // Puis on les relis 2 à 2
        GeneratePont(sourcesPonts);

        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        int tailleMaxCave = tailleMap / 2;
        //int volumeCaveMoyen = (int)Mathf.Pow((tailleMaxCave + tailleMinCave) / 2.0f, 3);
        //int nbCaves = (int)Mathf.Ceil(proportionCaves * volumeMap / volumeCaveMoyen);
        List<Cave> caves = GenerateCaves(proportionCaves, tailleMinCave, tailleMaxCave, bWithLumieres: true);
    }

	List<Cave> GenerateCaves(float proportionCaves, int tailleMinCave, int tailleMaxCave, bool bWithLumieres) {
        List<Cave> caves = new List<Cave>();
        float currentProportion = 0.0f;
        float volumeCaves = 0;
        while(currentProportion < proportionCaves) {
		//for (int k = 0; k < nbCaves; k++) {
            // On définit la taille de la cave
            Vector3Int size = Vector3Int.zero;
			size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
			size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
			size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

            // On définit sa position sur la carte
            Vector3 position = new Vector3(Random.Range(1, tailleMap - size.x),
                Random.Range(1, tailleMap - size.y),
                Random.Range(1, tailleMap - size.z));

            Cave cave = new Cave(position, size, bMakeSpaceArround: false, bDigInside: true);
            caves.Add(cave);

            // On y rajoute la lumière !
            cave.AddNLumiereInside(nbLumieresPerCaves);

            volumeCaves += cave.GetVolume();
            currentProportion = volumeCaves / GetVolume();
		}

        return caves;
	}

	// Génère des ponts entre les sources ! =)
	void GeneratePont(List<Cube> sources) {
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
			|| (debut.x == tailleMap && fin.x == tailleMap)
			|| (debut.y == 0 && fin.y == 0)
			|| (debut.y == tailleMap && fin.y == tailleMap)
			|| (debut.z == 0 && fin.z == 0)
			|| (debut.z == tailleMap && fin.z == tailleMap)) {
			return true;
		} else {
			return false;
		}
	}
}
