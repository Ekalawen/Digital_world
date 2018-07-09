using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permet au joueur de construire un pont !
/// Ce pont est crée entre la position courante du joueur et jusqu'à un cube visé actuellement par le joueur.
/// Ce pont est crée dynamiquement et va aussi vite que le joueur avance.
/// Lors de la construction de ce pont, il détruit tous les cubes qui sont sur son chemin dans un petit rayon ! =)
/// </summary>
public class PouvoirBridgeBuilder : IPouvoir {

    public GameObject cubePrefab; // Le prefab des cubes à créer pour fabriquer le pont !
    public float vitessePropagationCubes; // La vitesse à laquelle sont crées les cubes !
    public bool isDestructive; // Permet de savoir si ce pouvoir détruit les autres cubes ou pas !
    public float rayonDestruction; // Le rayon de destruction autour duquel on détruit les cubes pour pouvoir passe =)

    private Vector3 pointSource; // Le départ du pont
    private Vector3 pointCible; // La fin du pont

    protected override void usePouvoir() {
        // On récupère les informations du pont en lancant un rayon
        // Et en vérifiant qu'il touche bien un cube
        pointSource = transform.parent.transform.position + Vector3.down; // On essaye de faire partir le pont d'en dessous de nous !
        Ray ray = new Ray(pointSource, transform.parent.GetComponent<PersonnageScript>().camera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(pointSource, transform.parent.GetComponent<PersonnageScript>().camera.transform.forward, Color.red, 1f);
        if(Physics.Raycast(ray, out hit, float.PositiveInfinity)) {
            // On vérifie qu'on a bien touché un cube
            if(hit.collider.tag == "Cube") {
                pointCible = hit.collider.transform.position;

                // Puis on lance sa création, si tout s'est bien passé !
                StartCoroutine(buildBridge());

            } else {
                cibleInvalide();
            }
        } else {
            cibleInvalide();
        }

    }

    void cibleInvalide() {
        // On informe que ce n'est pas une cible valide !
        Debug.Log("Ce n'est pas une cible valide !");
        ConsoleScript.Instance.pouvoirBridgeBuilderInvalide();
    }

    IEnumerator buildBridge() {
        // On calcule la direction
        Vector3 bridgeDirection = (pointCible - pointSource).normalized;

        // On compte le nombre de cubes
        int nbCubes = (int)(pointCible - pointSource).magnitude + 1;

        // Pour chaque cube, on le crée avec un interval !
        for(int i = 0; i < nbCubes; i++) {
            buildCube(pointSource + i * bridgeDirection, Quaternion.LookRotation(bridgeDirection, Vector3.up));
            yield return new WaitForSeconds(vitessePropagationCubes);
        }
    }

    // On construit un cube !
    void buildCube(Vector3 position, Quaternion orientation) {
        // Créer le cube
        Instantiate(cubePrefab, position, orientation);

        // Détruire les autres cubes qui sont autour de lui et qui ne sont pas des cubes de ponts !
        if (isDestructive) {
            Collider[] colliders = Physics.OverlapSphere(position, rayonDestruction);
            foreach (Collider collider in colliders)
            {
                if (collider.tag == "Cube")
                {
                    if (!collider.gameObject.GetComponent<CubeScript>().isBridgeCube)
                    {
                        DestroyImmediate(collider.gameObject);
                    }
                }
            }
        }
    }

}