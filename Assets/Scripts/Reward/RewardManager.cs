﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    static RewardManager _instance;
    public static RewardManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<RewardManager>()); } }

    [Header("Parameters")]
    public float delayBetweenTrails = 10.0f;
    public float durationTrailMinimum = 10.0f;
    public float durationTrailLogarithmer = 10.0f;
    public float pourcentageEnnemiTrailTime = 0.2f;
    public float pointDisplayerScaleFactor = 0.2f;

    [Header("Prefabs")]
    public GameObject playerTrailPrefab;
    public GameObject ennemiTrailPrefab;
    public GameObject lumiereObjectPrefab;
    public GameObject consolePrefab; // On récupère la console !

    [Header("Links")]
    public RegularRewardCamera regularCamera;
    public InfiniteRewardCamera infiniteCamera;


    protected Transform displayersFolder;
    protected RewardTrailThemeDisplayer playerDisplayer;
    protected List<RewardTrailDisplayer> ennemisDisplayers;
    protected List<RewardPointDisplayer> lumieresDisplayers;
    protected List<RewardPointDisplayer> itemsDisplayers;

    protected HistoryManager hm;
	protected RewardConsole console;
    protected RewardCamera camera;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public void Start() {
        displayersFolder = new GameObject("Displayers").transform;

        hm = HistoryManager.Instance;
        ObjectHistory playerHistory = hm.GetPlayerHistory();
        List<ObjectHistory> ennemisHistory = hm.GetEnnemisHistory();
        List<ObjectHistory> lumieresHistory = hm.GetLumieresHistory();
        List<ObjectHistory> itemsHistory = hm.GetItemsHistory();

        float dureeGame = hm.GetDureeGame();
        float dureeReward = ComputeDurationTrail(dureeGame);
        float accelerationCoefficiant = dureeReward / dureeGame;
        Debug.Log("DureeGame = " + dureeGame + " DureeReward = " + dureeReward + " Acceleration = " + accelerationCoefficiant);

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

        lumieresDisplayers = new List<RewardPointDisplayer>();
        foreach(ObjectHistory history in lumieresHistory) {
            RewardPointDisplayer displayer = gameObject.AddComponent<RewardPointDisplayer>();
            displayer.Initialize(history.prefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, pointDisplayerScaleFactor);
            lumieresDisplayers.Add(displayer);
        }

        itemsDisplayers = new List<RewardPointDisplayer>();
        foreach(ObjectHistory history in itemsHistory) {
            RewardPointDisplayer displayer = gameObject.AddComponent<RewardPointDisplayer>();
            displayer.Initialize(history.prefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, pointDisplayerScaleFactor);
            itemsDisplayers.Add(displayer);
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

        InitializeCamera();
    }

    protected void InitializeCamera() {
        if(hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            camera = regularCamera;
            regularCamera.enabled = true;
            infiniteCamera.enabled = false;
        } else {
            camera = infiniteCamera;
            regularCamera.enabled = false;
            infiniteCamera.enabled = true;
        }
        camera.Initialize();
    }

    public void Update() {
        TestExit();
    }

    public void TestExit() {
        if (Input.GetKey (KeyCode.Escape) 
         || Input.GetKey(KeyCode.KeypadEnter)
         || Input.GetKey(KeyCode.Return)
         || Input.GetKey(KeyCode.Space)) {
            Exit();
		}
    }

    public void Exit() {
        SceneManager.LoadScene("SelectorScene");
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

    public RewardTrailThemeDisplayer GetPlayerDisplayer() {
        return playerDisplayer;
    }
}
