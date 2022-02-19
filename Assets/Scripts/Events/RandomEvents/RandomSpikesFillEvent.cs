using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomSpikesFillEvent : RandomEventFrequence {

    public GameObject movingSpikePrefab;
    public bool useBoundingBox;
    public GeoData geoData;

    protected MapManager map;
    protected List<MovingSpike> movingSpikes;
    protected float creationDuration;
    protected List<Vector3> targetPositions;

    public override void Initialize() {
        base.Initialize();
        map = gm.map;
        movingSpikes = new List<MovingSpike>();
        targetPositions = map.GetAllEmptyPositions();
        MathTools.Shuffle(targetPositions);
    }

    protected override void StartEvent() {
        Vector3 target = targetPositions.First();
        List<Tuple<Vector3, float>> startingPositionsForDirections = ComputeStartingPositionsForTarget(target);
        Tuple<Vector3, float> chosenOne = startingPositionsForDirections.OrderBy(t => t.Item2).Last();
        Vector3 startingPosition = chosenOne.Item1;
        float distance = chosenOne.Item2;
        Vector3 direction = (target - startingPosition).normalized;

        if (direction != Vector3.zero) {
            MovingSpike movingSpike = Instantiate(movingSpikePrefab, parent: transform).GetComponent<MovingSpike>();
            movingSpike.Initialize(startingPosition, direction,
                previsualizationDelay: 0.0f,
                shouldDisplayPrevisualization: true,
                useBoundingBox: false);
            movingSpikes.Add(movingSpike);
            MarkTargetPositionsAsCovered(movingSpike.GetPositionsCovered());
            AddGeoPoint(startingPosition);
        } else {
            MarkTargetPositionsAsCovered(new List<Vector3>() { target });
        }
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
            if(map.IsCubeAt(nextCurrent) || !isInMap || map.IsLumiereAt(nextCurrent) || gm.itemManager.IsItemAt(nextCurrent)) {
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

    protected void AddEmptyPositionsOnDeleteCube() {
        // TODO ! ==> Add listener !
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
        // TODO !
    }
}

