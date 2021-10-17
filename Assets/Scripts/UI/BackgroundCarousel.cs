﻿using System;
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
    protected Coroutine coroutine;
    protected Color filtreColor;

    public void Start() {
        alphaFluctuator = new Fluctuator(this, GetAlphaTransition, SetAlphaTransition);
        imageAvant.sprite = GetFirstSprite();
        SetAlphaTransition(alphaOfAvant: 1.0f);
        coroutine = StartCoroutine(CStartSwapping());
        filtreColor = imageArriere.color;
    }

    protected IEnumerator CStartSwapping() {
        while(true) {
            yield return new WaitForSeconds(imageDuration);
            imageArriere.sprite = isRandom ? GetRandomSprite() : GetNextSprite();
            SetAlphaOfArriere(1.0f);
            alphaFluctuator.GoTo(0.0f, transitionDuration);
            yield return new WaitForSeconds(transitionDuration);
            SwapImages();
        }
    }

    protected Sprite GetNextSprite() {
        int spriteIndice = sprites.IndexOf(imageAvant.sprite);
        int newIndice = (spriteIndice + 1) % sprites.Count;
        return sprites[newIndice];
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

    public void GoToBlack() {
        if(coroutine != null) {
            StopCoroutine(coroutine);
        }
        imageArriere.color = Color.black;
        alphaFluctuator.GoTo(0.0f, transitionDuration);
    }

    public void AddFiltre() {
        imageAvant.color = filtreColor;
        imageArriere.color = filtreColor;
    }

    public void RemoveFiltre() {
        imageAvant.color = Color.white;
        imageArriere.color = Color.white;
    }
}