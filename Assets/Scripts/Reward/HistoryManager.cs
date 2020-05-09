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
    public GameObject prefab; // Only used for items !
    public List<TimedVector3> positions;

    public ObjectHistory(MonoBehaviour obj) {
        this.obj = obj;
        positions = new List<TimedVector3>();
        positions.Add(new TimedVector3(obj.transform.position, GameManager.Instance.timerManager.GetElapsedTime()));
        prefab = null;
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
    public Vector3 mapCenter; // Different de mapSize pour le tutoriel !
    [HideInInspector]
    public List<ColorSource.ThemeSource> themes;
    protected ObjectHistory playerHistory;
    protected List<ObjectHistory> ennemisHistory;
    protected List<ObjectHistory> lumieresHistory;
    protected List<ObjectHistory> itemsHistory;
    protected List<TimedMessage> timedMessages;
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
        if(itemsHistory == null)
            itemsHistory = new List<ObjectHistory>();
        if(timedMessages == null)
            timedMessages = new List<TimedMessage>();

        echantillonnageTimer = new Timer(frequenceEchantillonnagePositions);
        mapSize = gm.map.tailleMap;
        mapCenter = gm.map.GetCenter();
        themes = gm.colorManager.themes;
    }

    public void Update() {
        EchantillonnerPositionPlayer();
        EchantillonnerPositionsEnnemis();
        EchantillonnerPositionsLumieres();
        EchantillonnerPositionsItems();

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
                if (ch.obj != null) { // Il est maintenant possible que certains ennemis soient détruits ! ==> peut engendrer des bugs !
                    TimedVector3 tpos = new TimedVector3(ch.obj.transform.position, gm.timerManager.GetElapsedTime());
                    ch.positions.Add(tpos);
                }
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

    protected void EchantillonnerPositionsItems() {
        if(!gm.eventManager.IsWin() && echantillonnageTimer.IsOver()) {
            for(int i = 0; i < itemsHistory.Count; i++) {
                ObjectHistory ch = itemsHistory[i];
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

    public List<ObjectHistory> GetItemsHistory() {
        return itemsHistory;
    }

    public float GetDureeGame() {
        return dureeGame;
    }
    public void SetDureeGame(float duree) {
        this.dureeGame = duree;
    }

    public void AddLumiereHistory(Lumiere lumiere, GameObject lumierePrefab) {
        if (lumieresHistory == null)
            lumieresHistory = new List<ObjectHistory>();
        ObjectHistory history = new ObjectHistory(lumiere);
        history.prefab = lumierePrefab;
        lumieresHistory.Add(history);
    }

    public void AddItemHistory(Item item, GameObject itemPrefab) {
        if (itemsHistory == null)
            itemsHistory = new List<ObjectHistory>();
        ObjectHistory history = new ObjectHistory(item);
        history.prefab = itemPrefab;
        itemsHistory.Add(history);
    }

    public void AddEnnemiHistory(Ennemi ennemi) {
        if(ennemisHistory == null)
            ennemisHistory = new List<ObjectHistory>();
        ennemisHistory.Add(new ObjectHistory(ennemi));
    }

    public void AddConsoleMessage(TimedMessage timedMessage) {
        if (timedMessages == null)
            timedMessages = new List<TimedMessage>();
        timedMessages.Add(timedMessage);
    }

    public List<TimedMessage> GetTimedMessages() {
        return timedMessages;
    }
}
