using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GetCubesHelper {
    public enum HowToGetCubes {
        ALL,
        OF_TYPE,
        IN_BOX_AREA,
        IN_SPHERE_AREA,
        REGULAR,
        NOT_REGULAR,
        IN_CUBE_ENSEMBLES,
    };

    public HowToGetCubes howToGetCubes = HowToGetCubes.ALL;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.OF_TYPE)]
    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.IN_BOX_AREA)]
    public Vector3 areaBoxCenter = Vector3.zero;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.IN_BOX_AREA)]
    public Vector3 areaBoxHalfExtents = Vector3.zero;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.IN_SPHERE_AREA)]
    public Vector3 areaSphereCenter = Vector3.zero;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.IN_SPHERE_AREA)]
    public float areaSphereRadius = 0f;
    [ConditionalHide("!howToGetCubes", HowToGetCubes.IN_CUBE_ENSEMBLES)]
    public CubeEnsemble.CubeEnsembleType cubeEnsembleType;

    public List<Cube> Get() {
        switch(howToGetCubes) {
            case HowToGetCubes.ALL:
                return GetAllCubes();
            case HowToGetCubes.OF_TYPE:
                return GetCubesOfType();
            case HowToGetCubes.IN_BOX_AREA:
                return GetCubesInBoxArea();
            case HowToGetCubes.IN_SPHERE_AREA:
                return GetCubesInSphereArea();
            case HowToGetCubes.REGULAR:
                return GetCubesRegular();
            case HowToGetCubes.NOT_REGULAR:
                return GetCubesNotRegular();
            case HowToGetCubes.IN_CUBE_ENSEMBLES:
                return GetCubesInCubeEnsembles();
            default:
                return null;
        }
    }

    protected List<Cube> GetAllCubes() {
        MapManager map = GameManager.Instance.map;
        return map.GetAllCubes();
    }

    protected List<Cube> GetCubesNotRegular() {
        MapManager map = GameManager.Instance.map;
        return map.GetAllNonRegularCubes();
    }

    protected List<Cube> GetCubesRegular() {
        MapManager map = GameManager.Instance.map;
        return map.GetAllRegularCubes();
    }

    protected List<Cube> GetCubesInBoxArea() {
        MapManager map = GameManager.Instance.map;
        return map.GetCubesInBox(areaBoxCenter, areaBoxHalfExtents);
    }

    protected List<Cube> GetCubesInSphereArea() {
        MapManager map = GameManager.Instance.map;
        return map.GetCubesInSphere(areaSphereCenter, areaSphereRadius);
    }

    protected List<Cube> GetCubesOfType() {
        MapManager map = GameManager.Instance.map;
        return map.GetAllCubesOfType(cubeType);
    }

    protected List<Cube> GetCubesInCubeEnsembles() {
        MapManager map = GameManager.Instance.map;
        List<CubeEnsemble> cubeEnsembles = map.GetCubeEnsemblesOfType(cubeEnsembleType);
        List<Cube> cubes = new List<Cube>();
        foreach (CubeEnsemble cubeEnsemble in cubeEnsembles)
            cubes.AddRange(cubeEnsemble.GetCubes());
        return cubes;
    }
}
