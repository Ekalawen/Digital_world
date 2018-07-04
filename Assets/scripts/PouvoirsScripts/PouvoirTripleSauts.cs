using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permet au joueur d'effectuer un triple saut ! Amazing ! :D
/// Du début à la fin du triple saut, le temps est arrêté.
/// Il peut rester appuyer sur son bouton, puis s'il relache il dash dans la direction visé sur une certaine distance.
/// Tant qu'il reste appuyé sur le bouton, une sphère bleu lui indique la portée maximale de son dash.
/// Puis il doit recommencer 2 fois !
/// </summary>
public class PouvoirTripleSauts : IPouvoir {

    public int nbDashs; // Le nombre de dashs que peut faire le joueur

    public override void usePouvoir() {
        Debug.Log("On a bien utilisé le Triple Saut ! <3");
        // Freezer le temps
        for (int i = 0; i < nbDashs; i++) {
            // Faire un dash et attendre qu'il se termine
            StartCoroutine(performDash());
        }
        // Defreezer le temps
    }

    IEnumerator performDash() {
        // Faire le dash
        // Rendre la main
        yield return null;
    }

}
