using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixNbLumieresMapFunction : GenerateLumieresMapFunction {

    public bool lumieresFixedInCaves = false;

    public override void Activate() {
        FixNbLumieres();
    }

    protected virtual void FixNbLumieres() {
        List<Lumiere> lumieres = map.GetLumieres();
        if(lumieres.Count > map.nbLumieresInitial) {
            int nbLumieresToDelete = lumieres.Count - map.nbLumieresInitial;
            for(int i = 0; i < nbLumieresToDelete; i++) {
                int indiceLumiere = Random.Range(0, lumieres.Count);
                Destroy(lumieres[indiceLumiere].gameObject);
                lumieres.RemoveAt(indiceLumiere);
            }
        }

        if(lumieres.Count < map.nbLumieresInitial) {
            int nbLumieresToAdd = map.nbLumieresInitial - lumieres.Count;
            for(int i = 0; i < nbLumieresToAdd; i++) {
                if(lumieresFixedInCaves)
                    CreateRandomLumiereInCave();
                else
                    CreateRandomLumiere();
            }
        }
    }

    protected void CreateRandomLumiereInCave() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        List<Cave> cavesGrandes = new List<Cave>();
        foreach(Cave cave in caves) {
            if (cave.nbCubesParAxe.x >= 3 && cave.nbCubesParAxe.y >= 3 && cave.nbCubesParAxe.z >= 3)
                cavesGrandes.Add(cave);
        }
        Cave chosenCave = cavesGrandes[Random.Range(0, cavesGrandes.Count)];
        chosenCave.AddNLumiereInside(1);
    }

    protected void CreateRandomLumiere() {
        Vector3 posLumiere = map.GetFreeRoundedLocation();
        map.CreateLumiere(posLumiere, lumiereType);
    }

}
