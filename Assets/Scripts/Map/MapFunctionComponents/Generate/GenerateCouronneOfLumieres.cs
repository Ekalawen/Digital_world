using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCouronneOfLumieres : GenerateLumieresMapFunction {

    public int nbCouronnesOfLumieres = 2;
    public int offsetCouronnesOfLumieres = 2;

    public override void Activate() {
        GenerateTheCouronneOfLumieres();
    }

    protected void GenerateTheCouronneOfLumieres() {
        int hauteur = 1;
        for(int i = 0; i < nbCouronnesOfLumieres; i++) {
            hauteur += offsetCouronnesOfLumieres;
            List<Vector3> posCouronne = map.GetCouronne(hauteur, offsetSides: 1);
            foreach (Vector3 pos in posCouronne)
                map.CreateLumiere(pos, lumiereType);
        }
    }
}
