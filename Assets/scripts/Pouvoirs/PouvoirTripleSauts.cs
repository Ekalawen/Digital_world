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
    public float tempsPourDash; // Le temps qu'on laisse au joueur pourchoisir sa trajectoire !

    private GameObject blueSphere; // La sphère une fois allumée

    protected override bool UsePouvoir() {
        // Freezer le temps
        pouvoirEnabled = false;
        GameManager.Instance.FreezeTime();
        player.pouvoirHolder.FreezePouvoirs(true);
        // Faire un dash et attendre qu'il se termine
        StartCoroutine(PerformDash(nbDashs));

        return true;
    }

    IEnumerator PerformDash(int dashsRestants) {
        // On a un certain temps pour pouvoir effectuer chaque dash !
        float debutDash = Time.timeSinceLevelLoad;

        // Allumer la boule de couleur
        Vector3 pos = transform.parent.transform.position ;
        blueSphere = Instantiate(blueSpherePrefab, pos, Quaternion.identity) as GameObject;
        blueSphere.transform.localScale = Vector3.one * distanceDash * 2;

        // Faire le dash, pour ça on attends que le joueur relache le bouton de la souris !
        yield return null; // Important pour ne pas trigger le premier dash tout de suite !
        while (!Input.GetMouseButtonDown(0) && Time.timeSinceLevelLoad - debutDash < tempsPourDash
            && !Input.GetMouseButtonDown(1)) {
            // Rendre la main
            yield return null;
        }
        bool appuiClickDroit = Input.GetMouseButtonDown(1);
        yield return null; // Important pour ne pas que le même relachement de la touche affecte tous les performDashs !

        // On éteint la sphère
        DestroyImmediate(blueSphere);

        // Si on a mis trop de temps, le triple dash est brisé !
        // Ou si on a appuyé sur le click droit !
        if (Time.timeSinceLevelLoad - debutDash < tempsPourDash && !appuiClickDroit) {

            // Si il vient de choisir la position, on le TP (pour le moment on s'embête pas ^^')
            Vector3 direction = transform.parent.GetComponent<Player>().camera.transform.forward;
            direction.Normalize();
            yield return StartCoroutine(TranslatePlayer(direction * distanceDash));

            if (dashsRestants - 1 > 0) {
                StartCoroutine(PerformDash(dashsRestants - 1));
            } else {
                // Si c'est la dernière coroutine, defreezer le temps
                pouvoirEnabled = true;
                GameManager.Instance.UnFreezeTime();
                player.pouvoirHolder.FreezePouvoirs(false);
            }
        } else {
            pouvoirEnabled = true;
            GameManager.Instance.UnFreezeTime();
            player.pouvoirHolder.FreezePouvoirs(false);
        }
    }

    IEnumerator TranslatePlayer(Vector3 mouvementTotal) {
        CharacterController controller = transform.parent.GetComponent<Player>().controller;
        float stepOffset = controller.stepOffset;
        controller.stepOffset = 0;
        for(int i = 0; i < nbFramesDash; i++) {
            controller.Move(mouvementTotal / nbFramesDash);
            yield return null;
        }
        controller.stepOffset = stepOffset;
    }

}