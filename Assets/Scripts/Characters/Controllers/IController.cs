using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IController : MonoBehaviour {

	public CharacterController controller;

    protected GameManager gm;

    public virtual void Start() {
        gm = GameManager.Instance;
		controller = this.GetComponent<CharacterController> ();
        if (controller == null)
            Debug.LogError("Il est nécessaire d'avoir un CharacterController avec un " + name + " !");
    }

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(gm.IsTimeFreezed()) {
            return;
        }

        UpdateSpecific();
	}

    protected abstract void UpdateSpecific();
}
