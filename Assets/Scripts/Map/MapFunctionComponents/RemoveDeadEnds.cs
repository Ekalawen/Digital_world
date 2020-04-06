using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveDeadEnds : MapFunctionComponent {
    public override void Activate() {
        RemoveAllDeadEnds();
    }

    void RemoveAllDeadEnds() {
        List<Vector3> deadEnds = GetAllDeadEnds();
        int k = 0;
        while(deadEnds.Count != 0 && k <= 100) {
            foreach (Vector3 deadEnd in deadEnds)
                RemoveDeadEnd(deadEnd);
            deadEnds = GetAllDeadEnds();
            k++;
        }
        if(k >= 100)
            Debug.Log("Problème dans les dead ends !!! k = " + k);
    }

    protected List<Vector3> GetAllDeadEnds() {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 pos in map.GetAllEmptyPositions()) {
            if (map.IsInInsidedRegularMap(pos) && map.GetVoisinsLibres(pos).Count == 1) {
                res.Add(pos);
            }
        }
        return res;
    }

    protected void RemoveDeadEnd(Vector3 deadEnd) {
        List<Vector3> voisins = map.GetVoisinsPleins(deadEnd);
        List<Vector3> voisinsInInsidedMap = new List<Vector3>();
        foreach (Vector3 voisin in voisins) {
            if(map.IsInInsidedRegularMap(voisin))
                voisinsInInsidedMap.Add(voisin);
        }
        if (voisinsInInsidedMap.Count == 0)
            return;
        int ind = Random.Range(0, voisinsInInsidedMap.Count);
        map.DeleteCubesAt(voisinsInInsidedMap[ind]);
    }
}
