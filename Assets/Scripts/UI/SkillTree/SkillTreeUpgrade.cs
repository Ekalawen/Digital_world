using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUpgrade : MonoBehaviour {

    public List<SkillTreeUpgrade> requirements;

    protected SkillTreeManager skillTreeManager;
    public void Initialize(SkillTreeManager skillTreeManager) {
        this.skillTreeManager = skillTreeManager;
        InitializeRequirementLinks();
    }

    protected void InitializeRequirementLinks() {
        requirements.ForEach(r => skillTreeManager.CreateLink(r, this));
    }
}
