using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pointeur : MonoBehaviour {

    public RawImage auMurPointeur;
    public RawImage auSolPointeur;
    public RawImage gripDashPointeur;
    public RawImage timeHackPointeur;

    [Header("Couleurs")]
    public Color auSolColor;
    public Color enSautColor;
    public Color enChuteColor;
    public Color auMurColor;

    [Header("Scales")]
    public float auSolScale = 3;
    public float enSautScale = 3;
    public float enChuteScale = 3;
    public float auMurScale = 5;

    [Header("Textures")]
    public Texture2D dashDefaultTexture;
    public List<Texture2D> textureByDashCharges;

    [Header("GripDash")]
    public Texture2D gripDefaultTexture;
    public Texture2D gripTargetingTexture;

    protected GameManager gm;
    protected ChargeCooldown tripleDashChargeCooldown = null;
    protected ChargeCooldown gripDashCooldown = null;
    protected PouvoirTimeHack timeHack = null;
    protected bool hasSlideUi;

    public void Initialize() {
        gm = GameManager.Instance;
        auSolPointeur.color = auSolColor;
        auSolPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
        gripDashPointeur.color = auSolColor;
        gripDashPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
        timeHackPointeur.color = auSolColor;
        timeHackPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
        hasSlideUi = SkillTreeManager.Instance.IsEnabled(SkillKey.BLUE_SLIDE);

        InitTexture();
    }

    void Update() {
        SetColorAndSize();
        UpdateTripleDashImage();
        UpdateGripDashImage();
        UpdateTimeHackImage();
    }

    protected void SetColorAndSize() {
        Player.EtatPersonnage etat = gm.player.GetEtat();
        if(!hasSlideUi && etat == Player.EtatPersonnage.AU_MUR) {
            etat = Player.EtatPersonnage.AU_SOL;
        }
        switch (etat) {
            case Player.EtatPersonnage.AU_SOL:
                auMurPointeur.color = auSolColor;
                gripDashPointeur.color = auSolColor;
                timeHackPointeur.color = auSolColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                timeHackPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                break;
            case Player.EtatPersonnage.EN_SAUT:
                auMurPointeur.color = enSautColor;
                gripDashPointeur.color = enSautColor;
                timeHackPointeur.color = enSautColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                timeHackPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                break;
            case Player.EtatPersonnage.EN_CHUTE:
                auMurPointeur.color = enChuteColor;
                gripDashPointeur.color = enChuteColor;
                timeHackPointeur.color = enChuteColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                timeHackPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                break;
            case Player.EtatPersonnage.AU_MUR:
                Color color = GetFinalColorAuMur();
                auMurPointeur.color = color;
                gripDashPointeur.color = color;
                timeHackPointeur.color = color;
                float scale = GetFinalScaleAuMur();
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * scale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * scale;
                timeHackPointeur.rectTransform.sizeDelta = Vector2.one * scale;
                break;
        }
    }

    protected void InitTexture() {
        IPouvoir dash = gm.player.GetPouvoirLeftClick();
        if (dash != null) {
            tripleDashChargeCooldown = dash.GetCooldown() as ChargeCooldown;
        }
        if (dash == null || tripleDashChargeCooldown == null) {
            SetTexture(dashDefaultTexture);
        }

        IPouvoir gripDash = gm.player.GetPouvoirRightClick();
        if (gripDash != null) {
            gripDashCooldown = gripDash.GetCooldown() as ChargeCooldown;
        } else {
            gripDashPointeur.gameObject.SetActive(false);
        }

        timeHack = gm.player.GetTimeHack();
        timeHackPointeur.gameObject.SetActive(timeHack != null);
    }

    protected void SetTexture(Texture2D texture) {
        auMurPointeur.texture = texture;
        auSolPointeur.texture = texture;
    }

    protected void UpdateTripleDashImage() {
        if(tripleDashChargeCooldown == null) {
            return;
        }
        int nbCharges = Mathf.Min(tripleDashChargeCooldown.GetCurrentCharges(), textureByDashCharges.Count - 1);
        Texture2D textureToUse = textureByDashCharges[nbCharges];
        SetTexture(textureToUse);
    }

    protected void UpdateGripDashImage() {
        if(gripDashCooldown == null) {
            return;
        }
        gripDashPointeur.gameObject.SetActive(gripDashCooldown.GetCurrentCharges() > 0);
    }

    protected void UpdateTimeHackImage() {
        if(timeHack == null) {
            return;
        }
        timeHackPointeur.gameObject.SetActive(!timeHack.IsActive() && timeHack.IsAvailable());
    }

    protected Color GetFinalColorAuMur() {
        float avancement = Mathf.Max(0, gm.player.GetDureeRestanteMur() / gm.player.GetDureeMur()); // Max because when stunned, getDureeRestanteMur peut être négative !
        return ColorManager.InterpolateColors(auSolColor, auMurColor, avancement);
    }

    protected float GetFinalScaleAuMur() {
        float avancement = Mathf.Max(0, gm.player.GetDureeRestanteMur() / gm.player.GetDureeMur()); // Max because when stunned, getDureeRestanteMur peut être négative !
        return MathCurves.Linear(auSolScale, auMurScale, avancement);
    }

    public void DisableMainPointeur() {
        auSolPointeur.gameObject.SetActive(false);
        auMurPointeur.gameObject.SetActive(false);
    }

    public void EnableMainPointeur() {
        auSolPointeur.gameObject.SetActive(true);
        auMurPointeur.gameObject.SetActive(true);
    }

    public void SetDefaultGripPointeur() {
        gripDashPointeur.texture = gripDefaultTexture;
    }

    public void SetTargetingGripPointeur() {
        gripDashPointeur.texture = gripTargetingTexture;
    }
}
