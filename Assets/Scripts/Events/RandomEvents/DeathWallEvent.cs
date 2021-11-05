using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathWallEvent : RandomEvent {

    public enum PositionsMode { MAP_SIZE, BOUNDING_BOX, BOUNDING_BOX_ALONG_AXIS };

    public GameObject movingSpikePrefab;
    public float delayCoef = 0.02f;
    public float delayRandPercentage = 0.01f;
    public PositionsMode positionMode = PositionsMode.MAP_SIZE;

    protected MapManager map;
    protected List<MovingSpike> movingSpikes;

    public override void Initialize() {
        base.Initialize();
        map = gm.map;
        movingSpikes = new List<MovingSpike>();
        this.esperanceDuree = ComputeEsperanceDuree();
    }

    protected override void StartEvent() {
        Vector3 mainDirection = MathTools.ChoseOne(MathTools.GetAllOrthogonalNormals());
        List<Vector3> startingPositions = GetAllStartingPositions(mainDirection);
        Vector3 leadingPosition = MathTools.ChoseOne(startingPositions);
        bool useBoundingBox = positionMode != PositionsMode.MAP_SIZE;
        foreach (Vector3 startingPosition in startingPositions) {
            MovingSpike movingSpike = Instantiate(movingSpikePrefab, parent: transform).GetComponent<MovingSpike>();
            float previsualizationDelay = Vector3.Distance(leadingPosition, startingPosition) * delayCoef;
            previsualizationDelay = Mathf.Max(0, MathTools.RandArround(previsualizationDelay, delayRandPercentage));
            movingSpike.Initialize(startingPosition, mainDirection, previsualizationDelay, shouldDisplayPrevisualization: true, useBoundingBox);
            movingSpikes.Add(movingSpike);
        }
    }

    protected List<Vector3> GetAllStartingPositions(Vector3 direction) {
        if(direction == Vector3.down) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresHaut();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxHaut();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresHaut();
                    float maxBoundingBoxValue = map.GetBoundingBox().yMax;
                    return positions.Select(p => new Vector3(p.x, maxBoundingBoxValue, p.z)).ToList();
                default: return null;
            }
        }
        else if (direction == Vector3.up) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresBas();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxBas();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresBas();
                    float minBoundingBoxValue = map.GetBoundingBox().yMin;
                    return positions.Select(p => new Vector3(p.x, minBoundingBoxValue, p.z)).ToList();
                default: return null;
            }
        } else if (direction == Vector3.left) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresDroite();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxDroite();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresDroite();
                    float maxBoundingBoxValue = map.GetBoundingBox().xMax;
                    return positions.Select(p => new Vector3(maxBoundingBoxValue, p.y, p.z)).ToList();
                default: return null;
            }
        } else if (direction == Vector3.right) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresGauche();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxGauche();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresGauche();
                    float minBoundingBoxValue = map.GetBoundingBox().xMin;
                    return positions.Select(p => new Vector3(minBoundingBoxValue, p.y, p.z)).ToList();
                default: return null;
            }
        } else if (direction == Vector3.forward) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresArriere();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxArriere();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresArriere();
                    float minBoundingBoxValue = map.GetBoundingBox().zMin;
                    return positions.Select(p => new Vector3(p.x, p.y, minBoundingBoxValue)).ToList();
                default: return null;
            }
        } else if (direction == Vector3.back) {
            switch (positionMode) {
                case PositionsMode.MAP_SIZE: return map.GetAllPositionsOnBorduresAvant();
                case PositionsMode.BOUNDING_BOX: return map.GetAllPositionsOnBoundingBoxAvant();
                case PositionsMode.BOUNDING_BOX_ALONG_AXIS:
                    List<Vector3> positions = map.GetAllPositionsOnBorduresAvant();
                    float maxBoundingBoxValue = map.GetBoundingBox().zMax;
                    return positions.Select(p => new Vector3(p.x, p.y, maxBoundingBoxValue)).ToList();
                default: return null;
            }
        } else {
            throw new Exception($"GetAllStartingPositions doit recevoir un vecteur orthogonal normalisé ! Pas {direction} ! :p");
        }
    }

    protected override void EndEvent() {
        movingSpikes.Clear();
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
        BoundingBox boundingBox = map.GetBoundingBox();
        float maxTailleMap = positionMode != PositionsMode.MAP_SIZE ?
            Mathf.Max(boundingBox.width, boundingBox.height, boundingBox.depth) : Mathf.Max(map.tailleMap.x, map.tailleMap.y, map.tailleMap.z);
        float movingTime = maxTailleMap / movingSpike.speed;
        return movingSpike.previsualizationTime + movingSpike.dissolveTime + movingTime + movingSpike.decomposeTime;
    }
}

