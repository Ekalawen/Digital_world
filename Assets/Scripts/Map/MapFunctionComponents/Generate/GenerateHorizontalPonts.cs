using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateHorizontalPonts : GenerateCubesMapFunction {

    public Lumiere.LumiereType lumiereType = Lumiere.LumiereType.NORMAL;
    public int nbPonts = 10;
    public Vector2Int rangeSizePont = new Vector2Int(3, 5);
    public bool pontOnlyHorizontalSurrounding = false;
    public int pontsOffsetFromSides = 2;

    public override void Activate() {
        GenerateTheHorizontalPonts();
    }

    protected void GenerateTheHorizontalPonts() {
        for(int i = 0; i < nbPonts; i++) {
            GenerateHorizontalPont();
        }

        LinkUnreachableLumiereToRestForPonts();
    }

    protected void GenerateHorizontalPont() {
        // Selectionner une case de départ qui ne soit pas collé aux paroies
        Vector3 depart = map.GetFreeRoundedLocation(offsetFromSides: pontsOffsetFromSides);

        // Selectionner une direction
        Vector3 direction = GetHorizontalDirection(depart);

        // Selectionner le nombre de cubes du pont
        int nbCubes = GetNbCubesForPont(depart, direction);

        Debug.LogFormat("depart = {0} direction = {1} nbCubes = {2}", depart, direction, nbCubes);

        // Créer le pont !
        Pont pont = new Pont(depart, direction, nbCubes);
        // Et un autre au dessus !
        Pont pontAuDessus = new Pont(depart + Vector3.up, direction, nbCubes);

        // L'entourer de lumières
        List<Lumiere> lumieres = pont.SurroundWithLumieres(Lumiere.LumiereType.NORMAL, onlyHorizontaly: true);
    }

    protected Vector3 GetHorizontalDirection(Vector3 depart) {
        Vector3[] directions = new Vector3[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        while (true) {
            int ind = UnityEngine.Random.Range(0, directions.Length);
            Vector3 direction = directions[ind];
            if (ComputeNbCubesMaxForPont(depart, direction) >= rangeSizePont[0])
                return direction;
        }
    }

    // Assumed depart and direction are rounded !
    protected int GetNbCubesForPont(Vector3 depart, Vector3 direction) {
        int nbCubesMax = ComputeNbCubesMaxForPont(depart, direction);
        return UnityEngine.Random.Range(rangeSizePont[0], (int)Mathf.Min(nbCubesMax, rangeSizePont[1]) + 1);
    }

    protected int ComputeNbCubesMaxForPont(Vector3 depart, Vector3 direction) {
        Vector3 current = depart;
        int nbCubesMax = 0;
        while(map.IsInInsidedRegularMap(current, offsetFromSides: pontsOffsetFromSides)) {
            nbCubesMax++;
            current += direction;
        }
        return nbCubesMax;
    }

    // Important car les lumières sont à des positions non-entières !!!
    protected void LinkUnreachableLumiereToRestForPonts() {
        List<Vector3> reachableArea = map.GetReachableArea();
        foreach(Lumiere lumiere in map.GetLumieres()) {
            map.LinkPositionToReachableArea(lumiere.transform.position, reachableArea);
            map.LinkPositionToReachableArea(lumiere.transform.position + Vector3.up, reachableArea);

            // Et surelever les lumières !
            lumiere.transform.position = lumiere.transform.position + 0.5f * Vector3.up;
        }
    }
}
