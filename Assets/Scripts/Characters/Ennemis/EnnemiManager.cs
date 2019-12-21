using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiManager : MonoBehaviour {

    public List<GameObject> ennemisPrefabs; // On récupère les ennemis !
    public List<int> nbEnnemis;

    [HideInInspector]
    public List<Ennemi> ennemis; // Elle connait toutes les sondes

    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;

        GenerateEnnemies();
    }

    void GenerateEnnemies() {
        ennemis = new List<Ennemi>();
        for(int i = 0; i < ennemisPrefabs.Count; i++) {
            GameObject ennemiPrefab = ennemisPrefabs[i];
            int nbEnnemi = nbEnnemis[i];
            for (int j = 0; j < nbEnnemi; j++) {
                Vector3 pos = gm.map.GetFreeRoundedLocation();
                //Vector3 pos = gm.map.GetFreeSphereLocation(1.0f);
                Ennemi ennemi = Instantiate(ennemiPrefab, pos, Quaternion.identity).GetComponent<Ennemi>();
                ennemis.Add(ennemi);
            }
        }
    }

    public List<Vector3> GetAllRoundedEnnemisPositions() {
        List<Vector3> res = new List<Vector3>();
        foreach(Ennemi ennemi in ennemis) {
            res.Add(MathTools.Round(ennemi.transform.position));
        }
        return res;
    }

    public bool IsPlayerFollowed() {
        foreach(Ennemi ennemi in ennemis) {
            if (!ennemi.IsInactive())
                return true;
        }
        return false;
    }
}
