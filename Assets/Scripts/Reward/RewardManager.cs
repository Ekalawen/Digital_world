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

    protected HistoryManager hm;

    protected ObjectHistory playerHistory;
    protected List<ObjectHistory> ennemisHistory;
    protected List<ObjectHistory> lumieresHistory;

    protected Curve playerCurve;
    protected List<Curve> ennemisCurves;
    protected List<Curve> lumieresCurves;

    protected float durationTrail;
    protected TrailRenderer playerTrail;
    protected List<TrailRenderer> ennemisTrails;
    protected List<GameObject> lumieresObjects;

    protected float playerTrailDurationTime;
    protected float ennemiTrailDurationTime;

    protected Timer trailTimer, delayTrailTimer;
    protected float accelerationCoefficiant;
    protected float dureeGame;

    public void Start() {
        hm = HistoryManager.Instance;
        playerHistory = hm.GetPlayerHistory();
        ennemisHistory = hm.GetEnnemisHistory();
        lumieresHistory = hm.GetLumieresHistory();

        dureeGame = playerHistory.positions[playerHistory.positions.Count - 1].time;
        durationTrail = ComputeDurationTrail();
        accelerationCoefficiant = durationTrail / dureeGame;

        playerCurve = new LinearCurve();
        ennemisCurves = new List<Curve>();
        ennemisTrails = new List<TrailRenderer>();
        for(int i = 0; i < ennemisHistory.Count; i++) {
            ennemisCurves.Add(new LinearCurve());
        }
        lumieresCurves = new List<Curve>();
        lumieresObjects = new List<GameObject>();
        for(int i = 0; i < lumieresHistory.Count; i++) {
            lumieresCurves.Add(new LinearCurve());
        }

        playerCurve = CreateCurveFromHistory(playerHistory);
        for (int i = 0; i < ennemisHistory.Count; i++)
            ennemisCurves[i] = CreateCurveFromHistory(ennemisHistory[i]);
        for (int i = 0; i < lumieresHistory.Count; i++)
            lumieresCurves[i] = CreateCurveFromHistory(lumieresHistory[i]);

        playerTrailDurationTime = durationTrail;
        ennemiTrailDurationTime = playerTrailDurationTime * pourcentageEnnemiTrailTime;
        trailTimer = new Timer(playerTrailDurationTime);
        delayTrailTimer = new Timer(delayBetweenTrails);

        StartCoroutine(UpdateTrails());
    }

    protected Curve CreateCurveFromHistory(ObjectHistory history) {
        Curve curve = new LinearCurve();
        for(int i = 0; i < history.positions.Count; i++) {
            TimedVector3 tpos = history.positions[i];
            tpos.time *= accelerationCoefficiant;
            history.positions[i] = tpos;
            curve.AddPoint(tpos.position);
        }
        return curve;
    }

    protected IEnumerator UpdateTrails() {
        while(true) {
            playerTrail = CreateTrail(playerTrailPrefab, playerCurve[0], playerTrailDurationTime, ColorManager.GetColor(hm.themes));
            ennemisTrails = new List<TrailRenderer>();
            foreach(Curve curve in ennemisCurves) {
                ennemisTrails.Add(CreateTrail(ennemiTrailPrefab, curve[0], ennemiTrailDurationTime, Color.red));
            }
            lumieresObjects = new List<GameObject>();
            foreach(Curve curve in lumieresCurves) {
                lumieresObjects.Add(Instantiate(lumiereObjectPrefab, curve[0], Quaternion.identity));
            }
            trailTimer.Reset();
            yield return new WaitForSeconds(trailTimer.GetDuree());

            delayTrailTimer.Reset();
            yield return new WaitForSeconds(delayTrailTimer.GetDuree());
        }
    }

    public void Update() {
        float avancement = trailTimer.GetAvancement();
        playerTrail.transform.position = playerCurve.GetAvancement(avancement);
        playerTrail.time = trailTimer.GetRemainingTime();
        for(int i = 0; i < ennemisTrails.Count; i++) {
            ennemisTrails[i].transform.position = ennemisCurves[i].GetAvancement(avancement);
        }
        for(int i = 0; i < lumieresObjects.Count; i++) {
            if (lumieresObjects[i] == null)
                continue;
            float lastTime = lumieresHistory[i].LastTime();
            if(avancement > (lastTime / dureeGame) / accelerationCoefficiant) {
                Destroy(lumieresObjects[i]);
                lumieresObjects[i] = null;
            } else {
                lumieresObjects[i].transform.position = lumieresCurves[i].GetAvancement(avancement);
            }
        }
        TestExit();
    }

    protected TrailRenderer CreateTrail(GameObject prefab, Vector3 position, float duration, Color color) {
        TrailRenderer trail = Instantiate(prefab, position, Quaternion.identity).GetComponent<TrailRenderer>();
        trail.time = duration;
        trail.startColor = color;
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
    protected float ComputeDurationTrail() {
        if (dureeGame <= durationTrailMinimum)
            return dureeGame;
        else {
            return Mathf.Max(durationTrailMinimum, Mathf.Log(dureeGame, durationTrailLogarithmer));
        }
    }
}
