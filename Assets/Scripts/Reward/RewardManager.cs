using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    public GameObject playerTrailPrefab;
    public GameObject ennemiTrailPrefab;
    public GameObject lumiereObjectPrefab;
    public RewardCamera rewardCamera;
    public float delayBetweenTrails = 10.0f;
    public float durationTrailMinimum = 10.0f;
    public float durationTrailLogarithmer = 10.0f;
    public float pourcentageEnnemiTrailTime = 0.2f;

    protected RewardTrailThemeDisplayer playerDisplayer;
    protected List<RewardTrailDisplayer> ennemisDisplayers;
    protected List<RewardLumiereDisplayer> lumieresDisplayers;

    protected HistoryManager hm;

    public void Start() {
        hm = HistoryManager.Instance;
        ObjectHistory playerHistory = hm.GetPlayerHistory();
        List<ObjectHistory> ennemisHistory = hm.GetEnnemisHistory();
        List<ObjectHistory> lumieresHistory = hm.GetLumieresHistory();

        float dureeGame = hm.GetDureeGame();
        float durationTrail = ComputeDurationTrail(dureeGame);
        float accelerationCoefficiant = durationTrail / dureeGame;

        float playerTrailDurationTime = durationTrail;
        float ennemiTrailDurationTime = playerTrailDurationTime * pourcentageEnnemiTrailTime;

        playerDisplayer = gameObject.AddComponent<RewardTrailThemeDisplayer>();
        playerDisplayer.Initialize(playerTrailPrefab, playerHistory, dureeGame, delayBetweenTrails, accelerationCoefficiant, hm.themes);

        ennemisDisplayers = new List<RewardTrailDisplayer>();
        foreach(ObjectHistory history in ennemisHistory) {
            RewardTrailDisplayer displayer = gameObject.AddComponent<RewardTrailDisplayer>();
            displayer.Initialize(ennemiTrailPrefab, history, dureeGame, delayBetweenTrails, accelerationCoefficiant, ennemiTrailDurationTime, Color.red);
            ennemisDisplayers.Add(displayer);
        }

        lumieresDisplayers = new List<RewardLumiereDisplayer>();
        foreach(ObjectHistory history in lumieresHistory) {
            RewardLumiereDisplayer displayer = gameObject.AddComponent<RewardLumiereDisplayer>();
            displayer.Initialize(lumiereObjectPrefab, history, dureeGame, delayBetweenTrails, accelerationCoefficiant);
            lumieresDisplayers.Add(displayer);
        }
    }

    public void Update() {
        TestExit();
    }

    public void TestExit() {
        // Si on a appuyé sur la touche Escape, on revient au menu !
        if (Input.GetKey (KeyCode.Escape) 
         || Input.GetKey(KeyCode.KeypadEnter)
         || Input.GetKey(KeyCode.Return)
         || Input.GetMouseButton(0)) {
            //Destroy(HistoryManager.Instance.gameObject);
            SceneManager.LoadScene("MenuScene");
		}
    }

    protected float ComputeDurationTrail(float dureeGame) {
        if (dureeGame <= durationTrailMinimum)
            return dureeGame;
        else {
            return Mathf.Max(durationTrailMinimum, Mathf.Log(dureeGame, durationTrailLogarithmer));
        }
    }
}
