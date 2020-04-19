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

    protected override bool UsePouvoir() {
        Vector3 pointSource; // Le départ du pont
        Vector3 pointCible = Vector3.zero; // La fin du pont

        // On récupère les informations du pont en lancant un rayon
        // Et en vérifiant qu'il touche bien un cube
        pointSource = transform.parent.transform.position + gm.gravityManager.Down(); // On essaye de faire partir le pont d'en dessous de nous !
        Ray ray = new Ray(transform.parent.transform.position, transform.parent.GetComponent<Player>().camera.transform.forward); // On part pas d'en-dessous pour ne pas que le ray soit bloqué !
        RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity);
        bool touched = false;
        foreach(RaycastHit hit in hits) {
            Cube cube = hit.collider.gameObject.GetComponent<Cube>();
            if(cube != null) {
                pointCible = hit.collider.transform.position;
                touched = true;
                break;
            }
        }
        if(touched) {
            StartCoroutine(BuildBridge(pointSource, pointCible));
            return true;
        } else {
            CibleInvalide();
            return false;
        }
    }

    void CibleInvalide() {
        // On informe que ce n'est pas une cible valide !
        gm.console.PouvoirBridgeBuilderInvalide();
    }

    IEnumerator BuildBridge(Vector3 pointSource, Vector3 pointCible) {
        // On calcule la direction
        Vector3 bridgeDirection = (pointCible - pointSource).normalized;

        // On compte le nombre de cubes
        int nbCubes = (int)Mathf.Ceil((pointCible - pointSource).magnitude);

        // Pour chaque cube, on le crée avec un interval !
        for(int i = 0; i < nbCubes; i++) {
            Vector3 pos = pointSource + i * bridgeDirection;
            BuildCube(pos, Quaternion.LookRotation(bridgeDirection, gm.gravityManager.Up()));
            yield return new WaitForSeconds(vitessePropagationCubes);
        }
    }

    // On construit un cube !
    void BuildCube(Vector3 position, Quaternion orientation) {
        // Créer le cube
        if (FarEnoughtFromLumieres(position)) {
            Cube cube = gm.map.AddCube(position, Cube.CubeType.NORMAL, orientation);
            cube.ShouldRegisterToColorSources();
        }

        gm.soundManager.PlayCreateCubeClip(position);

        // Détruire les autres cubes qui sont autour de lui et qui ne sont pas des cubes de ponts !
        if (isDestructive) {
            List<Cube> cubes = gm.map.GetCubesInSphere(position, rayonDestruction);
            foreach (Cube c in cubes) {
                if (c.bIsRegular) {
                    gm.map.DeleteCube(c);
                }
            }
        }
    }

    protected bool FarEnoughtFromLumieres(Vector3 pos) {
        foreach(Lumiere lumiere in gm.map.GetLumieres()) {
            if (MathTools.AABBSphere(pos, Vector3.one * 0.5f, lumiere.transform.position, lumiere.transform.localScale.x / 2.0f))
                return false;
        }
        return true;
    }
}