using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlockBehavior : MonoBehaviour {

    protected GameManager gm;
    protected FlockManager flockManager;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        flockManager = gm.flockManager;
    }

    public abstract Vector3 CalculateMove(IController flockController);

    public Vector3 InterpolateWithForward(Vector3 vector, IController flockController) {
        // On ajoute un poids au vecteur pour qu'il l'emporte sur le mouvement passif et forcer le contact !
        return (vector * 2 + flockController.transform.forward * vector.magnitude) / 3;
    }
}
