using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrisableCube : NonBlackCube {

    public float dureeBeforeDestruction = 1.5f;
    public Color preDestructionColor = Color.gray;
    public List<Texture> textures;

    protected Coroutine coroutine = null;

    protected override void Start() {
        base.Start();
        Material material = GetComponent<Renderer>().material;
        material.mainTexture = textures[UnityEngine.Random.Range(0, textures.Count)];
        SetOpacity(1);
    }

    public void DestroyInSeconds() {
        if(coroutine == null)
            coroutine = StartCoroutine(CDestroyInSeconds());
    }

    protected IEnumerator CDestroyInSeconds() {
        Timer timer = new Timer(dureeBeforeDestruction);
        while(!timer.IsOver()) {
            float alpha = MathCurves.Power(0, 1, 1 - timer.GetAvancement(), 1.5f);
            SetOpacity(alpha);
            yield return null;
        }
        Explode();
    }

    public override void InteractWithPlayer() {
        DestroyInSeconds();
    }
}
