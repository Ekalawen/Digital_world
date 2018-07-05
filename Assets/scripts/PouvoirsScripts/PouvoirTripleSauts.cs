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

    public GameObject blueSpherePrefab; // La boule unitaire de couleur qui permet de visualiser la portée du dash
    public int nbDashs; // Le nombre de dashs que peut faire le joueur
    public float distanceDash; // La distance maximale du dash

    private GameObject blueSphere; // La sphère une fois allumée

    public override void usePouvoir() {
        Debug.Log("On a bien utilisé le Triple Saut ! <3");
        // Freezer le temps
        Time.timeScale = 0;
        Debug.Log("On FREEZE !");
        // Faire un dash et attendre qu'il se termine
        StartCoroutine(performDash(nbDashs));
    }

    IEnumerator performDash(int nbDashs) {
        // Allumer la boule de couleur
        Vector3 pos = transform.parent.transform.position ;
        blueSphere = Instantiate(blueSpherePrefab, pos, Quaternion.identity) as GameObject;
        blueSphere.transform.localScale = Vector3.one * distanceDash;

        // Faire le dash, pour ça on attends que le joueur relache le bouton de la souris !
        while (!Input.GetMouseButtonUp(0)) {
            // Rendre la main
            yield return null;
        }

        // Si il vient de choisir la position, on le TP (pour le moment on s'embête pas ^^')
        Vector3 direction = transform.parent.GetComponent<PersonnageScript>().camera.transform.forward;
        direction.Normalize();
        transform.parent.transform.Translate(direction * distanceDash);

        // On éteint la sphère
        DestroyImmediate(blueSphere);

        if(nbDashs > 0) {
            performDash(nbDashs - 1);
        } else {
            // Si c'est la dernière coroutine, defreezer le temps
            Time.timeScale = 1;
            Debug.Log("On defreeze !");
        }
    }

}