using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnnemiController : IController {

	public float distanceDeDetection = 10.0f; // La distance à partir de laquelle l'ennemi peut pourchasser l'ennemi
    public bool canAlwaysSeePlayer = false; // Permet de toujours voir le player et donc de toujours aller vers lui !

    protected Player player;

    public override void Start() {
        base.Start();
        player = gm.player;
    }

    // Permet de savoir si l'ennemi voit le joueur
    public virtual bool IsPlayerVisible() {
        if (canAlwaysSeePlayer)
            return true;

        // Si l'ennemie est suffisament proche et qu'il est visible !
        RaycastHit hit;
        Ray ray = new Ray (transform.position, player.transform.position - transform.position);
        return Physics.Raycast(ray, out hit, distanceDeDetection) && hit.collider.name == "Joueur";
    }
}
