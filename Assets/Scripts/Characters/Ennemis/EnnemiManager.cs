using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiManager : MonoBehaviour {

    [Header("Ennemis")]
    public List<GameObject> ennemisPrefabs; // On récupère les ennemis !
    public List<int> nbEnnemis;

    [Header("Player Capture")]
    public float playerCaptureTime = 5.0f;
    public float playerCaptureDistance = 0.3f;

    [HideInInspector]
    public List<Ennemi> ennemis; // Elle connait toutes les sondes

    protected GameManager gm;
    [HideInInspector]
    public GameObject ennemisFolder;
	protected Timer playerCaptureTimer; // Le temps depuis lequel le joueur est en contact avec un ennemi


    public void Initialize() {
        gm = GameManager.Instance;
        ennemisFolder = new GameObject("Ennemis");
        playerCaptureTimer = new Timer(playerCaptureTime);

        // On récupère tous les ennemis qui pourraient déjà exister dans la map !
        GetAllAlreadyExistingEnnemis();

        GenerateEnnemies();
    }

    public void Update() {
        TestPlayerCaptured();
    }

    void GenerateEnnemies() {
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

    public List<Vector3> GetAllRoundedPositionsOccupiedByEnnemis() {
        List<Vector3> res = new List<Vector3>();
        foreach(Ennemi ennemi in ennemis) {
            res.AddRange(ennemi.GetAllOccupiedRoundedPositions());
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
        ennemis = new List<Ennemi>();
        foreach (Ennemi ennemi in newEnnemis) {
            if(!ennemi.transform.IsChildOf(ennemisFolder.transform)) {
                Transform maxParent = ennemi.transform.parent;
                if (maxParent == null)
                    maxParent = ennemi.transform;
                while (maxParent.parent != null)
                    maxParent = maxParent.parent;
                maxParent.SetParent(ennemisFolder.transform);
            }
            Ennemi newEnnemi = Instantiate(ennemi.gameObject, ennemi.transform.position, ennemi.transform.rotation, ennemi.transform.parent).GetComponent<Ennemi>();
            Destroy(ennemi.gameObject);
            RegisterAlreadyExistingEnnemi(newEnnemi);
        }
    }

    public Ennemi GenerateEnnemiFromPrefab(GameObject ennemiPrefab, Vector3 pos) {
        Ennemi ennemi = Instantiate(ennemiPrefab, pos, Quaternion.identity, ennemisFolder.transform).GetComponent<Ennemi>();

        RegisterAlreadyExistingEnnemi(ennemi);

        return ennemi;
    }

    public Ennemi RegisterAlreadyExistingEnnemi(Ennemi ennemi) {
        ennemis.Add(ennemi);
        gm.historyManager.AddEnnemiHistory(ennemi);
        return ennemi;
    }

    public virtual Ennemi PopEnnemi(GameObject ennemiPrefab) {
        Vector3 pos = ennemiPrefab.GetComponent<Ennemi>().PopPosition(gm.map);
        return GenerateEnnemiFromPrefab(ennemiPrefab, pos);
    }

    public void RemoveEnnemi(Ennemi ennemi) {
        ennemis.Remove(ennemi);
    }

    protected void TestPlayerCaptured() {
        if (!IsPlayerInCaptureRange()) {
            playerCaptureTimer.Reset();
        }

        if (playerCaptureTimer.IsOver()) {
            gm.eventManager.LoseGame(EventManager.DeathReason.CAPTURED);
        }
    }

    public bool IsPlayerInCaptureRange() {
        return IsInCaptureRange(gm.player.transform.position);
    }

    public bool IsInCaptureRange(Vector3 position) {
        foreach (Ennemi ennemi in ennemis) {
            if (ennemi.CanCapture()) {
                float captureDistance = gm.player.GetSizeRadius() + ennemi.transform.localScale[0] + playerCaptureDistance;
                if (Vector3.Distance(ennemi.transform.position, position) <= captureDistance) {
                    return true;
                }
            }
        }
        return false;
    }

    public void SwapDisableEnnemis() {
        if (ennemis.Count > 0) {
            bool newState = !ennemis[0].gameObject.activeInHierarchy;
            foreach (Ennemi ennemi in ennemis) {
                ennemi.gameObject.SetActive(newState);
            }
        }
    }
}
