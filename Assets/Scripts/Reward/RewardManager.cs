using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    static RewardManager _instance;
    public static RewardManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<RewardManager>()); } }

    public GameObject playerTrailPrefab;
    public GameObject ennemiTrailPrefab;
    public GameObject lumiereObjectPrefab;
    public GameObject consolePrefab; // On récupère la console !
    public float delayBetweenTrails = 10.0f;
    public float durationTrailMinimum = 10.0f;
    public float durationTrailLogarithmer = 10.0f;
    public float pourcentageEnnemiTrailTime = 0.2f;

    protected Transform displayersFolder;
    protected RewardTrailThemeDisplayer playerDisplayer;
    protected List<RewardTrailDisplayer> ennemisDisplayers;
    protected List<RewardLumiereDisplayer> lumieresDisplayers;

    protected HistoryManager hm;
	protected RewardConsole console;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public void Start() {
        displayersFolder = new GameObject("Displayers").transform;

        hm = HistoryManager.Instance;
        ObjectHistory playerHistory = hm.GetPlayerHistory();
        List<ObjectHistory> ennemisHistory = hm.GetEnnemisHistory();
        List<ObjectHistory> lumieresHistory = hm.GetLumieresHistory();

        float dureeGame = hm.GetDureeGame();
        float dureeReward = ComputeDurationTrail(dureeGame);
        float accelerationCoefficiant = dureeReward / dureeGame;
        Debug.Log("DureeGame = " + dureeGame + " DureeReward = " + dureeReward + " Acceleration = " + accelerationCoefficiant);

        //List<ObjectHistory> allHistorys = new List<ObjectHistory>();
        //allHistorys.Add(playerHistory);
        //allHistorys.AddRange(ennemisHistory);
        //allHistorys.AddRange(lumieresHistory);
        //foreach(ObjectHistory oh in allHistorys) {
        //    Debug.Log("start = " + oh.positions[0].time + " end = " + oh.LastTime());
        //}

        float playerTrailDurationTime = dureeReward;
        float ennemiTrailDurationTime = playerTrailDurationTime * pourcentageEnnemiTrailTime;

        playerDisplayer = gameObject.AddComponent<RewardTrailThemeDisplayer>();
        playerDisplayer.Initialize(playerTrailPrefab, playerHistory, dureeReward, delayBetweenTrails, accelerationCoefficiant, hm.themes);

        ennemisDisplayers = new List<RewardTrailDisplayer>();
        foreach(ObjectHistory history in ennemisHistory) {
            RewardTrailDisplayer displayer = gameObject.AddComponent<RewardTrailDisplayer>();
            displayer.Initialize(ennemiTrailPrefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, ennemiTrailDurationTime, Color.red);
            ennemisDisplayers.Add(displayer);
        }

        lumieresDisplayers = new List<RewardLumiereDisplayer>();
        foreach(ObjectHistory history in lumieresHistory) {
            RewardLumiereDisplayer displayer = gameObject.AddComponent<RewardLumiereDisplayer>();
            displayer.Initialize(lumiereObjectPrefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant);
            lumieresDisplayers.Add(displayer);
        }

        // On lance la console ! :)
        console = Instantiate(consolePrefab).GetComponent<RewardConsole>();
        console.timedMessages = new List<TimedMessage>();
        foreach (TimedMessage tm in hm.GetTimedMessages()) {
            tm.timing *= accelerationCoefficiant;
            console.timedMessages.Add(tm);
        }
        console.Initialize();
        console.SetDureeReward(dureeReward, delayBetweenTrails);
    }

    public void Update() {
        TestExit();
    }

    public void TestExit() {
        // Si on a appuyé sur la touche Escape, on revient au menu !
        if (Input.GetKey (KeyCode.Escape) 
         || Input.GetKey(KeyCode.KeypadEnter)
         || Input.GetKey(KeyCode.Return)) {
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

    public Transform GetDisplayersFolder() {
        return displayersFolder;
    }
}
