﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllumePlusProcheLumiere : MonoBehaviour {

    protected GameManager gm;
    protected MapManager map;

    protected void Start() {
        gm = GameManager.Instance;
        map = gm.map;
    }

    public void AllumerPlusProcheLumiere(Vector3 position) {
        List<Lumiere> lumieres = map.GetLumieres();
        foreach (Lumiere lumiere in lumieres)
        {
            LumiereSwitchable ls = (LumiereSwitchable)lumiere;
            ls.SetState(LumiereSwitchable.LumiereSwitchableState.OFF);
        }
        if (lumieres.Count <= 1)
            return;
        Lumiere closest = map.GetLumieres().OrderBy(l => Vector3.Distance(l.transform.position, position)).Skip(1).First();
        LumiereSwitchable cloestSwitchable = (LumiereSwitchable)closest;
        cloestSwitchable.SetState(LumiereSwitchable.LumiereSwitchableState.ON);
        //List<float> distances = new List<float>();
        //foreach (Lumiere lumiere in map.GetLumieres())
        //{
        //    LumiereSwitchable ls = (LumiereSwitchable)lumiere;
        //    ls.SetState(LumiereSwitchable.LumiereSwitchableState.OFF);
        //    if (lumiere.transform.position != position)
        //        distances.Add(Vector3.Distance(position, lumiere.transform.position));
        //}
        //if (distances.Count == 0)
        //    return;
        //float minDistance = distances.Min();
        //foreach (Lumiere lumiere in map.GetLumieres())
        //{
        //    if (Vector3.Distance(position, lumiere.transform.position) == minDistance)
        //    {
        //        LumiereSwitchable ls = (LumiereSwitchable)lumiere;
        //        ls.SetState(LumiereSwitchable.LumiereSwitchableState.ON);
        //        break;
        //    }
        //}
    }
}
