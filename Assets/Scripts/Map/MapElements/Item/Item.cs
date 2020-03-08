using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

    public bool shouldRepop = false;

    protected GameManager gm;

    void Start() {
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
            gm.itemManager.PopItem(gameObject);
        }
        Destroy(this.gameObject);
    }
}
