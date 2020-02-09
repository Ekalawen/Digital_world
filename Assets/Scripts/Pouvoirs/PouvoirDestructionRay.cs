using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permet au joueur de détruire le premier cube devant lui ! :)
/// </summary>
public class PouvoirDestructionRay : IPouvoir {

    public float porteeRay; // La distance du tir
    public bool bDestroyAllCubes = false;

    protected override bool UsePouvoir() {
        // On lance le ray !
        Ray ray = new Ray(player.transform.position, player.camera.transform.forward);
        RaycastHit[] hits = null;
        if (bDestroyAllCubes) {
            hits = Physics.RaycastAll(ray, porteeRay);
        } else {
            hits = new RaycastHit[1];
            Physics.Raycast(ray, out hits[0], porteeRay);
        }

        // Puis on détruit tous les cubes touchés ! :D
        bool atLeastOneCubeDestroyed = false;
        foreach(RaycastHit hit in hits) {
            Cube cube = hit.collider.gameObject.GetComponent<Cube>();
            if(cube != null && cube.IsDestructible()) {
                cube.Explode();
                atLeastOneCubeDestroyed = true;
            }
        }

        return atLeastOneCubeDestroyed;
    }
}