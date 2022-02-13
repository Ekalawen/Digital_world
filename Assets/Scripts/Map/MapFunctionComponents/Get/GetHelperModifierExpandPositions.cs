using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierExpandPositions : GetHelperModifier {

    public enum DistanceMode { L2, CUBE, L_INFINI };
    public DistanceMode mode = DistanceMode.CUBE;
    public float distance = 1;

    public override List<Cube> ModifyCubes(List<Cube> cubes) {
        return ExpandCubes(cubes);
    }

    public override List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return ExpandPositions(positions);
    }

    public override bool IsInArea(Vector3 position) {
        // Not used !
        return false;
    }

    protected List<Cube> ExpandCubes(List<Cube> cubes) {
        MapManager map = GameManager.Instance.map;
        List<Cube> expandedCubes = new List<Cube>();
        switch (mode) {
            case DistanceMode.L2:
                foreach(Cube cube in cubes) {
                    expandedCubes.AddRange(map.GetCubesInSphere(cube.transform.position, distance));
                }
                break;
            case DistanceMode.CUBE:
                foreach(Cube cube in cubes) {
                    expandedCubes.AddRange(map.GetCubesAtLessThanCubeDistance(cube.transform.position, (int)distance));
                }
                break;
            case DistanceMode.L_INFINI:
                foreach(Cube cube in cubes) {
                    expandedCubes.AddRange(map.GetCubesInBox(cube.transform.position, Vector3.one * distance / 2));
                }
                break;
        }
        expandedCubes = expandedCubes.Distinct().ToList();
        return expandedCubes;
    }

    protected List<Vector3> ExpandPositions(List<Vector3> positions) {
        MapManager map = GameManager.Instance.map;
        List<Vector3> expandedPositions = new List<Vector3>();
        switch (mode) {
            case DistanceMode.L2:
                foreach(Vector3 position in positions) {
                    expandedPositions.AddRange(map.GetEmptyPositionsInSphere(position, distance));
                }
                break;
            case DistanceMode.CUBE:
                foreach(Vector3 position in positions) {
                    expandedPositions.AddRange(map.GetEmptyPositionsAtLessThanCubeDistance(position, (int)distance));
                }
                break;
            case DistanceMode.L_INFINI:
                foreach(Vector3 position in positions) {
                    expandedPositions.AddRange(map.GetEmptyPositionsInBox(position, Vector3.one * distance / 2));
                }
                break;
        }
        expandedPositions = expandedPositions.Distinct().ToList();
        return expandedPositions;
    }
}
