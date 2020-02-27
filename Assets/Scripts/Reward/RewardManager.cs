﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    public GameObject trailPrefab;
    public RewardCamera rewardCamera;
    public float delayBetweenTrails = 10.0f;
    public float durationTrail = 10.0f;

    protected HistoryManager hm;
    protected List<TimedVector3> playerPositions;
    protected Curve playerCurve;
    protected TrailRenderer trail;
    protected Timer trailTimer, delayTrailTimer;
    protected float trailDurationTime;
    protected float maxDistPoints, minDistPoints;
    protected float accelerationCoefficiant;

    public void Start() {
        hm = HistoryManager.Instance;
        playerPositions = hm.GetPositions();
        accelerationCoefficiant = durationTrail / playerPositions[playerPositions.Count - 1].time;

        playerCurve = new LinearCurve();


        maxDistPoints = 0;
        minDistPoints = float.PositiveInfinity;
        for(int i = 0; i < playerPositions.Count; i++) {
            TimedVector3 tpos = playerPositions[i];
            tpos.time *= accelerationCoefficiant;
            playerPositions[i] = tpos;
            playerCurve.AddPoint(tpos.position);
            if(i < playerPositions.Count - 1) {
                float dist = Vector3.Distance(tpos.position, playerPositions[i + 1].position);
                minDistPoints = Mathf.Min(minDistPoints, dist);
                maxDistPoints = Mathf.Max(maxDistPoints, dist);
            }
        }

        trailDurationTime = playerPositions[playerPositions.Count - 1].time;
        trailTimer = new Timer(trailDurationTime);
        delayTrailTimer = new Timer(delayBetweenTrails);

        StartCoroutine(UpdateTrails());
    }

    protected IEnumerator UpdateTrails() {
        while(true) {
            trail = CreateTrail();
            trailTimer.Reset();
            yield return new WaitForSeconds(trailTimer.GetDuree());

            delayTrailTimer.Reset();
            yield return new WaitForSeconds(delayTrailTimer.GetDuree());
        }
    }

    public void Update() {
        float avancement = trailTimer.GetAvancement();
        trail.transform.position = playerCurve.GetAvancement(avancement);
        TestExit();
    }

    protected TrailRenderer CreateTrail() {
        TrailRenderer trail = Instantiate(trailPrefab, playerCurve[0], Quaternion.identity).GetComponent<TrailRenderer>();
        trail.time = trailDurationTime;
        trail.startColor = ColorManager.GetColor(hm.themes);
        return trail;
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
}
