using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	[HideInInspector]
	public CharacterController controller;

	[HideInInspector]
    public List<Poussee> poussees;

    public virtual void Start() {
		controller = this.GetComponent<CharacterController> ();
        poussees = new List<Poussee>();
    }

    public void AddPoussee(Poussee poussee) {
        poussees.Add(poussee);
    }

    public virtual void ApplyPoussees() {
        for(int i = 0; i < poussees.Count; i++) {
            if(poussees[i].IsOver()) {
                poussees.RemoveAt(i);
                i--;
            }
        }

        foreach(Poussee poussee in poussees) {
            poussee.ApplyPoussee(controller);
        }
    }
}
