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
    public GameObject ennemiLightningLinkPrefab;

    [Header("Waiting Time")]
    public bool useCustomWaitingTime = true;
    [ConditionalHide("useCustomWaitingTime")]
    public float customWaitingTime = 1.0f;

    protected List<Ennemi> ennemisCreated = new List<Ennemi>();

    protected override void GenerateOneSpecific(Vector3 position) {
        if (CanAddNewEnnemi()) {
            Cube cube = map.GetCubeAt(position);
            if (cube != null) {
                cube.Explode();
            }
            float waitingTime = useCustomWaitingTime ? customWaitingTime : -1;
            Ennemi ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, position, waitingTime);
            SetupEnnemi(ennemi);
        }
    }

    protected void SetupEnnemi(Ennemi ennemi) {
        ennemisCreated.Add(ennemi);
        StartCoroutine(CLinkEnnemiToGenerator(ennemi));
    }

    protected bool CanAddNewEnnemi() {
        ennemisCreated = ennemisCreated.FindAll(e => e != null);
        return ennemisCreated.Count < nbEnnemisMax;
    }

    protected IEnumerator CLinkEnnemiToGenerator(Ennemi ennemi) {
        Lightning link = Instantiate(ennemiLightningLinkPrefab).GetComponent<Lightning>();
        link.Initialize(transform.position, ennemi.transform.position);
        while (ennemi != null) {
            link.SetPosition(transform.position, ennemi.transform.position);
            yield return null;
        }
        Destroy(link.gameObject);
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
