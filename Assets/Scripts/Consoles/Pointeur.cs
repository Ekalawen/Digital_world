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

    protected GameManager gm;

    void Start() {
        gm = GameManager.Instance;
        auSolPointeur.color = auSolColor;
        auSolPointeur.rectTransform.sizeDelta = Vector2.one * auSolScale;
    }

    void Update() {
        switch(gm.player.GetEtat()) {
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

    protected Color GetFinalColorAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
        return ColorManager.InterpolateColors(auSolColor, auMurColor, avancement);
    }

    protected float GetFinalScaleAuMur() {
        float avancement = gm.player.GetDureeRestanteMur() / gm.player.dureeMur;
        return MathCurves.Linear(auSolScale, auMurScale, avancement);
    }
}
