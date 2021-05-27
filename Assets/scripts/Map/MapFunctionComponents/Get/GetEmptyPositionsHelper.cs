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
    };

    public HowToGetPositions howToGetPositions = HowToGetPositions.ALL;
    [ConditionalHide("howToGetPositions", HowToGetPositions.WITH_N_CUBES_VOISINS)]
    public int nbCubesVoisins = 4;

    protected MapManager map;

    public List<Vector3> Get()
    {
        map = GameManager.Instance.map;
        List<Vector3> positions = BasicGet();
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
}
