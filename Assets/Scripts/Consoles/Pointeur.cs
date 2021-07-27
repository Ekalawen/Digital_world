using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pointeur : MonoBehaviour {

    public RawImage auMurPointeur;
    public RawImage auSolPointeur;

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
    public Texture2D textureFull;
    public Texture2D texture3Dashs;
    public Texture2D texture2Dashs;
    public Texture2D texture1Dashs;
    public Texture2D texture0Dashs;

    protected GameManager gm;
    protected ChargeCooldown tripleDashChargeCooldown = null;

    public void Initialize() {
        gm = GameManager.Instance;
        auSolPointeur.color = auSolColor;
        auSolPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;

        InitTexture();
    }

    void Update() {
        SetColorAndSize();
        UpdateTexture();
    }

    protected void SetColorAndSize() {
        switch (gm.player.GetEtat()) {
            case Player.EtatPersonnage.AU_SOL:
                auMurPointeur.color = auSolColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
                break;
            case Player.EtatPersonnage.EN_SAUT:
                auMurPointeur.color = enSautColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enSautScale;
                break;
            case Player.EtatPersonnage.EN_CHUTE:
                auMurPointeur.color = enChuteColor;
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                break;
            case Player.EtatPersonnage.AU_MUR:
                auMurPointeur.color = GetFinalColorAuMur();
                auMurPointeur.rectTransform.sizeDelta = Vector2.one * GetFinalScaleAuMur();
                break;
        }
    }

    protected void InitTexture() {
        IPouvoir dash = gm.player.GetPouvoirLeftClick().GetComponent<PouvoirDash>();
        if (dash != null) {
            tripleDashChargeCooldown = dash.GetCooldown() as ChargeCooldown;
        }
        if (dash == null || tripleDashChargeCooldown == null) {
            SetTexture(textureFull);
        }
    }

    protected void SetTexture(Texture2D texture) {
        auMurPointeur.texture = texture;
        auSolPointeur.texture = texture;
    }

    protected void UpdateTexture() {
        if(tripleDashChargeCooldown != null) {
            switch (tripleDashChargeCooldown.GetCurrentCharges()) {
                case 0:
                    SetTexture(texture0Dashs);
                    break;
                case 1:
                    SetTexture(texture1Dashs);
                    break;
                case 2:
                    SetTexture(texture2Dashs);
                    break;
                case 3:
                    SetTexture(texture3Dashs);
                    break;
                default:
                    SetTexture(textureFull);
                    break;
            }
        }
    }

    protected Color GetFinalColorAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
        return ColorManager.InterpolateColors(auSolColor, auMurColor, avancement);
    }

    protected float GetFinalScaleAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
        return MathCurves.Linear(auSolScale, auMurScale, avancement);
    }
}
