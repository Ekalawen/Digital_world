using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Permet de générer une carte à l'intérieur d'un grand cube !
public class ParkourTrainingMap : CubeMap {

    public int nbPonts = 10;
    public Vector2Int rangeSizePont = new Vector2Int(3, 5);

    public bool pontOnlyHorizontalSurrounding = false;
    public int pontsOffsetFromSides = 2;
    public int nbCouronnesOfLumieres = 2;
    public int offsetCouronnesOfLumieres = 2;
    public int nbRandomCubes = 30;

    protected override void GenerateMap() {
        GenerateParkourTrainingMap();
    }

    protected void GenerateParkourTrainingMap() {
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) + Vector3.one);

        List<FullBlock> fullBlocks = GenerateNumberedRandomFilling();

        AddLumiereOnTopOfFullBlocks(fullBlocks);

        /// Parties d'autres versions !
        //GenerateHorizontalPonts();
        //GenerateCouronneOfLumieres();
        //List<Vector3> cycle = GenerateHamiltonienCycle(fullBlocks);
        //LinkCycleWithLumieres(cycle);
    }

    protected void GenerateCouronneOfLumieres() {
        int hauteur = 1;
        for(int i = 0; i < nbCouronnesOfLumieres; i++) {
            hauteur += offsetCouronnesOfLumieres;
            List<Vector3> posCouronne = GetCouronne(hauteur, offsetSides: 1);
            foreach (Vector3 pos in posCouronne)
                CreateLumiere(pos, Lumiere.LumiereType.NORMAL);
        }
    }

    protected void GenerateHorizontalPonts() {
        for(int i = 0; i < nbPonts; i++) {
            GenerateHorizontalPont();
        }

        LinkUnreachableLumiereToRestForPonts();
    }

    protected void LinkUnreachableLumiereToRestForPonts() {
        List<Vector3> reachableArea = GetReachableArea();
        foreach(Lumiere lumiere in lumieres) {
            LinkPositionToReachableArea(lumiere.transform.position, reachableArea);
            LinkPositionToReachableArea(lumiere.transform.position + Vector3.up, reachableArea);

            // Et surelever les lumières !
            lumiere.transform.position = lumiere.transform.position + 0.5f * Vector3.up;
        }
    }

    protected void GenerateHorizontalPont() {
        // Selectionner une case de départ qui ne soit pas collé aux paroies
        Vector3 depart = GetFreeRoundedLocation(offsetFromSides: pontsOffsetFromSides);

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
        while(IsInInsidedRegularMap(current, offsetFromSides: pontsOffsetFromSides)) {
            nbCubesMax++;
            current += direction;
        }
        return nbCubesMax;
    }

    protected void AddLumiereOnTopOfFullBlocks(List<FullBlock> fullBlocks) {
        List<Vector3> reachableArea = GetReachableArea();
        foreach(FullBlock fb in fullBlocks) {
            Vector3 onTop = fb.GetOnTop();
            CreateLumiere(onTop, Lumiere.LumiereType.NORMAL);
            LinkPositionToReachableArea(onTop, reachableArea);
        }
    }

    protected List<FullBlock> GenerateNumberedRandomFilling() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = GetFarAwayPositions();
        List<Vector3> selectedPos = GaussianGenerator.SelecteSomeNumberOf(farAwayPos, nbRandomCubes);
        foreach (Vector3 pos in selectedPos) {
            Vector3 finalPos = pos - Vector3.one * (int)Mathf.Floor(sizeCubeRandomFilling / 2.0f);
            FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling);
            fullBlocks.Add(fb);
        }
        return fullBlocks;
    }

    protected List<Vector3> GenerateHamiltonienCycle(List<FullBlock> fullBlocks) {
        List<Vector3> positions = new List<Vector3>();
        foreach (FullBlock fb in fullBlocks) positions.Add(fb.GetOnTop());

        Graphe<Vector3> graphe = new Graphe<Vector3>();
        foreach(Vector3 pos in positions) {
            List<Vector3> voisins = GetAccessiblesFromJump(pos, positions);
            graphe[pos] = voisins;
        }

        List<Vector3> cycle = graphe.CycleHamiltonien(positions[0]);
        foreach(Vector3 pos in cycle) {
            Debug.Log("pos = " + pos);
        }
        return cycle;
    }

    protected List<Vector3> GetAccessiblesFromJump(Vector3 depart, List<Vector3> destinations) {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 destination in destinations) {
            if (IsAccessibleFromJump(depart, destination))
                res.Add(destination);
        }
        return res;
    }

    protected bool IsAccessibleFromJump(Vector3 depart, Vector3 destination) {
        // TODO !
        return Vector3.Distance(depart, destination) <= 10;
    }

    protected void LinkCycleWithLumieres(List<Vector3> cycle) {
        for(int i = 0; i < cycle.Count; i++) {
            Vector3 depart = cycle[i];
            Vector3 arrivee = cycle[(i + 1) % cycle.Count];
            CreateLumiereLine(depart, arrivee);
        }
    }

    protected void CreateLumiereLine(Vector3 depart, Vector3 arrivee) {
        float distance = Vector3.Distance(depart, arrivee);
        int n = (int)Mathf.Max(1, Mathf.Floor(distance));
        Vector3 pas = (arrivee - depart) / n;
        for(int i = 0; i < n; i++) {
            Vector3 pos = depart + pas * i;
            if(GetCubeAt(MathTools.Round(pos)) == null)
                CreateLumiere(pos, Lumiere.LumiereType.NORMAL, dontRoundPositions: true);
        }
    }
}
