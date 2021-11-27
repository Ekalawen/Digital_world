using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataProliferationManagerIcone : MonoBehaviour {

    public List<GameObject> lumieres;
    //public GameObject lightningPrefab; // Voir ActivateLumiere
    public float totalPeriod;
    public List<float> generationTimes;

    public void Start() {
        StartCoroutine(CStartPeriod());
    }

    protected IEnumerator CStartPeriod() {
        Timer totalTimer = new Timer(totalPeriod);
        ResetLumieres();
        while(true) {
            ActivateNeededLumieres(totalTimer);
            if(totalTimer.IsOver()) {
                ResetLumieres();
                totalTimer.Reset();
            }
            yield return null;
        }
    }

    protected void ActivateNeededLumieres(Timer timer) {
        for(int i = 0; i < lumieres.Count; i++) {
            GameObject lumiereGo = lumieres[i];
            if(!lumiereGo.activeInHierarchy) {
                if(timer.GetElapsedTime() >= generationTimes[i]) {
                    ActivateLumiere(lumiereGo);
                }
            }
        }
    }

    protected void ActivateLumiere(GameObject lumiereGo) {
        lumiereGo.SetActive(true);
        // Parce que l'icone tourne c'est trop la merde de lancer le lightning, donc on fait pas ^^'
    }

    protected void ResetLumieres() {
        foreach(GameObject lumiereGo in lumieres) {
            lumiereGo.SetActive(false);
        }
    }
}
