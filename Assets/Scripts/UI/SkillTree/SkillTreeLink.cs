using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeLink : MonoBehaviour {

    protected SkillTreeManager skillTreeManager;

    public void Initialize(SkillTreeManager skillTreeManager) {
        this.skillTreeManager = skillTreeManager;
    }
}
