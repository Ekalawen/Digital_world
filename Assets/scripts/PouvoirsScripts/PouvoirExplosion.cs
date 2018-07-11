using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permet au joueur de générer une onde de choc qui stun les ennemis et détruits tous les cubes aux alentours !
/// L'onde de choc est envoyé vers l'avant, elle explose automatiquement au boût du chemin, où si elle rencontre un obstacle en chemin !
/// </summary>
public class PouvoirExplosion : IPouvoir {

    public GameObject redSpherePrefab; // La boule unitaire de couleur qui permet de visualiser la portée de l'explosion
    public float distanceBlast; // La distance du tir
    public float porteeExplosion; // La distance maximale de l'explosion
    public float tempsIncantationExplosion; // Le temps avant que l'explosion ait lieu !
    public float puissanceDePoussee; // La puissance avec laquelle les ennemis sont poussés !
    public float tempsDeLaPoussee; // Le temps pendant lequel les ennemis sont poussés !


    protected override void usePouvoir() {
        // On ne peut pas faire 10 000 explosions à la fois ^^
        // Bah avec cette version on peut :D

        // On lance l'explosion !
        StartCoroutine(performExplosion());
    }

    IEnumerator performExplosion() {
        // On retient le temps
        float tempsDebut = Time.timeSinceLevelLoad;
        Vector3 direction = transform.parent.GetComponent<PersonnageScript>().camera.transform.forward.normalized;
        Vector3 centre = transform.parent.transform.position + direction * 0.5f;

        // On crée la boule rouge
        GameObject redSphere = Instantiate(redSpherePrefab, centre, Quaternion.identity) as GameObject; // La sphère une fois allumée

        // On fait grossir la boule jusqu'à ce qu'il faille qu'elle explose !
        yield return StartCoroutine(deplacerBoule(redSphere, centre, direction, tempsDebut));

        // La boule est arrivée à son terme ...
        // ON EXPLOSE !!!
        float ratioTemps = (Time.timeSinceLevelLoad - tempsDebut) / tempsIncantationExplosion;
        float rayonExplosion = Mathf.Max(0.5f, porteeExplosion * ratioTemps);
        Vector3 centreExplosion = centre + (ratioTemps * direction * distanceBlast);
        Collider[] colliders = Physics.OverlapSphere(centreExplosion, rayonExplosion);
        foreach (Collider collider in colliders) {
            if (collider.tag == "Cube") {
                DestroyImmediate(collider.gameObject);
            } else if(collider.tag == "Ennemi") {
                // Vector3 directionPoussee = (collider.gameObject.transform.position - centreExplosion).normalized;
                Vector3 directionPoussee = (collider.gameObject.transform.position - transform.parent.transform.position).normalized;
                float tempsDeLaPousseeFinal = Mathf.Max(0.2f, tempsDeLaPoussee * ratioTemps);
                collider.gameObject.GetComponent<SondeScript>().etrePoussee(directionPoussee * puissanceDePoussee, tempsDeLaPousseeFinal);
            }
        }

        // Et on supprime la boule, et on rend la main sur le pouvoir
        DestroyImmediate(redSphere);
    }

    IEnumerator deplacerBoule(GameObject redSphere, Vector3 centre, Vector3 direction, float tempsDebut) {
        // Tant que l'on explose pas ...
        while(!shouldExplodes(redSphere.transform.position, tempsDebut, redSphere.transform.localScale.x / 2)) {
            // On aggrandit la taille de notre bouboule <3
            float ratioTemps = (Time.timeSinceLevelLoad - tempsDebut) / tempsIncantationExplosion;
            redSphere.transform.localScale = Vector3.one * (porteeExplosion * ratioTemps * 2);

            // Et on la fait se déplacer =)
            redSphere.transform.position = centre + (ratioTemps * direction * distanceBlast);
            yield return null;
        }
    }

    bool shouldExplodes(Vector3 centre, float tempsDebut, float rayon) {
        // Si c'est la fin du temps imparti il faut exploser
        if(Time.timeSinceLevelLoad - tempsDebut >= tempsIncantationExplosion) {
            return true;
        }

        // Si le centre entre en contact avec un cube
        Collider[] colliders = Physics.OverlapSphere(centre, 0.3f); // O.5f pour symboliser le contact, oui c'est pas ouf
        foreach (Collider collider in colliders) {
            if (collider.tag == "Cube" || collider.tag == "Ennemi") {
                return true;
            }
        }
        
        // Ou que la boule elle-même entre en contact avec un ennemi
        colliders = Physics.OverlapSphere(centre, rayon);
        foreach (Collider collider in colliders) {
            if (collider.tag == "Ennemi") {
                return true;
            }
        }

        // Sinon c'est pas encore  le moment d'exploser =)
        return false;
    }
}