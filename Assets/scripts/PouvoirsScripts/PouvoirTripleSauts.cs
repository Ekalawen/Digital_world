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
    public int nbFramesDash; // Le nombre de frames que doit prendre le dash

    private GameObject blueSphere; // La sphère une fois allumée

    protected override void usePouvoir() {
        Debug.Log("On a bien utilisé le Triple Saut ! <3");
        // Freezer le temps
        pouvoirAvailable = false;
        Time.timeScale = 0;
        Debug.Log("On FREEZE !");
        // Faire un dash et attendre qu'il se termine
        StartCoroutine(performDash(nbDashs));
    }

    IEnumerator performDash(int dashsRestants) {
        // Allumer la boule de couleur
        Vector3 pos = transform.parent.transform.position ;
        blueSphere = Instantiate(blueSpherePrefab, pos, Quaternion.identity) as GameObject;
        blueSphere.transform.localScale = Vector3.one * distanceDash;

        // Faire le dash, pour ça on attends que le joueur relache le bouton de la souris !
        yield return null; // Important pour ne pas trigger le premier dash tout de suite !
        while (!Input.GetMouseButtonDown(0)) {
            // Rendre la main
            yield return null;
        }
        yield return null; // Important pour ne pas que le même relachement de la touche affecte tous les performDashs !

        // On éteint la sphère
        DestroyImmediate(blueSphere);

        // Si il vient de choisir la position, on le TP (pour le moment on s'embête pas ^^')
        Vector3 direction = transform.parent.GetComponent<PersonnageScript>().camera.transform.forward;
        direction.Normalize();
        Debug.Log("début");
        yield return StartCoroutine(translatePlayer(direction * distanceDash));
        Debug.Log("fin");

        Debug.Log("dashsRestants = " + dashsRestants);
        if(dashsRestants - 1 > 0) {
            StartCoroutine(performDash(dashsRestants - 1));
        } else {
            // Si c'est la dernière coroutine, defreezer le temps
            pouvoirAvailable = true;
            Time.timeScale = 1;
            Debug.Log("On defreeze !");
        }
    }

    IEnumerator translatePlayer(Vector3 mouvementTotal) {
        CharacterController controller = transform.parent.GetComponent<PersonnageScript>().controller;
        float slopeLimit = controller.slopeLimit;
        float stepOffset = controller.stepOffset;
        controller.slopeLimit = 0;
        controller.stepOffset = 0;
        for(int i = 0; i < nbFramesDash; i++) {
            controller.Move(mouvementTotal / nbFramesDash);
            yield return null;
        }
        controller.slopeLimit = slopeLimit;
        controller.stepOffset = stepOffset;
    }

}