using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunAwayPlayerController : IController {

    public enum Etat { WANDERING, FLEEING };

    public float distanceToFleePlayer = 5.0f; // La distance à laquelle on commence à fuir !
    public float wanderingVitesse; // vitesse de déplacement de base
	public float fleeingVitesse; // vitesse de lorsque l'on doit fuir !
    public float malusVerticalMouvement = 0f; // On peut faire en sorte de moins utiliser les hauteurs
    //public GameObject trailPrefab;

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

        // Tant que le joueur n'est pas trop proche, on erre
        switch (etat) {
            case Etat.WANDERING:
                // On cherche un point où aller
                if(Vector3.Distance(posObjectifWandering, transform.position) <= 0.1f) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifWandering = GetOtherObjectifWandering();
                }

                //GameObject tr = Instantiate(trailPrefab, transform.position, Quaternion.identity) as GameObject;
                //tr.GetComponent<Trail>().SetTarget(posObjectifWandering);

                // Puis on y va
                MoveToTarget(posObjectifWandering, useCustomVitesse: true, wanderingVitesse);
                //Vector3 direction = (posObjectifWandering - transform.position).normalized;
                //Vector3 finalMouvement = direction * wanderingVitesse * Time.deltaTime;
                //controller.Move(finalMouvement);
                break;

            case Etat.FLEEING:
                // On cherche à aller le plus loin possible et le plus vite possible d'une zone d'attraction du joueur
                if(Vector3.Distance(posObjectifFleeing, transform.position) <= 0.1f
                || IsInPlayerAttractionZone()) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifFleeing = GetOtherObjectifFleeing();
                }

                //GameObject trf = Instantiate(trailPrefab, transform.position, Quaternion.identity) as GameObject;
                //trf.GetComponent<Trail>().SetTarget(posObjectifFleeing);

                // Puis on y va
                MoveToTarget(posObjectifFleeing, useCustomVitesse: true, fleeingVitesse);
                //Vector3 directionVisible = (posObjectifFleeing - transform.position).normalized;
                //Vector3 finalMouvementVisible = directionVisible * fleeingVitesse * Time.deltaTime;
                //controller.Move(finalMouvementVisible);
                break;
        }
    }

    protected bool IsInPlayerAttractionZone() {
        return Vector3.Distance(transform.position, player.transform.position) <= distanceToFleePlayer;
    }

    protected Etat UpdateEtat() {
        if(IsInPlayerAttractionZone()) {
            if (etat != Etat.FLEEING) {
                posObjectifFleeing = transform.position;
            }
            etat = Etat.FLEEING;
            return Etat.FLEEING;
        } else {
            if (etat != Etat.WANDERING) {
                posObjectifWandering = transform.position;
            }
            etat = Etat.WANDERING;
            return Etat.WANDERING;
        }
    }

    protected Vector3 GetOtherObjectifFleeing() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();

        // On ne conserve que celles qui sont plus loin que la distance d'attraction !
        List<Vector3> allFarAwayPositions = new List<Vector3>();
        foreach(Vector3 pos in allEmptyLocations) {
            if (Vector3.Distance(player.transform.position, pos) >= distanceToFleePlayer)
                allFarAwayPositions.Add(pos);
        }
        if(allFarAwayPositions.Count == 0) {
            allFarAwayPositions = allEmptyLocations;
        }

        Vector3 bestPos = allFarAwayPositions[0];
        float minDist = Vector3.Distance(bestPos, transform.position);
        foreach (Vector3 pos in allFarAwayPositions) {
            Vector3 direction = pos - transform.position;
            float dist = direction.magnitude;
            float verticalMalus = Mathf.Abs(gm.gravityManager.GetHeightInMap(pos) - gm.gravityManager.GetHeightInMap(transform.position)) * malusVerticalMouvement;
            if (dist + verticalMalus < minDist) {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, direction);
                if (!Physics.Raycast(ray, out hit, dist)) {
                    minDist = dist;
                    bestPos = pos;
                }
            }
        }
        return bestPos;
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
