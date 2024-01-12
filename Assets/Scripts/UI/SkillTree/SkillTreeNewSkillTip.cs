using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNewSkillTip : MonoBehaviour {

    public GameObject newSkillTip;

    protected GameManager gm;
    protected SkillTreeMenu skillTreeMenu;
    protected ScoreManager scoreManager;

    public void Initialize() {
        gm = GameManager.Instance;
        skillTreeMenu = gm.console.GetPauseMenu().skillTreeMenu;
        scoreManager = gm.GetInfiniteMap().GetScoreManager();
        scoreManager.onScoreChange.AddListener(score => SetActiveNewSkillTip());
        skillTreeMenu.onClose.AddListener(SetActiveNewSkillTip);
        SetActiveNewSkillTip();
        gm.onFirstFrame.AddListener(TryOpenSkillTreeMenu);
    }

    public void SetActiveNewSkillTip() {
        int additionnalCredits = scoreManager.GetCurrentScore();
        newSkillTip.SetActive(skillTreeMenu.HasNewlyAffordableUpgrades(additionnalCredits));
    }

    protected void TryOpenSkillTreeMenu() {
        if (newSkillTip.activeInHierarchy) {
            gm.Pause();
            gm.console.GetPauseMenu().OpenSkillTree();
        }
    }
}
