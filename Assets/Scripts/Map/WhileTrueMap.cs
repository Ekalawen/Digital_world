using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhileTrueMap : CubeMap {

    public int nbCaves = 3;
    public int nbPonts = 50;

    protected override void GenerateMap() {
		volumeMap = (int) Mathf.Pow (tailleMap, 3);
		GenerateWhileTrueMap();
    }

    private void GenerateWhileTrueMap() {
        // On crée le contour de la map !
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap, tailleMap, tailleMap));

        // On veut créer des passerelles entre les sources ! <3
        // On définit les cubes qui seront à l'origine de passerelles
        List<Cube> sourcesPonts = new List<Cube>();
        int N = mapContainer.GetCubes().Count;
        int nbSources = nbPonts * 2;
        for (int i = 0; i < nbSources; i++) {
            Cube cube = mapContainer.GetCubes()[Random.Range(0, N)];
            sourcesPonts.Add(cube);
        }
        // Puis on les relis 2 à 2
        GeneratePont(sourcesPonts);

        // On veut générer des caves dangeureuses :3
        // Qui seront pleines de lumières ! :)
        int tailleMaxCave = tailleMinCave;
        List<Cave> caves = GenerateCaves(nbCaves, tailleMinCave, tailleMaxCave, bWithLumieres: true);
    }

	List<Cave> GenerateCaves(float proportionCaves, int tailleMinCave, int tailleMaxCave, bool bWithLumieres) {
        List<Cave> caves = new List<Cave>();
		for (int k = 0; k < nbCaves; k++) {
            // On définit la taille de la cave
            Vector3Int size = Vector3Int.zero;
			size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
			size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
			size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

            // On définit sa position sur la carte
            Vector3 position = new Vector3(Random.Range(2, tailleMap - size.x - 1),
                2, // On le force à être atteignable en sautant =)
                Random.Range(2, tailleMap - size.z - 1));

            Cave cave = new Cave(position, size, bMakeSpaceArround: true, bDigInside: true);
            caves.Add(cave);

            // On y rajoute la lumière !
            cave.AddAllLumiereInside();
		}

        return caves;
	}
}
