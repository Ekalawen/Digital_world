using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingSondeController : SondeController {

    public float wanderingSpeedCoef = 0.5f;
    public GameObject trailDePrevisualisation;

    protected bool shouldGoToPlayerLastPosition = false;
    protected Vector3 wanderingDestination;
    protected Timer timerBeforeNextDestination;

    public override void Start() {
        base.Start();
        timerBeforeNextDestination = new Timer(0.1f);
        etat = EtatSonde.WANDERING;
    }

    protected override void UpdateSpecific() {
        GetEtat();

        // Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
        // Sinon on le pourchasse !
        switch (etat) {
            case EtatSonde.WAITING:
                Wait();
                break;
            case EtatSonde.TRACKING:
                Track();
                break;
            case EtatSonde.WANDERING:
                Wander();
                break;
        }
        lastPosition = transform.position;
    }

    protected override void GetEtat() {
        EtatSonde previousEtat = etat;
        if(IsPlayerVisible()) {
            etat = EtatSonde.TRACKING;
            shouldGoToPlayerLastPosition = true;
            // Si la sonde vient juste de le repérer, on l'annonce
            if (previousEtat == EtatSonde.WANDERING) {
                gm.soundManager.PlayDetectionClip(transform.position, transform);
            }
        } else {
            if (shouldGoToPlayerLastPosition) {
                etat = EtatSonde.WAITING;
                // Si la sonde vient juste de perdre sa trace, on l'annonce
                if (etat != previousEtat)
                    gm.console.JoueurPerduDeVue(name);
            } else {
                etat = EtatSonde.WANDERING;
                // Si la sonde se met à errer, on trace un trail pour que ça se voit !
                if (etat != previousEtat) {
                    FindNextDestination();
                }
            }
        }
    }

    protected override void Wait() {
        base.Wait();
        if (transform.position == lastPositionSeen)
            shouldGoToPlayerLastPosition = false;
    }

    protected void ThrowRayToDestination() {
        GameObject tr = Instantiate(trailDePrevisualisation, transform.position, Quaternion.identity) as GameObject;
        tr.GetComponent<Trail>().SetTarget(wanderingDestination);
    }

    private void Wander() {
        Vector3 move = Move(wanderingDestination);

        // Si le mouvement est trop petit ou que l'on arrive plus à bouger, c'est que l'on est arrivé
        if (timerBeforeNextDestination.IsOver()) {
            if ((Vector3.Magnitude(move) <= 0.001f && Vector3.Magnitude(move) != 0f)
             || (Vector3.Distance(transform.position, lastPosition) <= 0.001f))
            {
                FindNextDestination();
            }
        }
    }

    protected void FindNextDestination() {
        timerBeforeNextDestination.Reset();

        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();
        bool founded = false;
        while (allEmptyLocations.Count > 1) {
            int ind = UnityEngine.Random.Range(0, allEmptyLocations.Count);
            RaycastHit hit;
            Vector3 direction = allEmptyLocations[ind] - transform.position;
            Ray ray = new Ray(transform.position, direction);
            if (!Physics.Raycast(ray, out hit, direction.magnitude)) {// Si c'est une position accessible directement !
                wanderingDestination = allEmptyLocations[ind];
                founded = true;
                break;
            }
            allEmptyLocations.RemoveAt(ind);
        }
        if(!founded)
            wanderingDestination = allEmptyLocations[0];

        //wanderingDestination = gm.map.GetFreeRoundedLocation();

        //// On va jusque là où l'on peut ! :D
        //RaycastHit hit;
        //Ray ray = new Ray(transform.position, wanderingDestination - transform.position);
        //bool hited = Physics.Raycast(ray, out hit);
        //if (hited) {
        //    Vector3 point = hit.point;
        //    if (Vector3.Distance(transform.position, point) <= Vector3.Distance(transform.position, wanderingDestination))
        //        wanderingDestination = point;
        //}

        //// Pour éviter d'être bloqué si on est sur un coin de la map ! :)
        //if (Vector3.Distance(wanderingDestination, transform.position) <= 1.0f) {
        //    Vector3 direction = UnityEngine.Random.insideUnitSphere;
        //    ray = new Ray(transform.position, direction);
        //    if (Physics.Raycast(ray, out hit)) {
        //        wanderingDestination = hit.point;
        //    } else {
        //        wanderingDestination = transform.position + direction * 3.0f;
        //    }
        //}

        if (!gm.ennemiManager.IsPlayerFollowed()) {
            ThrowRayToDestination();
        }
    }

    public override bool IsInactive() {
        return etat == EtatSonde.WANDERING;
    }
}
