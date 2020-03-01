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

public struct ObjectHistory {
    public MonoBehaviour obj;
    public List<TimedVector3> positions;

    public ObjectHistory(MonoBehaviour obj) {
        this.obj = obj;
        positions = new List<TimedVector3>();
        positions.Add(new TimedVector3(obj.transform.position, GameManager.Instance.timerManager.GetElapsedTime()));
    }

    public float LastTime() {
        return positions[positions.Count - 1].time;
    }
}

public class HistoryManager : MonoBehaviour {

    static HistoryManager _instance;
    public static HistoryManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<HistoryManager>()); } }

    public float frequenceEchantillonnagePositions = 0.33f;

    protected GameManager gm;
    [HideInInspector]
    public Vector3Int mapSize;
    [HideInInspector]
    public List<ColorSource.ThemeSource> themes;
    protected ObjectHistory playerHistory;
    protected List<ObjectHistory> ennemisHistory;
    protected List<ObjectHistory> lumieresHistory;
    protected Timer echantillonnageTimer;
    protected float dureeGame;

    private void Awake() {
        if (!_instance) { _instance = this; }
        DontDestroyOnLoad(this);
    }

    public void Initialize() {
        gm = GameManager.Instance;

        playerHistory = new ObjectHistory(gm.player);
        if(ennemisHistory == null)
            ennemisHistory = new List<ObjectHistory>();
        if(lumieresHistory == null)
            lumieresHistory = new List<ObjectHistory>();

        //foreach (Ennemi ennemi in gm.ennemiManager.ennemis)
        //    ennemisHistory.Add(new ObjectHistory(ennemi));
        //foreach(Lumiere lumiere in gm.map.GetLumieres())
        //    lumieresHistory.Add(new ObjectHistory(lumiere));

        echantillonnageTimer = new Timer(frequenceEchantillonnagePositions);
        mapSize = gm.map.tailleMap;
        themes = gm.colorManager.themes;
    }

    public void Update() {
        EchantillonnerPositionPlayer();
        EchantillonnerPositionsEnnemis();
        EchantillonnerPositionsLumieres();

        if(echantillonnageTimer.IsOver())
            echantillonnageTimer.Reset();
    }

    protected void EchantillonnerPositionPlayer() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            TimedVector3 tpos = new TimedVector3(gm.player.transform.position, gm.timerManager.GetElapsedTime());
            playerHistory.positions.Add(tpos);
        }
    }

    protected void EchantillonnerPositionsEnnemis() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            for(int i = 0; i < ennemisHistory.Count; i++) {
                ObjectHistory ch = ennemisHistory[i];
                TimedVector3 tpos = new TimedVector3(ch.obj.transform.position, gm.timerManager.GetElapsedTime());
                ch.positions.Add(tpos);
            }
        }
    }

    protected void EchantillonnerPositionsLumieres() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            for(int i = 0; i < lumieresHistory.Count; i++) {
                ObjectHistory ch = lumieresHistory[i];
                if (ch.obj != null) {
                    TimedVector3 tpos = new TimedVector3(ch.obj.transform.position, gm.timerManager.GetElapsedTime());
                    ch.positions.Add(tpos);
                }
            }
        }
    }

    public ObjectHistory GetPlayerHistory() {
        return playerHistory;
    }

    public List<ObjectHistory> GetEnnemisHistory() {
        return ennemisHistory;
    }

    public List<ObjectHistory> GetLumieresHistory() {
        return lumieresHistory;
    }

    public float GetDureeGame() {
        return dureeGame;
    }
    public void SetDureeGame(float duree) {
        this.dureeGame = duree;
    }

    public void AddLumiereHistory(Lumiere lumiere) {
        if (lumieresHistory == null)
            lumieresHistory = new List<ObjectHistory>();
        lumieresHistory.Add(new ObjectHistory(lumiere));
    }

    public void AddEnnemiHistory(Ennemi ennemi) {
        if(ennemisHistory == null)
            ennemisHistory = new List<ObjectHistory>();
        ennemisHistory.Add(new ObjectHistory(ennemi));
    }
}
