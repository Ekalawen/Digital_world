using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpike : DynamicCubeEnsemble {

    [Header("Previsualization")]
    public float previsualizationTime = 1.5f;
    public GameObject previsualizationLightningPrefab;

    [Header("Effect")]
    public float speed = 10.0f;
    public float dissolveTime = 0.1f;

    [Header("End Effect")]
    public float decomposeTime = 0.1f;

    protected Vector3 start;
    protected Vector3 direction;
    protected Vector3 end;
    protected float previsualizationDelay;
    protected Coroutine coroutinePrevisualization = null;
    protected Coroutine coroutineSpike = null;
    protected bool shouldDisplayPrevisualization;
    protected bool useBoundingBox;

    public void Initialize(Vector3 start, Vector3 direction, float previsualizationDelay, bool shouldDisplayPrevisualization, bool useBoundingBox)
    {
        base.Initialize();
        this.start = start;
        this.direction = direction;
        this.useBoundingBox = useBoundingBox;
        this.end = ComputeEnd();
        this.previsualizationDelay = previsualizationDelay;
        this.shouldDisplayPrevisualization = shouldDisplayPrevisualization;
        if (CanStartAtThisPosition(start)) {
            coroutinePrevisualization = StartCoroutine(CStartPrevisualization());
            coroutineSpike = StartCoroutine(CStartSpike());
        } else {
            DestroyCubeEnsemble();
        }
    }

    public bool CanStartAtThisPosition(Vector3 position) {
        return !map.IsCubeAt(position);
    }

    protected Vector3 ComputeEnd() {
        Vector3 current = start;
        GravityManager.Direction gravityDir = GravityManager.VecToDir(direction);
        int tailleMap = useBoundingBox ? map.GetBoundingBoxSizeAlongDirection(gravityDir) : map.GetTailleMapAlongDirection(gravityDir);
        for(int i = 0; i < tailleMap; i++) {
            bool isInMap = useBoundingBox ? map.IsInsideBoundingBox(current) : map.IsInRegularMap(current);
            if(map.IsCubeAt(current) || !isInMap) {
                return current;
            }
            current += direction;
        }
        return current;
    }

    protected IEnumerator CStartPrevisualization() {
        yield return new WaitForSeconds(previsualizationDelay);

        if (shouldDisplayPrevisualization) {
            Lightning lightning = Instantiate(previsualizationLightningPrefab).GetComponent<Lightning>();
            lightning.Initialize(start, end, Lightning.PivotType.EXTREMITY);
        }
    }

    protected IEnumerator CStartSpike() {
        yield return new WaitForSeconds(previsualizationTime);

        Cube cube = CreateCube(start);
        cube.StartDissolveEffect(dissolveTime);

        yield return new WaitForSeconds(dissolveTime);

        float distance = Vector3.Distance(start, end);
        float time = distance / speed;
        Timer timer = new Timer(time);
        while(!timer.IsOver()) {
            float avancement = Math.Min(timer.GetAvancement(), 1);
            Vector3 newPosition = Vector3.Lerp(start, end, avancement);
            bool moveSucceed = cube.MoveTo(newPosition);
            if (!moveSucceed) {
                //end = MathTools.Round(newPosition);
                //cube.MoveTo(end);
                break;
            }
            yield return null;
        }

        cube.Decompose(decomposeTime);
        yield return new WaitForSeconds(decomposeTime + 1); // +1 For security :)

        DestroyCubeEnsemble();
    }

    public override string GetName() {
        return "MovingSpike";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = DynamicCubeEnsembleType.MOVING_SPIKE;
    }

    public void Stop() {
        if(coroutinePrevisualization != null) {
            StopCoroutine(coroutinePrevisualization);
        }
        if(coroutineSpike != null) {
            StopCoroutine(coroutineSpike);
        }
    }
}

