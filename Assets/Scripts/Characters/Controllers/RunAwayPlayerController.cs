using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunAwayPlayerController : IController {

    public enum Etat { WANDERING, FLEEING };

    public float distanceToFleePlayer = 5.0f; // La distance à laquelle on commence à fuir !
    public float wanderingVitesse; // vitesse de déplacement de base
	public float fleeingVitesse; // vitesse de lorsque l'on doit fuir !
    public float malusVerticalMouvement = 0f; // On peut faire en sorte de moins utiliser les hauteurs : les cases sont malusVerticalMouvement * hauteurMouvement plus loins !
    public float minDistanceToObjectives = 0.1f;

	protected Player player;
    protected Etat etat;
    protected Vector3 posObjectifWandering;
    protected Vector3 posObjectifFleeing;

    public override void Start() {
        base.Start();
        gm = GameManager.Instance;
        player = gm.player;
        posObjectifWandering = transform.position;
        posObjectifFleeing = transform.position;
    }

    protected override void UpdateSpecific() {
        UpdateEtat();

        switch (etat) {
            case Etat.WANDERING:
                // On cherche un point où aller
                if(Vector3.Distance(posObjectifWandering, transform.position) <= minDistanceToObjectives
                || !HasMoveSinceLastFrame()) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifWandering = GetOtherObjectifWandering();
                }

                MoveToTarget(posObjectifWandering, useCustomVitesse: true, wanderingVitesse);
                break;

            case Etat.FLEEING:
                // On cherche à aller le plus loin possible et le plus vite possible d'une zone d'attraction du joueur
                if(Vector3.Distance(posObjectifFleeing, transform.position) <= minDistanceToObjectives
                || !HasMoveSinceLastFrame()) {
                //|| IsInPlayerAttractionZone()) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifFleeing = GetOtherObjectifFleeing();
                }

                MoveToTarget(posObjectifFleeing, useCustomVitesse: true, fleeingVitesse);
                break;
        }
    }

    protected bool IsInPlayerAttractionZone() {
        return Vector3.Distance(transform.position, player.transform.position) <= distanceToFleePlayer;
    }

    protected Etat UpdateEtat() {
        if(IsInPlayerAttractionZone()) {
            if (etat != Etat.FLEEING) {
                posObjectifFleeing = GetOtherObjectifFleeing();
            }
            etat = Etat.FLEEING;
            return Etat.FLEEING;
        } else {
            if (etat != Etat.WANDERING) {
                posObjectifWandering = GetOtherObjectifWandering();
            }
            etat = Etat.WANDERING;
            return Etat.WANDERING;
        }
    }

    protected Vector3 GetOtherObjectifFleeing() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();

        // On ne conserve que celles qui sont plus loin que la distance d'attraction !
        List<Vector3> allFarAwayPositions = allEmptyLocations.FindAll(p => Vector3.Distance(player.transform.position, p) >= distanceToFleePlayer);
        if(allFarAwayPositions.Count == 0) {
            allFarAwayPositions = allEmptyLocations;
        }

        Vector3 bestPos = allFarAwayPositions[0];
        float minWeight = ComputeWeightForPosition(bestPos);
        foreach (Vector3 pos in allFarAwayPositions) {
            Vector3 direction = pos - transform.position;
            float dist = direction.magnitude;
            float weight = ComputeWeightForPosition(pos);
            if (weight < minWeight) {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, direction);
                if (!Physics.Raycast(ray, out hit, dist)) {
                    minWeight = dist;
                    bestPos = pos;
                }
            }
        }
        return bestPos;
    }

    public float ComputeWeightForPosition(Vector3 position) {
        float dist = Vector3.Distance(position, transform.position);
        float verticalMalus = Mathf.Abs(gm.gravityManager.GetHeightInMap(position) - gm.gravityManager.GetHeightInMap(transform.position)) * malusVerticalMouvement;
        return dist + verticalMalus;
    }

    protected Vector3 GetOtherObjectifWandering() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();
        while (allEmptyLocations.Count > 1) {
            int ind = Random.Range(0, allEmptyLocations.Count);
            RaycastHit hit;
            Vector3 direction = allEmptyLocations[ind] - transform.position;
            Ray ray = new Ray(transform.position, direction);
            if (!Physics.Raycast(ray, out hit, direction.magnitude)) // Si c'est une position accessible directement !
                return allEmptyLocations[ind];
            allEmptyLocations.RemoveAt(ind);
        }
        return allEmptyLocations[0];
    }

    public override bool IsInactive() {
        return false;
    }

    public override bool IsMoving() {
        return true;
    }
}
