using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

    public bool shouldRepop = false;

    protected GameManager gm;
    protected GameObject itemPrefab;

    protected virtual void Start() {
        gm = GameManager.Instance;
    }

	protected virtual void OnTriggerEnter(Collider hit) {
		if (hit.gameObject.name == "Joueur"){
            gm.soundManager.PlayGetItemClip(transform.position);
            OnTrigger(hit);
            Disappear();
		}
	}

    public abstract void OnTrigger(Collider hit);

    public virtual void Disappear() {
        gm.itemManager.RemoveItem(this);
        if (shouldRepop) {
            gm.itemManager.PopItem(itemPrefab);
        }
        Destroy(this.gameObject);
    }

    public void SetPrefab(GameObject prefab) {
        itemPrefab = prefab;
    }
}
