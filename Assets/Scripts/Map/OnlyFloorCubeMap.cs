using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyFloorCubeMap : CubeMap {

    public bool isBrisableFloor = false;
    public float minDistanceRandomFilling = 2.5f;
    public float proportionRandomFilling = 0.02f;
    public int sizeCubeRandomFilling = 1; // Ca peut être intéressant d'augmenter cette taill ! :)

    protected override void GenerateCubeMap() {
        // On crée le sol de la map !
        if(isBrisableFloor)
            currentCubeTypeUsed = Cube.CubeType.SPECIAL;
        Mur floor = new Mur(Vector3.zero, Vector3.right, tailleMap.x + 1, Vector3.forward, tailleMap.y + 1);

        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        currentCubeTypeUsed = Cube.CubeType.NORMAL;
        List<Cave> caves = GenerateCaves(proportionCaves, bWithLumieres: true);

        // On mets des cubes unitaires aux endroits où il n'y en a pas beaucoup ! :)
        GenerateRandomFilling();
    }

    protected void GenerateRandomFilling() {
        List<Vector3> farAwayPos = GetFarAwayPositions();
        List<Vector3> selectedPos = GaussianGenerator.SelectSomeProportionOf<Vector3>(farAwayPos, proportionRandomFilling);
        foreach(Vector3 pos in selectedPos) {
            Vector3 finalPos = pos - Vector3.one * (int)Mathf.Floor(sizeCubeRandomFilling / 2.0f);
            FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling);
        }
    }

    protected List<Vector3> GetFarAwayPositions() {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 pos in GetAllEmptyPositions()) {
            List<Cube> nearCubes = GetCubesInSphere(pos, minDistanceRandomFilling);
            if (nearCubes.Count == 0)
                res.Add(pos);
        }
        return res;
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
