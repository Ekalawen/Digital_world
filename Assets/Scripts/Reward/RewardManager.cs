using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    public GameObject trailPrefab;

    protected HistoryManager hm;
    protected List<TimedVector3> playerPositions;
    protected Curve playerCurve;
    protected Trail trail;
    protected Timer rewardTimer;
    protected float trailDurationTime;
    protected float maxDistPoints, minDistPoints;

    public void Start() {
        hm = HistoryManager.Instance;
        playerPositions = hm.GetPositions();

        playerCurve = new LinearCurve();


        maxDistPoints = 0;
        minDistPoints = float.PositiveInfinity;
        for(int i = 0; i < playerPositions.Count; i++) {
            TimedVector3 tpos = playerPositions[i];
            playerCurve.AddPoint(tpos.position);
            Debug.Log("pos = " + tpos.position);
            Debug.Log("time = " + tpos.time);
            if(i < playerPositions.Count - 1) {
                float dist = Vector3.Distance(tpos.position, playerPositions[i + 1].position);
                minDistPoints = Mathf.Min(minDistPoints, dist);
                maxDistPoints = Mathf.Max(maxDistPoints, dist);
            }
        }


        trailDurationTime = playerPositions[playerPositions.Count - 1].time;
        rewardTimer = new Timer(trailDurationTime);

        trail = CreateTrail();
    }

    public void Update() {
        if (rewardTimer.IsOver()) {
            rewardTimer.Reset();
            trail = CreateTrail();
        }

        float avancement = rewardTimer.GetAvancement();
        trail.transform.position = playerCurve.GetAvancement(avancement);

        SetTrailColor(avancement);

        TestExit();
    }

    protected Trail CreateTrail() {
        Trail trail = Instantiate(trailPrefab, playerCurve[0], Quaternion.identity).GetComponent<Trail>();
        trail.GetComponent<TrailRenderer>().time = trailDurationTime;
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

    protected void SetTrailColor(float avancement) {
        List<Vector3> twoPoints = playerCurve.GetSurroundingPoints(avancement);
        Vector3 p1 = twoPoints[0];
        Vector3 p2 = twoPoints[1];
        float distance = Vector3.Distance(p1, p2);
        float H = (distance - minDistPoints) / (maxDistPoints - minDistPoints);
        Debug.Log("H = " + H);
        float S = 1.0f;
        float V = 0.5f;
        Color startColor = Color.HSVToRGB(H, S, V);
        trail.GetComponent<TrailRenderer>().startColor = startColor;
        Gradient gradient = new Gradient();
        GradientAlphaKey alphaKey;
        GradientColorKey colorKey;
        //trail.GetComponent<TrailRenderer>().colo
    }
}
