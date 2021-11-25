using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class IGenerator : MonoBehaviour {

    public enum ChoseType { GET_CUBES, GET_EMPTY };

    [Header("Parameters")]
    public float frequenceActivation = 0.25f;
    public bool firstActivationRandom = false;
    public float activationRange = 7.0f;
    public float frequenceRecomputePositions = 5.0f;
    public ChoseType choseType;
    [ConditionalHide("choseType", ChoseType.GET_CUBES)]
    public GetCubesHelper getCubesHelper;
    [ConditionalHide("choseType", ChoseType.GET_EMPTY)]
    public GetEmptyPositionsHelper getEmptyPositionsHelper;
    public bool randomizePositions = false;

    [Header("Lightning")]
    public GameObject lightningPrefab;

    [Header("Links")]
    public OrbTrigger orbTrigger;

    protected GameManager gm;
    protected MapManager map;
    protected Stack<Vector3> precomputedPositions;
    protected Timer generateTimer;
    protected Timer recomputeTimer;

    public void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
        generateTimer = new Timer(frequenceActivation, setOver: true);
        if (firstActivationRandom) {
            generateTimer.SetElapsedTime(UnityEngine.Random.Range(0, frequenceActivation));
        }
        recomputeTimer = new Timer(MathTools.RandArround(frequenceRecomputePositions, 0.1f));
        orbTrigger.Initialize(orbTrigger.rayon, orbTrigger.durationToActivate);
        ComputePrecomputedPositions();
    }

    protected void ComputePrecomputedPositions() {
        List<Vector3> positions;
        if (choseType == ChoseType.GET_CUBES) {
            positions = getCubesHelper.Get().Select(c => c.transform.position).ToList();
        } else {
            positions = getEmptyPositionsHelper.Get();
        }
        float activationRangeSqr = activationRange * activationRange;
        positions = positions.FindAll(p => Vector3.SqrMagnitude(p - transform.position) <= activationRangeSqr);
        positions = positions.FindAll(p => IsValidPosition(p));
        if (!randomizePositions) {
            positions = positions.OrderByDescending(p => PositionScore(p)).ToList();
        } else {
            MathTools.Shuffle(positions);
        }
        precomputedPositions = new Stack<Vector3>(positions);
    }

    protected abstract bool IsValidPosition(Vector3 p);

    protected float PositionScore(Vector3 position) {
        return Vector3.SqrMagnitude(position - transform.position);
    }

    public void Update() {
        TryGenerate();
    }

    protected void TryGenerate() {
        if(generateTimer.IsOver()) {
            if(precomputedPositions.Count == 0 || recomputeTimer.IsOver()) {
                ComputePrecomputedPositions();
                if(recomputeTimer.IsOver()) {
                    recomputeTimer.Reset();
                }
            }
            if (precomputedPositions.Count != 0) { // Can still have 0 elements after the recomputation !
                GenerateOne(precomputedPositions.Pop());
            }
            generateTimer.Reset();
        }
    }

    protected virtual void GenerateOne(Vector3 position) {
        GenerateLightning(position);
        gm.soundManager.PlayGeneratorGeneratesClip(position);
        GenerateOneSpecific(position);
    }

    protected void GenerateLightning(Vector3 position) {
        Lightning lightning = Instantiate(lightningPrefab).GetComponent<Lightning>();
        lightning.Initialize(transform.position, position, Lightning.PivotType.EXTREMITY);
    }

    protected abstract void GenerateOneSpecific(Vector3 position);

    public void StopGenerate() {
        generateTimer.Stop();
    }

    public virtual void DestroyIn(float duree) {
        StopGenerate();
        Destroy(gameObject, duree);
    }
}
