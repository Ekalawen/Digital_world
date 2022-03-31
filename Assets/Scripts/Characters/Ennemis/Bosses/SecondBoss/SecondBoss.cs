using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;

public class SecondBoss : TracerBlast {

    [Header("SecondBoss Parameters")]

    [Header("Impact Faces")]
    public int nbImpactFacesByPhases = 3;
    public float impactStunDuration = 0.5f;

    [Header("Time Reset Drops")]
    public GameObject resetTimeItemPrefab;
    public List<float> resetTimeByPhases;
    public GameObject lightningToTimeResetPrefab;

    [Header("Nanoboost Drops")]
    public GameObject nanoboostPrefab;
    public GameObject lightningToNanoboostPrefab;

    [Header("Generator Drops")]
    public int nbTriesByGeneratorPositions;
    public float timeBetweenDropGenerators;
    public GeoData geoDataToGenerator;
    public List<GameObject> generatorPhase2Prefabs;
    public List<GameObject> generatorPhase3Prefabs;

    [Header("Lumiere Drops")]
    public int nbLumieresToDrop = 15;
    public GameObject lightningToDataPrefab;

    [Header("Explosion Cubes Destruction")]
    public float explosionDestructionRange = 16; // La taille de la map pour que ce soit stylé ! :)
    public float explosionDestructionDecompositionDuration;
    public float explosionDestructionDuration;

    [Header("Builder Reconstruction")]
    public GameObject randomFillingAfterExplosionSingleEventPrefab;
    public GameObject randomFillingRandomEventPrefab;
    public List<float> randomFillingFrequencyByPhases;

    [Header("Lasers")]
    public float laserTimeMalus = 10f;
    public GameObject orthogonalLaserPrefab;
    public GameObject targetingLaserPrefab;
    public GameObject diagonalLaserPrefab;
    public float laserLenght = 25f;
    public float laserWidth = 1.5f;
    public float laserOffset = 0.1f;
    public float delayAfterActivatingLasers = 0.5f;

    [Header("Phase 4")]
    public int phase4NbTargetingLasers = 4;
    public float phase4Vitesse = 5.0f;

    [Header("PresenceSound")]
    public Vector2 presenceSoundVolumeRange = new Vector2(1, 7);

    protected int currentPhase;
    protected bool hasImpactFacesOn = false;
    protected int nbFacesToImpactRemaining;
    protected List<Vector3> faceNormalsUsable;
    protected Vector3 currentImpactFaceDirection;
    protected RandomEvent randomFillingEventToRemove = null;
    protected bool hasOrthogonalLasersUp = false;
    protected bool hasDiagonalLasersUp = false;
    protected List<SecondBossLaser> orthogonalLasers;
    protected List<SecondBossTargetingLaser> targetingLasers;
    protected int nbGeneratorsOfPhase2Collected = 0;
    protected int nbGeneratorsOfPhase3Collected = 0;
    protected float oldVitesse;

    public override void Start () {
        base.Start();
        name = "SecondBoss";
        player.onPowerDashEnnemiImpact.AddListener(OnPowerDashImpactFace);
        orthogonalLasers = new List<SecondBossLaser>();
        targetingLasers = new List<SecondBossTargetingLaser>();
        GoToPhase1();
        //GoToPhase4();
        StartPresenceClip();
    }

    protected void GoToPhase1() {
        Debug.Log($"Phase 1 !");
        currentPhase = 1;
        StartImpactFaces();
    }

    protected void StartImpactFaces() {
        hasImpactFacesOn = true;
        nbFacesToImpactRemaining = nbImpactFacesByPhases;
        faceNormalsUsable = MathTools.GetAllOrthogonalNormals();
        StartOneImpactFace();
    }

    protected void StopImpactFaces() {
        hasImpactFacesOn = false;
        material.SetFloat("_HasVulnerableFace", 0.0f);
    }

    protected void StartOneImpactFace() {
        currentImpactFaceDirection = MathTools.ChoseOne(faceNormalsUsable);
        faceNormalsUsable.Remove(currentImpactFaceDirection);
        material.SetFloat("_HasVulnerableFace", 1.0f);
        material.SetVector("_DirectionVulnerableFace", currentImpactFaceDirection);
    }

    public override bool CanBeHitByPowerDash(PouvoirPowerDash powerDash) {
        return hasImpactFacesOn
            && IsPowerDashGoingTowardImpactFace(powerDash)
            && IsPlayerOnSideOfTheImpactFace();
    }

