using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pointeur : MonoBehaviour {

    public RawImage auMurPointeur;
    public RawImage auSolPointeur;
    public RawImage gripDashPointeur;

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

    protected GameManager gm;
    protected ChargeCooldown tripleDashChargeCooldown = null;
    protected NoAutomaticRechargeCooldown gripDashCooldown = null;

    public void Initialize() {
        gm = GameManager.Instance;
        auSolPointeur.color = auSolColor;
        auSolPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
        gripDashPointeur.color = auSolColor;
        gripDashPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;

        InitTexture();
    }

    void Update() {
        SetColorAndSize();
        UpdateTripleDashImage();
        UpdateGripDashImage();
    }

    protected void SetColorAndSize() {
        switch (gm.player.GetEtat()) {
            case Player.EtatPersonnage.AU_SOL:
                auMurPointeur.color = auSolColor;
                gripDashPointeur.color = auSolColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                break;
            case Player.EtatPersonnage.EN_SAUT:
                auMurPointeur.color = enSautColor;
                gripDashPointeur.color = enSautColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                break;
            case Player.EtatPersonnage.EN_CHUTE:
                auMurPointeur.color = enChuteColor;
                gripDashPointeur.color = enChuteColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                break;
            case Player.EtatPersonnage.AU_MUR:
                Color color = GetFinalColorAuMur();
                auMurPointeur.color = color;
                gripDashPointeur.color = color;
                float scale = GetFinalScaleAuMur();
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * scale;
                gripDashPointeur.rectTransform.sizeDelta = Vector2.one * scale;
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
            gripDashCooldown = gripDash.GetCooldown() as NoAutomaticRechargeCooldown;
        } else {
            gripDashPointeur.gameObject.SetActive(false);
        }
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

    protected Color GetFinalColorAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
        return ColorManager.InterpolateColors(auSolColor, auMurColor, avancement);
    }

    protected float GetFinalScaleAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
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
}
