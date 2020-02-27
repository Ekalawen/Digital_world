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

public struct CharacterHistory {
    public Character character;
    public List<TimedVector3> timedPositions;

    public CharacterHistory(Character character) {
        this.character = character;
        timedPositions = new List<TimedVector3>();
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
    protected CharacterHistory playerHistory;
    protected List<CharacterHistory> ennemisHistory;
    protected Timer echantillonnageTimer;

    private void Awake() {
        if (!_instance) { _instance = this; }
        DontDestroyOnLoad(this);
    }

    public void Initialize() {
        gm = GameManager.Instance;
        playerHistory = new CharacterHistory(gm.player);
        ennemisHistory = new List<CharacterHistory>();
        foreach (Ennemi ennemi in gm.ennemiManager.ennemis)
            ennemisHistory.Add(new CharacterHistory(ennemi));
        echantillonnageTimer = new Timer(frequenceEchantillonnagePositions);
        mapSize = gm.map.tailleMap;
        themes = gm.colorManager.themes;
    }

    public void Update() {
        EchantillonnerPositionPlayer();
        EchantillonnerPositionsEnnemis();

        if(echantillonnageTimer.IsOver())
            echantillonnageTimer.Reset();
    }

    protected void EchantillonnerPositionPlayer() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            TimedVector3 tpos = new TimedVector3(gm.player.transform.position, gm.timerManager.GetElapsedTime());
            playerHistory.timedPositions.Add(tpos);
        }
    }

    protected void EchantillonnerPositionsEnnemis() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            for(int i = 0; i < ennemisHistory.Count; i++) {
                CharacterHistory ch = ennemisHistory[i];
                TimedVector3 tpos = new TimedVector3(ch.character.transform.position, gm.timerManager.GetElapsedTime());
                ch.timedPositions.Add(tpos);
            }
        }
    }

    public CharacterHistory GetPlayerHistory() {
        return playerHistory;
    }

    public List<CharacterHistory> GetEnnemisHistory() {
        return ennemisHistory;
    }
}
