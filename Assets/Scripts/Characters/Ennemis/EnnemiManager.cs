using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiManager : MonoBehaviour {

    public List<GameObject> ennemisPrefabs; // On récupère les ennemis !
    public List<int> nbEnnemis;

    [HideInInspector]
    public List<Ennemi> ennemis; // Elle connait toutes les sondes

    protected GameManager gm;
    [HideInInspector]
    public GameObject ennemisFolder;

    public void Initialize() {
        gm = GameManager.Instance;
        ennemisFolder = new GameObject("Ennemis");

        // On récupère tous les ennemis qui pourraient déjà exister dans la map !
        GetAllAlreadyExistingEnnemis();

        GenerateEnnemies();
    }

    void GenerateEnnemies() {
        ennemis = new List<Ennemi>();
        for(int i = 0; i < ennemisPrefabs.Count; i++) {
            GameObject ennemiPrefab = ennemisPrefabs[i];
            int nbEnnemi = nbEnnemis[i];
            for (int j = 0; j < nbEnnemi; j++) {
                PopEnnemi(ennemiPrefab);
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

    protected void GetAllAlreadyExistingEnnemis() {
        Ennemi[] newEnnemis = FindObjectsOfType<Ennemi>();
        foreach (Ennemi ennemi in newEnnemis) {
            if(!ennemi.transform.IsChildOf(ennemisFolder.transform)) {
                Transform maxParent = ennemi.transform.parent;
                if (maxParent == null)
                    maxParent = ennemi.transform;
                while (maxParent.parent != null)
                    maxParent = maxParent.parent;
                maxParent.SetParent(ennemisFolder.transform);
            }
            ennemi.Start(); // On reset l'ennemi histoire d'être sur qu'il va bien :)
            ennemis.Add(ennemi);
        }
    }

    public Ennemi GenerateEnnemiFromPrefab(GameObject ennemiPrefab, Vector3 pos) {
        Ennemi ennemi = Instantiate(ennemiPrefab, pos, Quaternion.identity, ennemisFolder.transform).GetComponent<Ennemi>();
        ennemis.Add(ennemi);

        gm.historyManager.AddEnnemiHistory(ennemi);

        //// For bosses ! :D
        //Ennemi[] childEnnemis = ennemi.gameObject.GetComponentsInChildren<Ennemi>();
        //foreach (Ennemi child in childEnnemis)
        //    gm.historyManager.AddEnnemiHistory(child);

        return ennemi;
    }

    public Ennemi RegisterAlreadyExistingEnnemi(Ennemi ennemi) {
        ennemis.Add(ennemi);
        gm.historyManager.AddEnnemiHistory(ennemi);
        return ennemi;
    }

    protected virtual Ennemi PopEnnemi(GameObject ennemiPrefab) {
        Vector3 pos = gm.map.GetFreeRoundedLocation();
        return GenerateEnnemiFromPrefab(ennemiPrefab, pos);
    }

    public void RemoveEnnemi(Ennemi ennemi) {
        ennemis.Remove(ennemi);
    }
}
