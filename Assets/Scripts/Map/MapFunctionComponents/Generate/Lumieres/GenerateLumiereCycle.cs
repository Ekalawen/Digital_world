using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenerateLumiereCycle : GenerateLumieresMapFunction {

    // /!\ Ceci est une classe en chantier ! La structure est là, mais il manque pleins de trucs ! :) /!\

    public override void Activate() {
        List<Vector3> cycle = GenerateHamiltonienCycle(null); // Il faut trouver quel est l'input ici ! :) Par exemple une liste de caves ! 
        LinkCycleWithLumieres(cycle);
    }

    protected List<Vector3> GenerateHamiltonienCycle(List<Vector3> positions) {
        Graphe<Vector3> graphe = new Graphe<Vector3>();
        foreach(Vector3 pos in positions) {
            //List<Vector3> voisins = GetAccessiblesFromJump(pos, positions);
            List<Vector3> voisins = IsAccessibleFrom(pos, positions);
            graphe[pos] = voisins;
        }

        List<Vector3> cycle = graphe.CycleHamiltonien(positions[0]);
        foreach(Vector3 pos in cycle) {
            Debug.Log("pos = " + pos);
        }
        return cycle;
    }

    protected abstract List<Vector3> IsAccessibleFrom(Vector3 pos, List<Vector3> positions);

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
            if(map.GetCubeAt(MathTools.Round(pos)) == null)
                map.CreateLumiere(pos, Lumiere.LumiereType.NORMAL, dontRoundPositions: true);
        }
    }
}
