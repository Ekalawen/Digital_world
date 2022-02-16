using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour {

    public List<Achievement.Tag> tags;

    protected GameManager gm;
    protected SelectorManager sm;
    protected bool isInGame;
    protected List<Achievement> achievements = new List<Achievement>();

    public void Initialize(bool isInGame) {
        InitMainManager(isInGame);

        GatherRelevantAchievementsIn(transform);
        InitializeAllAchievements();
    }

    protected void InitMainManager(bool isInGame) {
        this.isInGame = isInGame;
        if (isInGame) {
            gm = GameManager.Instance;
        } else {
            sm = SelectorManager.Instance;
        }
    }

    protected void GatherRelevantAchievementsIn(Transform folder) {
        foreach(Transform t in folder) {
            Achievement achievement = t.GetComponent<Achievement>();
            if(achievement != null && achievement.IsRelevant(this)) {
                achievements.Add(achievement);
            } else {
                GatherRelevantAchievementsIn(t);
            }
        }
    }

    protected void InitializeAllAchievements() {
        foreach(Achievement achievement in achievements) {
            achievement.Initialize();
        }
    }

    public bool IsInGame() {
        return isInGame;
    }

    public List<Achievement.Tag> GetTags() {
        return tags;
    }
}
