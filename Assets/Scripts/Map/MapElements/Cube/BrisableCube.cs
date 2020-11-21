using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrisableCube : Cube {

    public float dureeBeforeDestruction = 1.5f;
    public Color preDestructionColor = Color.gray;
    public List<Texture> textures;

    protected Coroutine coroutine = null;

    protected override void Start() {
        base.Start();
        Material material = GetComponent<Renderer>().material;
        material.mainTexture = textures[UnityEngine.Random.Range(0, textures.Count)];
    }

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
