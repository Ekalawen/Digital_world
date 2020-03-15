using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyFloorCubeMap : CubeMap {

    public bool isBrisableFloor = false;

    protected override void GenerateCubeMap() {
        // On crée le sol de la map !
        if(isBrisableFloor)
            currentCubeTypeUsed = Cube.CubeType.SPECIAL;
        Mur floor = new Mur(Vector3.zero, Vector3.right, tailleMap.x + 1, Vector3.forward, tailleMap.y + 1);

        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        currentCubeTypeUsed = Cube.CubeType.NORMAL;
        List<Cave> caves = GenerateCaves(proportionCaves, bWithLumieres: true);
    }

    protected override Vector3 GetPositionCave(Vector3Int sizeCave) {
        if (caveOffsetSides) {
            return new Vector3(Random.Range(2, tailleMap.x - sizeCave.x),
                Random.Range(2, tailleMap.y - sizeCave.y),
                Random.Range(2, tailleMap.z - sizeCave.z));
        } else {
            return new Vector3(Random.Range(0, tailleMap.x - sizeCave.x + 2),
                Random.Range(1, tailleMap.y - sizeCave.y + 2), // On autorize pas la cave à être complètement au sol !
                Random.Range(0, tailleMap.z - sizeCave.z + 2));
        }
    }
}
