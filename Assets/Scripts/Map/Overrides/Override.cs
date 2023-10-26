using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Localization;

public abstract class Override: MonoBehaviour {

    public int scoreMultiplier = 2;
    public LocalizedString overrideName;

    protected GameManager gm;
    protected MapManager map;

    public void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;

        InitializeSpecific();
        InitializeScoreMultiplier();
    }

    protected void InitializeScoreMultiplier() {
        InfiniteMap infiniteMap = gm.GetInfiniteMap();
        if(infiniteMap) {
            infiniteMap.GetScoreManager().SetMultiplier(scoreMultiplier);
        }
    }

    protected abstract void InitializeSpecific();

    public virtual string GetName() {
        return overrideName.GetLocalizedString().Result;
    }

    public virtual int GetScoreMultiplier() {
        return scoreMultiplier;
    }
}
