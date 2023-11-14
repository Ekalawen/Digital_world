using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour {

    public Transform upgradesFolder;
    public Transform linksFolder;
    public GameObject linkPrefab;

    [Header("Resizing")]
    public LayoutElement layoutElement;
    public CanvasScaler canvasScaler;
    public float openingDuration = 1.0f;
    public float closingDuration = 1.0f;
    public AnimationCurve resizingCurve;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;
    protected bool isOpen = false;
    protected Fluctuator sizeFluctuator;
    protected SingleCoroutine setActiveCoroutine;

    public void Initilalize() {
        links = new List<SkillTreeLink>();
        sizeFluctuator = new Fluctuator(this, GetSizeRatio, SetSizeRatio, resizingCurve);
        SetSizeRatio(0.0f);
        setActiveCoroutine = new SingleCoroutine(this);
        GatherUpgrades();
        InitializeUpgrades();
    }

    protected void SetSizeRatio(float percentageValue) {
        float pixelValue = canvasScaler.referenceResolution.x * percentageValue;
        layoutElement.preferredWidth = pixelValue;
    }

    protected float GetSizeRatio() {
        float width = layoutElement.preferredWidth;
        return width / canvasScaler.referenceResolution.x;
    }

    protected void InitializeUpgrades() {
        upgrades.ForEach(u => u.Initialize(this));
    }

    protected void GatherUpgrades() {
        upgrades = new List<SkillTreeUpgrade>();
        GatherUpgradesIn(upgradesFolder);
    }

    protected void GatherUpgradesIn(Transform folder) {
        foreach (Transform child in folder) {
            SkillTreeUpgrade upgrade = child.GetComponent<SkillTreeUpgrade>();
            if(upgrade) {
                upgrades.Add(upgrade);
            } else {
                GatherUpgradesIn(child);
            }
        }
    }

    public void CreateLink(SkillTreeUpgrade source, SkillTreeUpgrade target) {
        SkillTreeLink link = Instantiate(linkPrefab, linksFolder).GetComponent<SkillTreeLink>();
        RectTransform rect = link.GetComponent<RectTransform>();
        rect.anchorMin = Vector3.zero;
        rect.anchorMax = Vector3.one;
        rect.offsetMin = Vector3.zero;
        rect.offsetMax = Vector3.zero;
        link.Initialize(this, source, target);
        links.Add(link);
    }

    public void Toggle() {
        if(isOpen) {
            Close();
        } else {
            Open();
        }
    }

    protected void Open() {
        isOpen = true;
        SetActive(true);
        setActiveCoroutine.Stop();
        sizeFluctuator.GoTo(1.0f, openingDuration);
    }

    protected void Close() {
        isOpen = false;
        sizeFluctuator.GoTo(0.0f, closingDuration);
        setActiveCoroutine.Start(CSetActiveIn(closingDuration, false));
    }

    protected IEnumerator CSetActiveIn(float duration, bool isActive) {
        yield return new WaitForSeconds(duration);
        SetActive(isActive);
    }

    protected void SetActive(bool isActive) {
        gameObject.SetActive(isActive);
    }

    public bool IsOpen() {
        return isOpen;
    }
}