    protected bool IsPowerDashGoingTowardImpactFace(PouvoirPowerDash powerDash) {
        Vector3 dashDirection = powerDash.GetCurrentPoussee().direction;
        Vector3 impactFaceDirection = transform.rotation * currentImpactFaceDirection;
        bool dashIsInGoodDirection = Vector3.Dot(dashDirection, impactFaceDirection) < 0;
        return dashIsInGoodDirection;
    }

    protected bool IsPlayerOnSideOfTheImpactFace() {
        Vector3 impactFaceDirection = transform.rotation * currentImpactFaceDirection;
        float bossHalfSize = transform.localScale.x / 2;
        float playerDistanceProjected = Vector3.Dot(player.transform.position - transform.position, impactFaceDirection);
        return playerDistanceProjected > bossHalfSize;
    }

    protected override bool IsPlayerComingFromTop() {
        return false; // Le joueur ne peut pas abuser la face du haut ! :)
    }

    protected override bool CanHitPlayerFromSides() {
        return timerContactHit.IsOver()
            && (!IsPlayerOnSideOfTheImpactFace() || !player.IsPowerDashing())
            && !IsStunned()
            && !player.IsTimeHackOn();
    }

    public void OnPowerDashImpactFace(PouvoirPowerDash powerDash, Ennemi ennemi) {
        if(ennemi != this) {
            return;
        }
        if(!hasImpactFacesOn) {
            return;
        }
        nbFacesToImpactRemaining--;
        if(nbFacesToImpactRemaining <= 0) {
            StopImpactFaces();
            GoToNextPhaseIn(GetPowerDashStunDuration(powerDash));
        } else {
            StartOneImpactFace();
        }
    }

    protected override void ApplyStunOfPowerDash(PouvoirPowerDash powerDash) {
        float stunDuration = GetPowerDashStunDuration(powerDash);
        SpeedMultiplier newSpeedMultiplier = new SpeedMultiplier(powerDash.speedMultiplierStun);
        newSpeedMultiplier.duration = stunDuration;
        speedMultiplierController.AddMultiplier(newSpeedMultiplier);
        SetStunnedFor(stunDuration);
        StartCoroutine(CDetectPlayerIn(stunDuration - detectionDureeRotation));
    }

    protected float GetPowerDashStunDuration(PouvoirPowerDash powerDash) {
        return powerDash.dureePoussee + detectionDureeRotation + impactStunDuration;
    }

    protected IEnumerator CDetectPlayerIn(float duration) {
        yield return new WaitForSeconds(duration);
        OnDetectPlayer();
    }

    protected void GoToNextPhaseIn(float duration) {
        StartCoroutine(CGoToNextPhaseIn(duration));
    }

    protected IEnumerator CGoToNextPhaseIn(float duration) {
        yield return new WaitForSeconds(duration);
        GoToNextPhase();
    }

    protected void GoToNextPhase() {
        switch (currentPhase) {
            case 1:
                GoToPhase2();
                break;
            case 2:
                GoToPhase3();
                break;
            case 3:
                GoToPhase4();
                break;
            default:
                break;
        }
    }

    public void GoToPhase2() {
        Debug.Log($"Phase 2 !");
        StartCoroutine(CGoToPhase2());
    }
    protected IEnumerator CGoToPhase2() {
        currentPhase = 2;
        UpdateConsoleMessage(phaseIndice: 2);
        AddTimeItem(phaseIndice: 2);
        AddNanoboost();
        DeployOrthogonalLasers();
        SetVitesseTo0();
        yield return new WaitForSeconds(delayAfterActivatingLasers);
        yield return StartBlast();
        yield return StartCoroutine(CDropGenerators(generatorPhase2Prefabs));
        SetVitesseToOldVitesse();
        TriggerSingleEventRandomFilling();
        UpdateRandomEvent(phaseIndice: 2);
        //StartImpactFaces();
    }

    protected void UpdateRandomEvent(int phaseIndice) {
        gm.eventManager.RemoveEvent(randomFillingEventToRemove);
        RandomEvent randomEvent = gm.eventManager.AddRandomEvent(randomFillingRandomEventPrefab);
        randomEvent.esperanceApparition = randomFillingFrequencyByPhases[phaseIndice - 1];
        randomFillingEventToRemove = randomEvent;
    }

    protected void TriggerSingleEventRandomFilling() {
        gm.eventManager.StartSingleEvent(randomFillingAfterExplosionSingleEventPrefab);
    }

    protected void SetVitesseTo0() {
        IController controller = GetComponent<IController>();
        oldVitesse = controller.vitesse;
        controller.vitesse = 0.0f;
    }

