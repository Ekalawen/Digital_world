using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLumieresWithHelper : GenerateLumieresMapFunction {

    public int nbDatas = 0;
    public GetEmptyPositionsHelper emptyPositionsHelper;
    public bool dontRoundPositions = false;

    protected List<Lumiere> lumieres = new List<Lumiere>();

    public override void Activate() {
        List<Vector3> positions = emptyPositionsHelper.Get();
        if(positions.Count < nbDatas) {
            Debug.LogError($"Moins de positions dans le GetEmptyPositionsHelper ({positions.Count}) que de Datas à générer ({nbDatas})");
        }
        for(int i = 0; i < Mathf.Min(positions.Count, nbDatas); i++) {
            Vector3 pos = MathTools.ChoseOne(positions);
            lumieres.Add(map.CreateLumiere(pos, lumiereType, dontRoundPositions: dontRoundPositions)); ;
            positions.Remove(pos);
        }
    }
}
