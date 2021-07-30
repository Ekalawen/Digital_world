using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class IGenerator : MonoBehaviour {

    public enum ChoseType { GET_CUBES, GET_EMPTY };

    [Header("Parameters")]
    public float maxRangeOfSelection = 10.0f;
    public float frequenceActivation = 0.25f;
    public ChoseType choseType;
    [ConditionalHide("choseType", ChoseType.GET_CUBES)]
    public GetCubesHelper getCubesHelper;
    [ConditionalHide("choseType", ChoseType.GET_EMPTY)]
    public GetEmptyPositionsHelper getEmptyPositionsHelper;

    [Header("Lightning")]
    public GameObject lightningPrefab;

    protected GameManager gm;
    protected MapManager map;
    protected Stack<Vector3> precomputedPositions;
    protected Timer generateTimer;

    public void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
        generateTimer = new Timer(frequenceActivation, setOver: true);
        ComputePrecomputedPositions();
    }

    protected void ComputePrecomputedPositions() {
        if (choseType == ChoseType.GET_CUBES) {
            precomputedPositions = new Stack<Vector3>(getCubesHelper.Get().Select(c => c.transform.position).OrderBy(p => PositionScore(p)));
        } else {
            precomputedPositions = new Stack<Vector3>(getEmptyPositionsHelper.Get().OrderBy(p => PositionScore(p)));
        }
    }

    protected float PositionScore(Vector3 position) {
        return Vector3.SqrMagnitude(position - transform.position);
    }

    public void Update() {
        TryGenerate();
    }

    protected void TryGenerate() {
        if(generateTimer.IsOver()) {
            if(precomputedPositions.Count == 0) {
                ComputePrecomputedPositions();
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

    public void DestroyIn(float duree) {
        StopGenerate();
        Destroy(gameObject, duree);
    }
}
