using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GetEmptyPositionsHelper : MonoBehaviour {
    public enum HowToGetPositions {
        ALL,
        WITH_N_CUBES_VOISINS,
        IN_PASSAGES,
        OPTIMALY_SPACED,
        CENTER,
        IN_BOX_AREA,
        IN_SPHERE_AREA,
        UNION,
        OF_DATA,
        IN_CAVES,
    };

    public HowToGetPositions howToGetPositions = HowToGetPositions.ALL;
    [ConditionalHide("howToGetPositions", HowToGetPositions.WITH_N_CUBES_VOISINS)]
    public int nbCubesVoisins = 4;
    [ConditionalHide("howToGetPositions", HowToGetPositions.OPTIMALY_SPACED)]
    public int optimalySpacedNbPositions;
    [ConditionalHide("howToGetPositions", HowToGetPositions.OPTIMALY_SPACED)]
    public int optimalySpacedNbTriesByPosition;
    [ConditionalHide("howToGetPositions", HowToGetPositions.OPTIMALY_SPACED)]
    public GetOptimalySpacedPositions.Mode optimalySpacedMode;
    [ConditionalHide("howToGetPositions", HowToGetPositions.IN_BOX_AREA)]
    public Vector3 areaBoxCenter = Vector3.zero;
    [ConditionalHide("howToGetPositions", HowToGetPositions.IN_BOX_AREA)]
    public Vector3 areaBoxHalfExtents = Vector3.zero;
    [ConditionalHide("howToGetPositions", HowToGetPositions.IN_SPHERE_AREA)]
    public Vector3 areaSphereCenter = Vector3.zero;
    [ConditionalHide("howToGetPositions", HowToGetPositions.IN_SPHERE_AREA)]
    public float areaSphereRadius = 0f;
    [ConditionalHide("howToGetPositions", HowToGetPositions.IN_CAVES)]
    public int caveOffsetFromSides = 0;
    public List<GetHelperModifier> modifiers;
    public List<GetEmptyPositionsHelper> unionGetters;

    protected MapManager map;

    public List<Vector3> Get()
    {
        map = GameManager.Instance.map;
        List<Vector3> positions = BasicGet();
        positions = ApplyModifier(positions);
        return positions;
    }

    protected List<Vector3> ApplyModifier(List<Vector3> positions) {
        foreach (GetHelperModifier modifier in modifiers) {
            positions = modifier.ModifyEmpties(positions);
        }
        return positions;
    }
    protected List<Vector3> BasicGet() {
        switch (howToGetPositions) {
            case HowToGetPositions.ALL:
                return GetAllPositions();
            case HowToGetPositions.WITH_N_CUBES_VOISINS:
                return GetCubesWithNVoisins(nbCubesVoisins);
            case HowToGetPositions.IN_PASSAGES:
                return GetCubesInPassages();
            case HowToGetPositions.OPTIMALY_SPACED:
                return GetOptimalySpaced();
            case HowToGetPositions.CENTER:
                return GetCenterPosition();
            case HowToGetPositions.IN_BOX_AREA:
                return GetInBoxArea();
            case HowToGetPositions.IN_SPHERE_AREA:
                return GetInSphereArea();
            case HowToGetPositions.OF_DATA:
                return map.GetAllLumieresPositions();
            case HowToGetPositions.IN_CAVES:
                return GetInCaves();
            case HowToGetPositions.UNION:
                return GetUnion();
            default:
                return null;
        }
    }

    protected List<Vector3> GetAllPositions() {
        return map.GetAllEmptyPositions();
    }
    protected List<Vector3> GetCubesWithNVoisins(int n) {
        List<Vector3> positions = map.GetAllEmptyPositions().FindAll(p => map.GetVoisinsPleinsAll(p).Count == n).ToList();
        return positions;
    }

    protected List<Vector3> GetCubesInPassages() {
        List<Vector3> positions = map.GetAllEmptyPositions().FindAll(p => map.GetVoisinsPleinsAll(p).Count == 4).ToList();
        positions = positions.FindAll(p => AreOppositeArroundPosition(map.GetVoisinsLibresAll(p))).ToList();
        return positions;
    }

    protected bool AreOppositeArroundPosition(List<Vector3> positions) {
        if(positions.Count != 2) {
            Debug.LogError("Il devrait y avoir exactement 2 positions dans la fonction AreOpposite !");
        }
        Vector3 pos1 = positions[0];
        Vector3 pos2 = positions[1];
        int nbSameAxes = pos1.x == pos2.x ? 1 : 0;
        nbSameAxes += pos1.y == pos2.y ? 1 : 0;
        nbSameAxes += pos1.z == pos2.z ? 1 : 0;
        return nbSameAxes == 2 && MathTools.CubeDistance(pos1, pos2) == 2;
    }

    protected List<Vector3> GetOptimalySpaced() {
        return GetOptimalySpacedPositions.GetSpacedPositions(map, optimalySpacedNbPositions, null, optimalySpacedNbTriesByPosition, optimalySpacedMode);
    }

    protected List<Vector3> GetCenterPosition() {
        return new List<Vector3>() { transform.position };
    }

    protected List<Vector3> GetInSphereArea() {
        return map.GetEmptyPositionsInSphere(areaSphereCenter, areaSphereRadius);
    }

    protected List<Vector3> GetInBoxArea() {
        return map.GetEmptyPositionsInBox(areaBoxCenter, areaBoxHalfExtents);
    }

    protected List<Vector3> GetUnion() {
        List<Vector3> res = new List<Vector3>();
        foreach(GetEmptyPositionsHelper getter in unionGetters) {
            res.AddRange(getter.Get());
        }
        return res;
    }

    protected List<Vector3> GetInCaves() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        List<Vector3> posInCaves = new List<Vector3>();
        foreach(Cave cave in caves) {
            posInCaves.AddRange(cave.GetAllFreeLocations(caveOffsetFromSides));
        }
        posInCaves = posInCaves.Distinct().ToList();
        return posInCaves;
    }
}
