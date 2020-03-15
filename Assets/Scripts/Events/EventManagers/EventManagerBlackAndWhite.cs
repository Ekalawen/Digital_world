using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class EventManagerBlackAndWhite : EventManager { 

    public float topEjectionTreshold = -10.0f; // On peut tomber par en haut ! :D

    protected override bool IsPlayerEjected() {
        return gm.player.transform.position.y < ejectionTreshold
            || gm.player.transform.position.y > topEjectionTreshold;
    }
}