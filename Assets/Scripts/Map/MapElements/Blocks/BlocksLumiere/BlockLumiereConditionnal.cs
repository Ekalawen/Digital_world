using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BlockLumiereConditionnal : BlockLumiere {

    public enum ConditionType
    {
        Presence,
        Absence,
    }

    public ConditionType conditionType = ConditionType.Presence;
    public List<Cube> necessaryCubes;

    public override bool CanBePicked() {
        if (conditionType == ConditionType.Presence) {
            return necessaryCubes.All(c => infiniteMap.IsCubeAt(c.transform.position));
        }
        return necessaryCubes.All(c => !infiniteMap.IsCubeAt(c.transform.position));
    }
}
