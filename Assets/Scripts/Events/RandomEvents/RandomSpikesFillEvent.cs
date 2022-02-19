using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomSpikesFillEvent : RandomEventFrequence {

    public GameObject movingSpikePrefab;
    public int sizeSpikes = 1;
    public float minDistanceFromPlayer = 5.0f;
    public bool useBoundingBox = true;
    public bool useGeoData = false;
    [ConditionalHide("useGeoData")]
    public GeoData geoData;

    protected MapManager map;
    protected List<MovingSpike> movingSpikes;
    protected float creationDuration;
    protected List<Vector3> targetPositions;

    public override void Initialize() {
        base.Initialize();
        map = gm.map;
        movingSpikes = new List<MovingSpike>();
        map.onDeleteCube.AddListener(AddEmptyPositionsOnDeleteCube);
        targetPositions = map.GetAllEmptyPositions();
        MathTools.Shuffle(targetPositions);
    }

    protected override void StartEvent() {
        Vector3 target = targetPositions.First();
        List<Tuple<Vector3, float>> startingPositionsForDirections = ComputeStartingPositionsForTarget(target);
        startingPositionsForDirections = startingPositionsForDirections.FindAll(t => Vector3.Distance(t.Item1, gm.player.transform.position) >= minDistanceFromPlayer);
        if(startingPositionsForDirections.Count == 0) {
            RestartEvent(target);
            return;
        }
        Tuple<Vector3, float> chosenOne = startingPositionsForDirections.OrderBy(t => t.Item2).Last();
        Vector3 startingPosition = chosenOne.Item1;
        Vector3 direction = (target - startingPosition).normalized;

        if (direction != Vector3.zero) {
            StartSpikesAtPosition(startingPosition, direction);
        } else {
            RestartEvent(target);
        }
    }

    protected void RestartEvent(Vector3 target) {
        MarkTargetPositionsAsCovered(new List<Vector3>() { target });
        StartEvent();
    }

    protected void StartSpikesAtPosition(Vector3 startingPosition, Vector3 direction) {
        Vector3 dir2 = (direction == Vector3.up || direction == Vector3.down) ? Vector3.forward : Vector3.up;
        Vector3 dir3 = Vector3.Cross(direction, dir2);
        float startOnDir1 = Vector3.Dot(startingPosition, direction);
        float startOnDir2 = Vector3.Dot(startingPosition, dir2);
        float startOnDir3 = Vector3.Dot(startingPosition, dir3);
        for(float i = startOnDir2 - Mathf.FloorToInt(sizeSpikes / 2.0f); i < startOnDir2 + Mathf.CeilToInt(sizeSpikes / 2.0f); i++) {
            for(float j = startOnDir3 - Mathf.FloorToInt(sizeSpikes / 2.0f); j < startOnDir3 + Mathf.CeilToInt(sizeSpikes / 2.0f); j++) {
                Vector3 pos = direction * startOnDir1 + i * dir2 + j * dir3;
                StartOneSpikeAtPosition(pos, direction);
            }
        }
        AddGeoPoint(startingPosition);
    }

    protected void StartOneSpikeAtPosition(Vector3 startingPosition, Vector3 direction) {
        MovingSpike movingSpike = Instantiate(movingSpikePrefab, parent: transform).GetComponent<MovingSpike>();
        movingSpike.Initialize(startingPosition, direction,
            previsualizationDelay: 0.0f,
            shouldDisplayPrevisualization: true,
            useBoundingBox: false);
        movingSpikes.Add(movingSpike);
        MarkTargetPositionsAsCovered(movingSpike.GetPositionsCovered());
    }

    protected List<Tuple<Vector3, float>> ComputeStartingPositionsForTarget(Vector3 target) {
        List<Vector3> directions = MathTools.GetAllOrthogonalNormals();
        List<Vector3> startingPositions = directions.Select(d => ComputeStartingPositionForTargetAndDirection(target, d)).ToList();
        return startingPositions.Select(sp => new Tuple<Vector3, float>(sp, Vector3.Distance(sp, target))).ToList();
    }

    protected Vector3 ComputeStartingPositionForTargetAndDirection(Vector3 target, Vector3 direction) {
        Vector3 current = target;
        GravityManager.Direction gravityDir = GravityManager.VecToDir(direction);
        int tailleMap = useBoundingBox ? map.GetBoundingBoxSizeAlongDirection(gravityDir) : map.GetTailleMapAlongDirection(gravityDir);
        for(int i = 0; i < tailleMap; i++) {
            Vector3 nextCurrent = current + direction;
            bool isInMap = useBoundingBox ? map.IsInsideBoundingBox(nextCurrent) : map.IsInRegularMap(nextCurrent);
            if(map.IsCubeAt(nextCurrent) || !isInMap) {
                return current;
            }
            current = nextCurrent;
        }
        return current;
    }

    protected void MarkTargetPositionsAsCovered(List<Vector3> positionsToMark) {
        foreach(Vector3 position in positionsToMark) {
            targetPositions.Remove(position);
        }
        targetPositions.AddRange(positionsToMark);
    }

    protected void AddEmptyPositionsOnDeleteCube(Cube deletedCube) {
        if (deletedCube.transform.parent.name != MovingSpike.MOVING_SPIKE_FOLDER_NAME) {
            targetPositions.Add(deletedCube.transform.position);
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

    protected void AddGeoPoint(Vector3 startingPosition) {
        if (!useGeoData) {
            return;
        }

        GeoData newGeoData = new GeoData(geoData);
        newGeoData.SetTargetPosition(startingPosition);
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }
}

