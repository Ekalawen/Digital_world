using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeLink : MonoBehaviour {

    public UILineRenderer lineRenderer;
    public GameObject lineUnlockedPrefab;
    public GameObject lineLockedPrefab;

    protected SkillTreeMenu skillTreeManager;
    protected SkillTreeUpgrade sourceUpgrade;
    protected SkillTreeUpgrade targetUpgrade;

    public void Initialize(SkillTreeMenu skillTreeManager, SkillTreeUpgrade sourceUpgrade, SkillTreeUpgrade targetUpgrade) {
        this.skillTreeManager = skillTreeManager;
        this.sourceUpgrade = sourceUpgrade;
        this.targetUpgrade = targetUpgrade;
        InitializeLineRenderer();
    }

    protected void InitializeLineRenderer() {
        Vector2 source = sourceUpgrade.GetComponent<RectTransform>().anchoredPosition;
        Vector2 target = targetUpgrade.GetComponent<RectTransform>().anchoredPosition;
        List<Vector2> positions = new List<Vector2>() { source, target };
        lineRenderer.linePrefab = UnityEngine.Random.value < 0.5f ? lineUnlockedPrefab : lineLockedPrefab;
        lineRenderer.Initialize(positions);
    }
}
