﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un pouvoir est un effet que le joueur peut activer en appuyant sur une touche.
/// Il s'agit ici d'une classe abstraite dont hériteront les différents pouvoirs.
/// Le joueur ne pourra normalement posséder qu'un seul pouvoir à la fois =)
/// </summary>
public abstract class IPouvoir : MonoBehaviour {

    protected bool pouvoirAvailable;
    protected GameManager gm;

    public void Start() {
        pouvoirAvailable = true;
        gm = FindObjectOfType<GameManager>();
    }

    // La fonction appelée lorsque le joueur appui sur une touche
    public void tryUsePouvoir() {
        if(pouvoirAvailable) {
            usePouvoir();
        }
    }

    // La véritable fonction qui appelle le pouvoir
    protected abstract void usePouvoir();
}
