using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundCarousel : MonoBehaviour {

    public List<Sprite> sprites;
    public float imageDuration = 10.0f;
    public float transitionDuration = 0.5f;
    public bool isRandom = true;
    [ConditionalHide("isRandom")]
    public bool firstIsAlwaysTheSame = true;
    public Image imageAvant;
    public Image imageArriere;

    protected Fluctuator alphaFluctuator;

    public void Start() {
        alphaFluctuator = new Fluctuator(this, GetAlphaTransition, SetAlphaTransition);
        imageAvant.sprite = GetFirstSprite();
        SetAlphaTransition(alphaOfAvant: 1.0f);
        StartCoroutine(CStartSwapping());
    }

    protected IEnumerator CStartSwapping() {
        while(true) {
            yield return new WaitForSeconds(imageDuration);
            imageArriere.sprite = GetRandomSprite();
            SetAlphaOfArriere(1.0f);
            alphaFluctuator.GoTo(0.0f, transitionDuration);
            yield return new WaitForSeconds(transitionDuration);
            SwapImages();
        }
    }

    protected Sprite GetRandomSprite() {
        return MathTools.ChoseOne(sprites.FindAll(sprite => sprite != imageAvant.sprite));
    }

    protected void SetAlphaTransition(float alphaOfAvant) {
        Color colorAvant = imageAvant.color;
        colorAvant.a = alphaOfAvant;
        imageAvant.color = colorAvant;
    }

    protected void SetAlphaOfArriere(float alphaOfArriere) {
        Color colorArriere = imageArriere.color;
        colorArriere.a = alphaOfArriere;
        imageArriere.color = colorArriere;
    }

    protected float GetAlphaTransition() {
        return imageAvant.color.a;
    }

    protected void SwapImages() {
        imageAvant.sprite = imageArriere.sprite;
        SetAlphaOfArriere(0.0f);
        SetAlphaTransition(alphaOfAvant: 1.0f);
    }

    protected Sprite GetFirstSprite() {
        if (!isRandom || firstIsAlwaysTheSame) {
            return sprites[0];
        }
        return MathTools.ChoseOne(sprites);
    }
}
