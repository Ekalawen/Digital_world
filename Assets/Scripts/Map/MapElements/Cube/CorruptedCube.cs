using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorruptedCube : NonBlackCube {

    [Header("Gameplay")]
    public float dureeBeforeCorruption = 2.5f;
    public int sizeCorruption = 1;
    public int rangeCorruption = 2;
    public CubeType cubeDangerousGeneratedType = CubeType.VOID;
    public CubeType cubeHarmlessGeneratedType = CubeType.NORMAL;

    [Header("Visual")]
    public float generatedDissolveTime = 0.5f;
    public float cancelCorruptionDecomposeDuration = 1.0f;
    public float cancelCorruptionProgressiveDelayByDistance = 0.1f;
    public GameObject lightningPrefab;
    public GeoData corruptionGeoData;

    protected float sizeCorruptionSqr;
    protected Coroutine corruptionCoroutine;

    public override void Initialize() {
        base.Initialize();
        gm.eventManager.InitializeCorruptedCubeManager();
        corruptionCoroutine = null;
        sizeCorruptionSqr = sizeCorruption * sizeCorruption;
        BothMaterialsSetFloat("_DurationCorruption", dureeBeforeCorruption);
    }

    public override void InteractWithPlayer() {
        StartCorruption();
    }

    public void StartCorruption() {
        if (!gm.eventManager.IsGameOver() && !gm.player.IsInvincible()) {
            if (corruptionCoroutine == null) {
                corruptionCoroutine = StartCoroutine(CStartCorruption());
            }
        }
    }

    protected IEnumerator CStartCorruption() {
        CorruptShader();
        AddCorruptionGeoData();
        float duration = MathTools.RandArround(dureeBeforeCorruption, 0.02f);
        yield return new WaitForSeconds(duration);
        Corrupt();
    }

    protected void AddCorruptionGeoData() {
        GeoData newGeoData = new GeoData(corruptionGeoData);
        newGeoData.SetTargetPosition(transform.position);
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }

    protected void CorruptShader() {
        BothMaterialsSetFloat("_IsCorrupted", 1.0f);
        BothMaterialsSetFloat("_TimeStartCorruption", Time.time);
    }

    protected void UncorruptShader() {
        BothMaterialsSetFloat("_IsCorrupted", 0.0f);
    }

    protected void Corrupt() {
        Destroy(); // Before to be able to replace it with a normal cube !

        CreateAllCorruptedCubes();

        CorruptAnotherCubeCloseToPlayer();
    }

    protected void CreateAllCorruptedCubes() {
        for(int i = -sizeCorruption * 2; i <= sizeCorruption * 2; i++) {
            for(int j = -sizeCorruption * 2; j <= sizeCorruption * 2; j++) {
                for(int k = -sizeCorruption * 2; k <= sizeCorruption * 2; k++) {
                    CreateCorruptedCubeAtOffset(new Vector3Int(i, j, k));
                }
            }
        }
    }

    private void CreateCorruptedCubeAtOffset(Vector3Int offset) {
        Vector3 pos = transform.position + offset;
        if (Vector3.SqrMagnitude(pos - transform.position) <= sizeCorruptionSqr && !gm.map.IsLumiereOrItemAt(pos)) {
            bool isAccessibleType = (offset.x == 0 && offset.y == 0 || offset.x == 0 && offset.z == 0 || offset.y == 0 && offset.z == 0);
            CubeType type = isAccessibleType ? cubeHarmlessGeneratedType : cubeDangerousGeneratedType;
            Cube newCube = gm.map.AddCube(pos, type);
            if (newCube != null) {
                newCube.StartDissolveEffect(generatedDissolveTime);
                if (type == cubeDangerousGeneratedType) {
                    gm.eventManager.GetCorruptedCubeManager().RegisterDangerousCubeOfCorruptedCube(newCube);
                } else { // cubeHarmlessGeneratedType
                    gm.eventManager.GetCorruptedCubeManager().RegisterHarmlessCubeOfCorruptedCube(newCube);
                }
            }
        }
    }

    protected void CorruptAnotherCubeCloseToPlayer()
    {
        List<Cube> cubesInRangeOfCorruption = gm.map.GetCubesInSphere(transform.position, rangeCorruption);
        cubesInRangeOfCorruption.Remove(this);
        if (cubesInRangeOfCorruption.Count == 0)
        {
            return;
        }
        Vector3 playerPos = gm.player.transform.position;
        Cube closestCube = cubesInRangeOfCorruption.OrderBy(c => Vector3.SqrMagnitude(c.transform.position - playerPos)).First();
        CreateLightningToNextCorruptedCube(closestCube);
        CorruptedCube corruptedCube = gm.map.SwapCubeType(closestCube, CubeType.CORRUPTED)?.GetComponent<CorruptedCube>();
        if (corruptedCube == null)
        {
            return;
        }
        corruptedCube.StartCorruption();
        gm.eventManager.GetCorruptedCubeManager().RegisterCreatedCorruptedCube(corruptedCube);
    }

    protected void CreateLightningToNextCorruptedCube(Cube cube) {
        Lightning lightning = Instantiate(lightningPrefab).GetComponent<Lightning>();
        lightning.Initialize(transform.position, cube.transform.position, Lightning.PivotType.EXTREMITY);
    }

    public void CancelCorruption() {
        if(corruptionCoroutine != null) {
            StopCoroutine(corruptionCoroutine);
            corruptionCoroutine = null;
        }
        UncorruptShader();
    }
}
