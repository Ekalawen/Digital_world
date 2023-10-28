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

    public void Initialize(bool applyMultiplier = true) {
        gm = GameManager.Instance;
        map = gm.map;

        InitializeSpecific();
        if (applyMultiplier) {
            InitializeScoreMultiplier();
        }
    }

    protected void InitializeScoreMultiplier() {
        InfiniteMap infiniteMap = gm.GetInfiniteMap();
        if(infiniteMap) {
            infiniteMap.GetScoreManager().SetMultiplier(GetScoreMultiplier());
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
