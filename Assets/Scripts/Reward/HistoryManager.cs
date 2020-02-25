using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TimedVector3 {
    public Vector3 position;
    public float time;

    public TimedVector3(Vector3 position, float time) {
        this.position = position;
        this.time = time;
    }
}

public class HistoryManager : MonoBehaviour {

    static HistoryManager _instance;
    public static HistoryManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<HistoryManager>()); } }

    public float frequenceEchantillonnagePositions = 0.33f;

    protected GameManager gm;
    [HideInInspector]
    public Vector3Int mapSize;
    protected List<TimedVector3> playerPositions;
    protected Timer echantillonnageTimer;

    private void Awake() {
        if (!_instance) { _instance = this; }
        DontDestroyOnLoad(this);
    }

    public void Initialize() {
        gm = GameManager.Instance;
        playerPositions = new List<TimedVector3>();
        echantillonnageTimer = new Timer(frequenceEchantillonnagePositions);
        mapSize = gm.map.tailleMap;
    }

    public void Update() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            TimedVector3 tpos = new TimedVector3(gm.player.transform.position, gm.timerManager.GetElapsedTime());
            playerPositions.Add(tpos);
            echantillonnageTimer.Reset();
        }
    }

    public List<TimedVector3> GetPositions() {
        return playerPositions;
    }
}
