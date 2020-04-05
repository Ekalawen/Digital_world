using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCaves : GenerateCubesMapFunction {

	public float proportionCaves;
    public int tailleMinCave = 3;
    public int tailleMaxCave = 10;
    public bool makeSpaceArround = false;
    public int nbLumieresPerCaves = 1;
    public int offsetLumieresFromCenter = 1;
    public bool caveOffsetSides = true;

    public override void Activate() {
        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        List<Cave> caves = GenerateAllCaves(proportionCaves, bWithLumieres: true, bMakeSpaceArround: makeSpaceArround);
    }

	protected List<Cave> GenerateAllCaves(float proportionCaves, bool bWithLumieres, bool bMakeSpaceArround = false) {
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
            Vector3 position = GetPositionCave(size);

            Cave cave = new Cave(position, size, bMakeSpaceArround: bMakeSpaceArround, bDigInside: true);
            caves.Add(cave);

            // On y rajoute la lumière !
            cave.AddNLumiereInside(nbLumieresPerCaves, offsetLumieresFromCenter);

            volumeCaves += cave.GetVolume();
            currentProportion = volumeCaves / map.GetVolume();
		}

        return caves;
	}

    protected virtual Vector3 GetPositionCave(Vector3Int sizeCave) {
        if (caveOffsetSides) {
            return new Vector3(Random.Range(2, map.tailleMap.x - sizeCave.x),
                Random.Range(2, map.tailleMap.y - sizeCave.y),
                Random.Range(2, map.tailleMap.z - sizeCave.z));
        } else {
            return new Vector3(Random.Range(0, map.tailleMap.x - sizeCave.x + 2),
                Random.Range(0, map.tailleMap.y - sizeCave.y + 2),
                Random.Range(0, map.tailleMap.z - sizeCave.z + 2));
        }
    }
}
