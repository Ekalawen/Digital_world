using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LumiereGenerator : IGenerator {

    [Header("LumiereGenerator")]
    public Lumiere.LumiereType lumiereType = Lumiere.LumiereType.NORMAL;

    protected List<Lumiere> lumieresCreated = new List<Lumiere>();

    protected override void GenerateOneSpecific(Vector3 position) {
        Lumiere lumiere = map.CreateLumiere(position, lumiereType);
        lumieresCreated.Add(lumiere);
    }

    protected override bool IsValidPosition(Vector3 position) {
        return !map.IsCubeAt(position) && !map.IsLumiereAt(position);
    }
}
