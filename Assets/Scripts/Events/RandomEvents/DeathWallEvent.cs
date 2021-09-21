using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWallEvent : RandomEvent {

    public GameObject movingSpikePrefab;
    public float delayCoef = 0.02f;
    public float delayRandPercentage = 0.01f;

    protected MapManager map;
    protected List<MovingSpike> movingSpikes;

    public override void Start() {
        base.Start();
        map = gm.map;
        movingSpikes = new List<MovingSpike>();
        this.esperanceDuree = ComputeEsperanceDuree();
    }

    protected override void StartEvent() {
        Vector3 mainDirection = MathTools.ChoseOne(MathTools.GetAllOrthogonalNormals());
        List<Vector3> startingPositions = GetAllStartingPositions(mainDirection);
        Vector3 leadingPosition = MathTools.ChoseOne(startingPositions);
        foreach (Vector3 startingPosition in startingPositions) {
            MovingSpike movingSpike = Instantiate(movingSpikePrefab, parent: transform).GetComponent<MovingSpike>();
            float previsualizationDelay = Vector3.Distance(leadingPosition, startingPosition) * delayCoef;
            previsualizationDelay = Mathf.Max(0, MathTools.RandArround(previsualizationDelay, delayRandPercentage));
            movingSpike.Initialize(startingPosition, mainDirection, previsualizationDelay, shouldDisplayPrevisualization: true);
            movingSpikes.Add(movingSpike);
        }
    }

    protected List<Vector3> GetAllStartingPositions(Vector3 direction) {
        if(direction == Vector3.down) {
            return map.GetAllPositionsOnBorduresHaut();
        } else if (direction == Vector3.up) {
            return map.GetAllPositionsOnBorduresBas();
        } else if (direction == Vector3.left) {
            return map.GetAllPositionsOnBorduresDroite();
        } else if (direction == Vector3.right) {
            return map.GetAllPositionsOnBorduresGauche();
        } else if (direction == Vector3.forward) {
            return map.GetAllPositionsOnBorduresArriere();
        } else if (direction == Vector3.back) {
            return map.GetAllPositionsOnBorduresAvant();
        } else {
            throw new Exception($"GetAllStartingPositions doit recevoir un vecteur orthogonal normalisé ! Pas {direction} ! :p");
        }
    }

    protected override void EndEvent() {
        movingSpikes.Clear();
    }

    public override bool CanBeStarted() {
        return true;
    }

    protected override void StartEventConsoleMessage() {
    }

    public override void StopEvent() {
        foreach(MovingSpike movingSpike in movingSpikes) {
            if (movingSpike != null) {
                movingSpike.Stop();
            }
        }
        movingSpikes.Clear();
    }

    protected float ComputeEsperanceDuree() {
        MovingSpike movingSpike = movingSpikePrefab.GetComponent<MovingSpike>();
        float maxTailleMap = Mathf.Max(map.tailleMap.x, map.tailleMap.y, map.tailleMap.z);
        float movingTime = maxTailleMap / movingSpike.speed;
        return movingSpike.dissolveTime + movingSpike.previsualizationTime + movingSpike.decomposeTime + movingTime;
    }
}

