using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

    [Header("Repop")]
    public bool shouldRepop = false;

    [Header("ScreenShake")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 10;

    protected GameManager gm;
    protected GameObject itemPrefab;
    protected bool isCaptured = false;

    protected virtual void Start() {
        gm = GameManager.Instance;
    }

	protected virtual void OnTriggerEnter(Collider hit) {
		if (!isCaptured && hit.gameObject.name == "Joueur"){
            isCaptured = true;
            gm.soundManager.PlayGetItemClip(transform.position);
            OnTrigger(hit);
            ScreenShakeOnCapture();
            Disappear();
		}
	}

    protected void ScreenShakeOnCapture() {
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.1f, 0.1f);
    }

    public abstract void OnTrigger(Collider hit);

    public virtual void Disappear() {
        gm.itemManager.RemoveItem(this);
        if (shouldRepop) {
            gm.itemManager.PopItem(itemPrefab);
        }
        Destroy();
    }

    public virtual void Destroy() {
        Destroy(this.gameObject);
    }

    public void SetPrefab(GameObject prefab) {
        itemPrefab = prefab;
    }
}