    protected void SetVitesseToOldVitesse() {
        IController controller = GetComponent<IController>();
        controller.vitesse = oldVitesse;
    }

    public override void Blast() {
        base.Blast();
        ApplyVoidLikeExplosion(transform.position,
                               explosionDestructionDecompositionDuration,
                               explosionDestructionDuration,
                               explosionDestructionRange);
    }

    protected override bool IsPlayerBlastable(EnnemiController ennemiController) {
        return base.IsPlayerBlastable(ennemiController)
            && ennemiController.IsPlayerInLineOfSight_ThisIsNotVisible();
    }

    public void ApplyVoidLikeExplosion(Vector3 centerPosition, float decompositionDuration, float explosionDuration, float explosionRange) {
        StartCoroutine(CApplyVoidLikeExplosion(centerPosition, decompositionDuration, explosionDuration, explosionRange));
    }

    protected IEnumerator CApplyVoidLikeExplosion(Vector3 centerPosition, float decompositionDuration, float explosionDuration, float explosionRange) {
        List<Cube> allCubes = explosionRange == - 1 ? gm.map.GetAllCubes() : gm.map.GetCubesInSphere(centerPosition, explosionRange);
        if (explosionRange == -1) {
            explosionRange = allCubes.Select(c => Vector3.SqrMagnitude(centerPosition - c.transform.position)).Max();
            explosionRange = Mathf.Sqrt(explosionRange);
        }
        Timer timer = new Timer(explosionDuration);
        while (!timer.IsOver()) {
            allCubes = allCubes.FindAll(c => c != null);
            float maxRange = explosionRange * timer.GetAvancement();
            float maxRangeSqr = maxRange * maxRange;
            foreach (Cube cube in allCubes) {
                if (Vector3.SqrMagnitude(centerPosition - cube.transform.position) <= maxRangeSqr) {
                    cube.Decompose(decompositionDuration);
                }
            }
            yield return null;
        }
        allCubes = allCubes.FindAll(c => c != null);
        foreach (Cube cube in allCubes) {
            cube.Decompose(decompositionDuration);
        }
    }

    protected void UpdateConsoleMessage(int phaseIndice) {
        gm.console.SecondBossChangementDePhase(phaseIndice, blastLoadDuree);
    }

    protected void AddTimeItem(int phaseIndice) {
        ResetTimeItem resetTemporel = gm.itemManager.PopItem(resetTimeItemPrefab) as ResetTimeItem;
        resetTemporel.settedTime = resetTimeByPhases[phaseIndice - 1];
        GenerateLightningTo(resetTemporel.transform.position, lightningToNanoboostPrefab);
    }

    protected void AddNanoboost() {
        SpeedBoostItem nanoboost = gm.itemManager.PopItem(nanoboostPrefab) as SpeedBoostItem;
        GenerateLightningTo(nanoboost.transform.position, lightningToNanoboostPrefab);
    }

    protected Lightning GenerateLightningTo(Vector3 position, GameObject lightningPrefab) {
        Lightning lightning = Instantiate(lightningPrefab, position, Quaternion.identity).GetComponent<Lightning>();
        lightning.Initialize(transform.position, position, Lightning.PivotType.EXTREMITY);
        return lightning;
    }

    protected IEnumerator CDropGenerators(List<GameObject> generatorPrefabs) {
        List<Vector3> playerPos = new List<Vector3>() { player.transform.position };
        List<Vector3> optimalySpacedPositions = GetOptimalySpacedPositions.GetSpacedPositions(gm.map, generatorPrefabs.Count, playerPos, nbTriesByGeneratorPositions, GetOptimalySpacedPositions.Mode.MAX_MIN_DISTANCE);
        for(int i = 0; i < generatorPrefabs.Count; i++) {
            GameObject generatorPrefab = generatorPrefabs[i];
            Vector3 pos = optimalySpacedPositions[i];
            IGenerator generator = Instantiate(generatorPrefab, pos, Quaternion.identity, parent: gm.map.zonesFolder).GetComponent<IGenerator>();
            generator.Initialize();
            Lightning lightning = GenerateLightningTo(generator.transform.position, generator.lightningPrefab);
            AddGeoPointToGenerator(generator.transform.position, lightning.GetTotalDuration());
            yield return new WaitForSeconds(timeBetweenDropGenerators);
        }
    }

    protected void AddGeoPointToGenerator(Vector3 position, float duration) {
        GeoData newGeoData = new GeoData(geoDataToGenerator);
        newGeoData.SetTargetPosition(position);
        newGeoData.duration = duration;
        player.geoSphere.AddGeoPoint(newGeoData);
    }

