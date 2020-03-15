using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrisableCube : Cube {

    public float dureeBeforeDestruction = 1.5f;
    public Color preDestructionColor = Color.gray;

    protected Coroutine coroutine = null;

    public void DestroyInSeconds() {
        if(coroutine == null)
            coroutine = StartCoroutine(CDestroyInSeconds());
    }

    protected IEnumerator CDestroyInSeconds() {
        SetColor(preDestructionColor);
        yield return new WaitForSeconds(dureeBeforeDestruction);
        Explode();
    }

    public override void InteractWithPlayer() {
        DestroyInSeconds();
    }
}
