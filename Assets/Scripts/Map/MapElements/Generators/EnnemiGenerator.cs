using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnnemiGenerator : IGenerator {

    [Header("EnnemiGenerator")]
    public GameObject ennemiPrefab;
    public int nbEnnemisMax = 10;
    public bool destroyEnnemisOnDestruction = true;

    protected List<Ennemi> ennemisCreated = new List<Ennemi>();

    protected override void GenerateOneSpecific(Vector3 position) {
        if (CanAddNewEnnemi()) {
            Cube cube = map.GetCubeAt(position);
            if (cube != null)
            {
                cube.Explode();
            }
            Ennemi ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, position);
            ennemisCreated.Add(ennemi);
        }
    }

    protected bool CanAddNewEnnemi() {
        ennemisCreated = ennemisCreated.FindAll(e => e != null);
        return ennemisCreated.Count < nbEnnemisMax;
    }

    protected override bool IsValidPosition(Vector3 position) {
        return true;
    }

    public override void DestroyIn(float duree) {
        base.DestroyIn(duree);
        if (destroyEnnemisOnDestruction) {
            foreach (Ennemi ennemi in ennemisCreated) {
                if (ennemi != null) {
                    ennemi.DestroyEnnemi();
                }
            }
        }
    }
}