    protected void GoToPhase3() {
        Debug.Log($"Phase 3 !");
        StartCoroutine(CGoToPhase3());
    }
    protected IEnumerator CGoToPhase3() {
        currentPhase = 3;
        UpdateConsoleMessage(phaseIndice: 3);
        AddTimeItem(phaseIndice: 3);
        AddNanoboost();
        DeployOneTargetingLaser();
        SetVitesseTo0();
        yield return new WaitForSeconds(delayAfterActivatingLasers);
        yield return StartBlast();
        yield return StartCoroutine(CDropGenerators(generatorPhase3Prefabs));
        SetVitesseToOldVitesse();
        TriggerSingleEventRandomFilling();
        UpdateRandomEvent(phaseIndice: 3);
    }

    protected void GoToPhase4() {
        Debug.Log($"Phase 4 !");
        StartCoroutine(CGoToPhase4());
    }
    protected IEnumerator CGoToPhase4() {
        currentPhase = 4;
        UpdateConsoleMessage(phaseIndice: 4);
        AddTimeItem(phaseIndice: 4);
        AddNanoboost();
        DeployDiagonalLasers();
        SetVitesseTo0();
        yield return new WaitForSeconds(delayAfterActivatingLasers);
        yield return StartBlast();
        SetVitesseToOldVitesse();
        TriggerSingleEventRandomFilling();
        UpdateRandomEvent(phaseIndice: 4);
        IncreaseSpeedToPhase4();
        PopAllDatas();
    }

    protected void IncreaseSpeedToPhase4() {
        IController controller = GetComponent<IController>();
        controller.vitesse = phase4Vitesse;
    }

    protected void PopAllDatas() {
        gm.console.ApparitionDesDatas();
        FixNbLumieres fixNbLumieres = gm.map.gameObject.AddComponent<FixNbLumieres>();
        fixNbLumieres.lumiereType = Lumiere.LumiereType.NORMAL;
        gm.map.nbLumieresInitial = nbLumieresToDrop;
        fixNbLumieres.Initialize();
        fixNbLumieres.Activate();
        foreach(Vector3 lumierePosition in gm.map.GetAllLumieresPositions()) {
            GenerateLightningTo(lumierePosition, lightningToDataPrefab);
        }
    }

    protected void StartPresenceClip() {
        gm.soundManager.PlayFirstBossPresenceClip(transform.position, transform, presenceSoundVolumeRange);
    }

    public override Vector3 PopPosition(MapManager map) {
        return map.GetFreeBoxLocation(Vector3.one * 3.0f);
    }

    protected void DeployOrthogonalLasers() {
        hasOrthogonalLasersUp = true;
        List<Vector3> directions = MathTools.GetAllOrthogonalNormals();
        foreach(Vector3 direction in directions) {
            SecondBossLaser laser = Instantiate(orthogonalLaserPrefab, parent: gm.map.lightningsFolder).GetComponent<SecondBossLaser>();
            laser.Initialize(this, direction);
            orthogonalLasers.Add(laser);
        }
    }

    protected void DeployDiagonalLasers() {
        hasDiagonalLasersUp = true;
        List<Vector3> directions = MathTools.GetAllDiagonalNormals();
        foreach(Vector3 direction in directions) {
            SecondBossLaser laser = Instantiate(diagonalLaserPrefab, parent: gm.map.lightningsFolder).GetComponent<SecondBossLaser>();
            laser.Initialize(this, direction);
            orthogonalLasers.Add(laser);
        }
    }

    protected void DeployOneTargetingLaser() {
        SecondBossTargetingLaser targetingLaser = Instantiate(targetingLaserPrefab, parent: gm.map.lightningsFolder).GetComponent<SecondBossTargetingLaser>();
        targetingLaser.Initialize(this);
        targetingLasers.Add(targetingLaser);
    }

    public void HitPlayerWithLaser() {
        HitPlayerCustom(EventManager.DeathReason.SECOND_BOSS_LASER, laserTimeMalus);
    }

    public void CollectGeneratorsOfPhase2() {
        nbGeneratorsOfPhase2Collected++;
        if(nbGeneratorsOfPhase2Collected >= generatorPhase2Prefabs.Count) {
            StartImpactFaces();
        }
    }

    public void CollectGeneratorsOfPhase3() {
        nbGeneratorsOfPhase3Collected++;
        if(nbGeneratorsOfPhase3Collected >= generatorPhase3Prefabs.Count) {
            StartImpactFaces();
        }
    }
}
