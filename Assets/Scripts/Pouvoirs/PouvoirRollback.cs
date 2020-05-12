using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EnnemiCurve {
    public Ennemi ennemi;
    public Curve curve;

    public EnnemiCurve(Ennemi ennemi, Curve curve) {
        this.ennemi = ennemi;
        this.curve = curve;
    }
}

public class PouvoirRollback : IPouvoir {

    public float dureeRevertedMax = 3.0f;
    public float dureeTotalRevert = 1.0f;
    public bool shouldRevertEnnemis = true;

    protected float startedTime;
    protected float acceleration;
    protected Timer timer;
    protected Curve playerCurve;
    protected List<EnnemiCurve> ennemisCurves;

    public override void Start() {
        base.Start();
        acceleration = dureeRevertedMax / dureeTotalRevert;
    }

    protected override bool UsePouvoir() {
        StartCoroutine(CUpdatePouvoir());
        return true;
    }

    protected Curve CreateReversedLastRelevantCurveFromHistory(ObjectHistory history) {
        ObjectHistory lastHistory = new ObjectHistory(history.obj);
        for (int i = history.positions.Count - 1; i >= 0; i--) {
            if (IsRecent(history.positions[i].time)) {
                lastHistory.positions.Add(history.positions[i]);
            }
        }
        Curve curve = RewardObjectDisplayer.CreateCurveFromHistory(lastHistory, acceleration);
        return curve;
    }

    protected bool IsRecent(float time) {
        return time >= startedTime - dureeRevertedMax;
    }

    protected void UpdatePlayer(float avancement) {
        Vector3 pos = playerCurve.GetAvancement(avancement);
        gm.player.transform.position = pos;
    }

    protected void UpdateEnnemis(float avancement) {
        foreach(EnnemiCurve ennemiCurve in ennemisCurves) {
            Vector3 pos = ennemiCurve.curve.GetAvancement(avancement);
            ennemiCurve.ennemi.transform.position = pos;
        }
    }

    protected virtual void InitPouvoir() {
        timer = new Timer(dureeTotalRevert);
        startedTime = Time.timeSinceLevelLoad;
        playerCurve = CreateReversedLastRelevantCurveFromHistory(gm.historyManager.GetPlayerHistory());
        if (shouldRevertEnnemis) {
            ennemisCurves = new List<EnnemiCurve>();
            foreach (ObjectHistory ennemiHistory in gm.historyManager.GetEnnemisHistory()) {
                if (ennemiHistory.obj != null) {
                    Curve curve = CreateReversedLastRelevantCurveFromHistory(ennemiHistory);
                    Ennemi ennemi = ennemiHistory.obj.GetComponent<Ennemi>();
                    ennemisCurves.Add(new EnnemiCurve(ennemi, curve));
                }
            }
        }

        gm.FreezeTime();
        player.FreezePouvoirs(true);
        player.RemoveAllPoussees();
    }

    protected void UnInitPouvoir() {
        player.FreezePouvoirs(false);
        gm.UnFreezeTime();
        player.ResetGrip();
    }

    protected IEnumerator CUpdatePouvoir() {
        InitPouvoir();
        while (!timer.IsOver() && Input.GetKey(binding)) {
            float avancement = timer.GetAvancement();
            UpdatePlayer(avancement);
            if(shouldRevertEnnemis)
                UpdateEnnemis(avancement);
            yield return null;
        }
        UnInitPouvoir();
    }
}
