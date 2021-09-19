using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GetCubesHelper : MonoBehaviour {
    public enum HowToGetCubes {
        ALL,
        OF_TYPE,
        IN_BOX_AREA,
        IN_SPHERE_AREA,
        REGULAR,
        NOT_REGULAR,
        IN_CUBE_ENSEMBLES,
        CAVES_OPENINGS,
        WITH_N_VOISINS,
    };

    public enum HowToGetCubesInCubesEnsembles {
        ALL,
        FIX_QUANTITY,
        PROPORTION,
    }


    public HowToGetCubes howToGetCubes = HowToGetCubes.ALL;
    [ConditionalHide("howToGetCubes", HowToGetCubes.OF_TYPE)]
    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_BOX_AREA)]
    public Vector3 areaBoxCenter = Vector3.zero;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_BOX_AREA)]
    public Vector3 areaBoxHalfExtents = Vector3.zero;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_SPHERE_AREA)]
    public Vector3 areaSphereCenter = Vector3.zero;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_SPHERE_AREA)]
    public float areaSphereRadius = 0f;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_CUBE_ENSEMBLES)]
    public CubeEnsemble.CubeEnsembleType cubeEnsembleType;
    [ConditionalHide("howToGetCubes", HowToGetCubes.IN_CUBE_ENSEMBLES)]
    public HowToGetCubesInCubesEnsembles howToGetCubesInCubeEnsemble;
    [ConditionalHide("howToGetCubesInCubeEnsemble", HowToGetCubesInCubesEnsembles.FIX_QUANTITY)]
    public int cubeEnsembleQuantity = 1;
    [ConditionalHide("howToGetCubesInCubeEnsemble", HowToGetCubesInCubesEnsembles.PROPORTION)]
    public float cubeEnsembleProportion = 0.5f;
    [ConditionalHide("howToGetCubes", HowToGetCubes.WITH_N_VOISINS)]
    public int nbCubesVoisins = 4;
    public List<GetHelperModifier> modifiers;

    protected MapManager map;

    public List<Cube> Get()
    {
        map = GameManager.Instance.map;
        List<Cube> cubes = BasicGet();
        cubes = ApplyModifier(cubes);
        return cubes;
    }

    protected List<Cube> ApplyModifier(List<Cube> cubes) {
        foreach (GetHelperModifier modifier in modifiers) {
            cubes = modifier.ModifyCubes(cubes);
        }
        return cubes;
    }

    protected List<Cube> BasicGet() {
        switch (howToGetCubes) {
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
            case HowToGetCubes.CAVES_OPENINGS:
                return GetCubesInCavesOpenings();
            case HowToGetCubes.WITH_N_VOISINS:
                return GetCubesWithNVoisins(nbCubesVoisins);
            default:
                return null;
        }
    }

    protected List<Cube> GetAllCubes() {
        return map.GetAllCubes();
    }

    protected List<Cube> GetCubesNotRegular() {
        return map.GetAllNonRegularCubes();
    }

    protected List<Cube> GetCubesRegular() {
        return map.GetAllRegularCubes();
    }

    protected List<Cube> GetCubesInBoxArea() {
        return map.GetCubesInBox(areaBoxCenter, areaBoxHalfExtents);
    }

    protected List<Cube> GetCubesInSphereArea() {
        return map.GetCubesInSphere(areaSphereCenter, areaSphereRadius);
    }

    protected List<Cube> GetCubesOfType() {
        return map.GetAllCubesOfType(cubeType);
    }

    protected List<Cube> GetCubesInCubeEnsembles() {
        List<CubeEnsemble> cubeEnsembles = map.GetCubeEnsemblesOfType(cubeEnsembleType);
        List<Cube> cubes = new List<Cube>();
        if (howToGetCubesInCubeEnsemble == HowToGetCubesInCubesEnsembles.ALL) {
            cubes.AddRange(cubeEnsembles.SelectMany(ce => ce.GetCubes()));
        } else if (howToGetCubesInCubeEnsemble == HowToGetCubesInCubesEnsembles.FIX_QUANTITY) {
            MathTools.Shuffle(cubeEnsembles);
            cubeEnsembles = cubeEnsembles.Take(cubeEnsembleQuantity).ToList();
            cubes.AddRange(cubeEnsembles.SelectMany(ce => ce.GetCubes()));
        } else { // HowToGetCubesInCubesEnsembles.PROPORTION
            MathTools.Shuffle(cubeEnsembles);
            cubeEnsembles = cubeEnsembles.Take(Mathf.RoundToInt(cubeEnsembles.Count * cubeEnsembleProportion)).ToList();
            cubes.AddRange(cubeEnsembles.SelectMany(ce => ce.GetCubes()));
        }
        return cubes;
    }

    protected List<Cube> GetCubesInCavesOpenings() {
        List<CubeEnsemble> cubeEnsembles = map.GetCubeEnsemblesOfType(CubeEnsemble.CubeEnsembleType.CAVE);
        List<Cave> caves = cubeEnsembles.Select(c => (Cave)c).ToList();
        return caves.SelectMany(c => c.GetOpenings()).ToList();
    }

    protected List<Cube> GetCubesWithNVoisins(int n) {
        List<Cube> cubes = map.GetAllCubes().FindAll(c => map.GetVoisinsPleinsAll(c.transform.position).Count == n).ToList();
        return cubes;
    }
}
