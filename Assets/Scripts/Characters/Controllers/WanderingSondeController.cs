using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingSondeController : SondeController {

    public float wanderingSpeedCoef = 0.5f;
    public bool throwTrailOnWandering = true;
    [ConditionalHide("throwTrailOnWandering")]
    public GameObject trailDePrevisualisation;

    protected bool shouldGoToPlayerLastPosition = false;
    protected Vector3 wanderingDestination;
    protected Timer timerBeforeNextDestination;

    public override void Start() {
        base.Start();
        timerBeforeNextDestination = new Timer(0.1f);
        etat = EtatSonde.WANDERING;
        wanderingDestination = GetNextDestination();
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
            SetTracking();
        } else {
            if (shouldGoToPlayerLastPosition) {
                SetWaiting();
            } else {
                SetWandering(previousEtat);
            }
        }
    }

    protected override void SetTracking() {
        base.SetTracking();
        shouldGoToPlayerLastPosition = true;
    }

    protected void SetWandering(EtatSonde previousEtat) {
        etat = EtatSonde.WANDERING;
        // Si la sonde se met à errer, on trace un trail pour que ça se voit !
        if (etat != previousEtat) {
            FindNextDestination();
        }
        Sonde sonde = GetComponent<Sonde>();
        if(sonde != null) {
            sonde.ActivateWaves(true);
        }
    }

    protected override void Wait() {
        base.Wait();
        if (transform.position == lastPositionSeen) {
            shouldGoToPlayerLastPosition = false;
        }
    }

    protected void ThrowRayToDestination() {
        if (throwTrailOnWandering) {
            GameObject tr = Instantiate(trailDePrevisualisation, transform.position, Quaternion.identity) as GameObject;
            tr.GetComponent<Trail>().SetTarget(wanderingDestination);
        }
    }

    private void Wander() {
        float currentVitesse = vitesse * wanderingSpeedCoef;
        Vector3 move = MoveToTarget(wanderingDestination, useCustomVitesse: true, customVitesse: currentVitesse);

        // Si le mouvement est trop petit ou que l'on arrive plus à bouger, c'est que l'on est arrivé
        if (timerBeforeNextDestination.IsOver()) {
            if ((Vector3.Magnitude(move) <= 0.001f && Vector3.Magnitude(move) != 0f)
             || (Vector3.Distance(transform.position, lastPosition) <= 0.001f)) {
                 FindNextDestination();
            }
        }
    }

    protected void FindNextDestination() {
        timerBeforeNextDestination.Reset();

        wanderingDestination = GetNextDestination();

        if (!gm.ennemiManager.IsPlayerFollowed()) {
            ThrowRayToDestination();
        }
    }

    private Vector3 GetNextDestination() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();
        while (allEmptyLocations.Count > 1) {
            int ind = UnityEngine.Random.Range(0, allEmptyLocations.Count);
            RaycastHit hit;
            Vector3 direction = allEmptyLocations[ind] - transform.position;
            Ray ray = new Ray(transform.position, direction);
            if (!Physics.Raycast(ray, out hit, direction.magnitude)) {// Si c'est une position accessible directement !
                return allEmptyLocations[ind];
            }
            allEmptyLocations.RemoveAt(ind);
        }
        return allEmptyLocations[0];
    }

    public override bool IsInactive() {
        return etat == EtatSonde.WANDERING;
    }
}
