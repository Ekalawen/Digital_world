using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightningBox : MonoBehaviour {

    public GameObject lighningPrefab;

    protected Vector3 center;
    protected Vector3 halfExtents;
    protected float dureeAvantConnection;
    protected float dureeApresConnection;
    protected List<Lightning> lightnings;

    public void Initialize(Vector3 center, Vector3 halfExtents, float dureeAvantConnection, float dureeApresConnection) {
        this.center = center;
        this.halfExtents = halfExtents;
        this.dureeAvantConnection = dureeAvantConnection;
        this.dureeApresConnection = dureeApresConnection;
        GenerateLightnings();
        StartCoroutine(CDestroyAfterUse());
    }

    protected void GenerateLightnings() {
        List<Tuple<Vector3, Vector3>> edges = MathTools.GetBoxEdges();
        edges = edges.Select(e => new Tuple<Vector3, Vector3>(center + MathTools.VecMul(e.Item1, halfExtents), center + MathTools.VecMul(e.Item2, halfExtents))).ToList();

        lightnings = new List<Lightning>();
        foreach(Tuple<Vector3, Vector3> edge in edges) {
            Vector3 start = edge.Item1;
            Vector3 end = edge.Item2;
            Lightning lightning = Instantiate(lighningPrefab, start, Quaternion.identity, parent: transform).GetComponent<Lightning>();
            lightning.SetDurees(dureeAvantConnection, (end - start).magnitude, dureeApresConnection);
            lightning.Initialize(start, end, Lightning.PivotType.CENTER);
            lightnings.Add(lightning);
        }
    }

    protected IEnumerator CDestroyAfterUse() {
        yield return new WaitForSeconds(dureeAvantConnection + dureeApresConnection);
        Destroy(gameObject);
    }
}
