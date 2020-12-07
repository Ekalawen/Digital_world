using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pointeur : MonoBehaviour {

    public RawImage pointeurImage;

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
    }

    void Update() {
        switch(gm.player.GetEtat()) {
            case Player.EtatPersonnage.AU_SOL:
                pointeurImage.color = auSolColor;
                pointeurImage.rectTransform.sizeDelta = Vector2.one * auSolScale;
                break;
            case Player.EtatPersonnage.EN_SAUT:
                pointeurImage.color = enSautColor;
                pointeurImage.rectTransform.sizeDelta = Vector2.one * enSautScale;
                break;
            case Player.EtatPersonnage.EN_CHUTE:
                pointeurImage.color = enChuteColor;
                pointeurImage.rectTransform.sizeDelta = Vector2.one * enChuteScale;
                break;
            case Player.EtatPersonnage.AU_MUR:
                pointeurImage.color = auMurColor;
                pointeurImage.rectTransform.sizeDelta = Vector2.one * auMurScale;
                break;
        }
    }
}
