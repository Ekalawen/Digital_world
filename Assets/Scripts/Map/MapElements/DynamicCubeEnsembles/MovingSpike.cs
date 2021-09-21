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
    protected float delay;
    protected Coroutine coroutine = null;
    protected bool shouldDisplayPrevisualization;

    public void Initialize(Vector3 start, Vector3 direction, float delay, bool shouldDisplayPrevisualization)
    {
        base.Initialize();
        this.start = start;
        this.direction = direction;
        this.end = ComputeEnd();
        this.delay = delay;
        this.shouldDisplayPrevisualization = shouldDisplayPrevisualization;
        if (CanStartAtThisPosition(start)) {
            coroutine = StartCoroutine(CStartSpike());
        } else {
            DestroyCubeEnsemble();
        }
    }

    public bool CanStartAtThisPosition(Vector3 position) {
        return map.IsInRegularMap(position) && !map.IsCubeAt(position);
    }

    protected Vector3 ComputeEnd() {
        Vector3 current = start;
        int tailleMap = map.GetTailleMapAlongDirection(GravityManager.VecToDir(direction));
        for(int i = 0; i < tailleMap; i++) {
            if(map.IsCubeAt(current) || !map.IsInRegularMap(current)) {
                return current;
            }
            current += direction;
        }
        return current;
    }

    protected IEnumerator CStartSpike() {
        yield return new WaitForSeconds(delay);

        //if (shouldDisplayPrevisualization) {
        //    Lightning lightning = Instantiate(previsualizationLightningPrefab).GetComponent<Lightning>();
        //    lightning.Initialize(start, end, Lightning.PivotType.EXTREMITY);
        //}

        yield return new WaitForSeconds(previsualizationTime);

        Cube cube = CreateCube(start);
        cube.SetDissolveTime(dissolveTime);

        yield return new WaitForSeconds(dissolveTime);

        float distance = Vector3.Distance(start, end);
        float time = distance / speed;
        Timer timer = new Timer(time);
        while(!timer.IsOver()) {
            float avancement = Math.Min(timer.GetAvancement(), 1);
            Vector3 newPosition = Vector3.Lerp(start, end, avancement);
            bool moveSucceed = cube.MoveTo(newPosition);
            if(!moveSucceed) {
                //end = MathTools.Round(newPosition);
                //cube.MoveTo(end);
                break;
            }
            yield return null;
        }

        cube.Decompose(decomposeTime);
        yield return new WaitForSeconds(decomposeTime);

        DestroyCubeEnsemble();
    }

    public override string GetName() {
        return "MovingSpike";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = DynamicCubeEnsembleType.MOVING_SPIKE;
    }

    public void Stop() {
        if(coroutine != null) {
            StopCoroutine(coroutine);
        }
    }
}

