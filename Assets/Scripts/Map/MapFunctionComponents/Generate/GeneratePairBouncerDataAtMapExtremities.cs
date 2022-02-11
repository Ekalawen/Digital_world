using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratePairBouncerDataAtMapExtremities : GenerateCubesMapFunction {

    [Header("Cube creation")]
    [Range(0, 6)]
    public int nbDirections = 1;
    public float voidCubeExplosionDistance = 5;
    public bool makeCreatedCubeAccessible = true;
    [ConditionalHide("makeCreatedCubeAccessible")]
    public Cube.CubeType cubeTypeToMakeAccessible = Cube.CubeType.NORMAL;
    [ConditionalHide("makeCreatedCubeAccessible")]
    public int distanceFromCreatedCube = 2;

    [Header("LumiereCreation")]
    public Lumiere.LumiereType lumiereType = Lumiere.LumiereType.NORMAL;
    public int distanceOfLumiereFromCube = 9;

    protected List<Cube> accessibleCubes;

    public override void Activate() {
        accessibleCubes = ComputeAccessibleCubes();
        List<Vector3> directions = MathTools.GetAllOrthogonalNormals();
        for(int i = 0; i < nbDirections; i++) {
            Vector3 direction = MathTools.ChoseOne(directions);
            CreatePairInDirection(direction);
            directions.Remove(direction);
        }
    }

    protected void CreatePairInDirection(Vector3 direction) {
        Cube bestCube = GetBestCubeForDirection(direction);
        bestCube = map.SwapCubeType(bestCube, cubeType);
        Vector3 lumierePos = bestCube.transform.position + direction * distanceOfLumiereFromCube;
        map.CreateLumiere(lumierePos, lumiereType);
        MakeCreatedCubeAccessible(bestCube, direction);
    }

    protected void MakeCreatedCubeAccessible(Cube createdCube, Vector3 direction) {
        if (!makeCreatedCubeAccessible) {
            return;
        }
        // We know that direction is an orthogonal vector
        Vector3 secondDirection = Vector3.up;
        if(direction == secondDirection || direction == -secondDirection) {
            secondDirection = Vector3.right;
        }
        Vector3 thirdDirection = Vector3.Cross(direction, secondDirection).normalized;
        map.AddCube(createdCube.transform.position + secondDirection * distanceFromCreatedCube, cubeTypeToMakeAccessible);
        map.AddCube(createdCube.transform.position - secondDirection * distanceFromCreatedCube, cubeTypeToMakeAccessible);
        map.AddCube(createdCube.transform.position + thirdDirection * distanceFromCreatedCube, cubeTypeToMakeAccessible);
        map.AddCube(createdCube.transform.position - thirdDirection * distanceFromCreatedCube, cubeTypeToMakeAccessible);
    }

    protected Cube GetBestCubeForDirection(Vector3 direction) {
        return accessibleCubes.OrderBy(c => Vector3.Dot(direction, c.transform.position)).Last();
    }

    private List<Cube> ComputeAccessibleCubes() {
        List<Cube.CubeType> notAccessibleCubesType = new List<Cube.CubeType>() { Cube.CubeType.VOID, Cube.CubeType.BRISABLE, Cube.CubeType.DEATH };
        List<Cube> allCubes = map.GetAllCubes();
        List<Cube> voidCubes = allCubes.FindAll(c => c.type == Cube.CubeType.VOID);
        List<Cube> accessibleCubes = allCubes.FindAll(c => !notAccessibleCubesType.Contains(c.type));
        float distSquared = voidCubeExplosionDistance * voidCubeExplosionDistance;
        if (voidCubes.Count > 0) {
            accessibleCubes = accessibleCubes.FindAll(c => MinDistanceSquaredToVoidCubes(c, voidCubes) > distSquared);
        }
        return accessibleCubes;
    }

    protected float MinDistanceSquaredToVoidCubes(Cube cube, List<Cube> voidCubes) {
        return voidCubes.Select(vc => Vector3.SqrMagnitude(vc.transform.position - cube.transform.position)).Min();
    }
}
