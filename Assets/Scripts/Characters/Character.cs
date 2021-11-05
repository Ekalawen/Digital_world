﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	[HideInInspector]
	public CharacterController controller;

	[HideInInspector]
    public List<Poussee> poussees;

	[HideInInspector]
    public SpeedMultiplierController speedMultiplierController;

    public virtual void Start() {
		controller = this.GetComponent<CharacterController> ();
        poussees = new List<Poussee>();
        InitSpeedMultiplierController();
    }

    public void AddPoussee(Poussee poussee) {
        poussees.Add(poussee);
    }

    public void RemoveAllPoussees() {
        poussees.Clear();
    }

    public virtual void ApplyPoussees() {
        for(int i = 0; i < poussees.Count; i++) {
            if(poussees[i].IsOver()) {
                poussees.RemoveAt(i);
                i--;
            }
        }

        for(int i = 0; i < poussees.Count; i++) {
            Poussee poussee = poussees[i];
            poussee.ApplyPoussee(controller);
        }
    }

    public virtual Vector3 ComputePoussees() {
        for(int i = 0; i < poussees.Count; i++) {
            if(poussees[i].IsOver()) {
                poussees.RemoveAt(i);
                i--;
            }
        }

        Vector3 pousseeTotale = Vector3.zero;
        for(int i = 0; i < poussees.Count; i++) {
            Poussee poussee = poussees[i];
            pousseeTotale += poussee.ComputePoussee(controller);
        }
        return pousseeTotale;
    }

    protected void InitSpeedMultiplierController() {
        speedMultiplierController = GetComponent<SpeedMultiplierController>();
        if(speedMultiplierController == null) {
            speedMultiplierController = gameObject.AddComponent<SpeedMultiplierController>();
        }
        speedMultiplierController.Initialize(this);
    }

    public float GetSpeedMultiplier() {
        return speedMultiplierController.GetMultiplier();
    }

    public SpeedMultiplier AddMultiplier(SpeedMultiplier speedMultiplier) {
        return speedMultiplierController.AddMultiplier(speedMultiplier);
    }
}
