using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddLumieresInCaves : GenerateLumieresMapFunction {

    public int nbLumieres = 1;

    public override void Activate() {
        CreatesLumieresInCaves();
    }

    protected void CreatesLumieresInCaves() {
        for(int i = 0; i < nbLumieres; i++) {
            CreateRandomLumiereInCave();
        }
    }

    protected void CreateRandomLumiereInCave() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        List<Cave> cavesGrandes = new List<Cave>();
        foreach(Cave cave in caves) {
            if (cave.nbCubesParAxe.x >= 3 && cave.nbCubesParAxe.y >= 3 && cave.nbCubesParAxe.z >= 3)
                cavesGrandes.Add(cave);
        }
        int nbGrandesCaves = cavesGrandes.Count;
        for(int k = 0; k < nbGrandesCaves; k++) {
            Cave chosenCave = MathTools.ChoseOne(cavesGrandes);
            if(chosenCave.AddNLumiereInside(1)) {
                break;
            }
            cavesGrandes.Remove(chosenCave);
        }
    }

    protected void CreateRandomLumiere() {
        Vector3 posLumiere = map.GetFreeRoundedLocationWithoutLumiere();
        map.CreateLumiere(posLumiere, lumiereType);
    }

}
