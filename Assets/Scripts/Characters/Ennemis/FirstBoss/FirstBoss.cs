using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FirstBoss : Sonde {

    [Header("FirstBoss Parameters")]

    [Header("Attacks")]
    public float attackDelay = 0.5f;
    public List<float> attackRateByPhases;
    public GameObject attacksPrefab;
    public float explosionAttackDelay = 3.8f;
    public float explosionAttackTotalTime = 7.0f;
    public float fastExplosionAttackDelay = 1.0f;
    public float fastExplosionAttackTotalTime = 2.0f;
    public float explosionAttackDistance = 5.5f;
    public float explosionAttackDamage = 100f;
    public GameObject explosionAttackParticulesPrefab;
    public GameObject fastExplosionAttackParticulesPrefab;

    [Header("OrbTriggers")]
    public List<OrbTrigger> orbTriggersByPhases;

    [Header("RandomFillings")]
    public List<float> esperanceApparitionRandomFillingByPhases;
    public GameObject generateRandomFillingEventPrefab;

    [Header("PouvoirGiver")]
    public GameObject pouvoirDash333Prefab;
    public VisualEffect particlesGainPouvoir;
    public float timeBeforeGivingDash = 3.0f;
    public float timeToStopParticles = 1.5f;
    public float timeAfterGivingDash = 3.0f;

    [Header("ThingsToDrop")]
    public GameObject itemToPopPrefab;
    public int nbLumieres = 15;
    public GameObject lightningToItemPrefab;
    public GameObject lightningToDataPrefab;
    public GameObject pouvoirLocalisationPrefab;
    public List<GameObject> generatorPhase2Prefabs;
    public List<GameObject> generatorPhase3Prefabs;
    public int nbTriesByGeneratorPositions = 3;
    public float timeBetweenDropGenerators = 0.8f;

    [Header("Links")]
    public Transform componentsFolder;
    public IController sondeController;
    public IController tracerController;

    protected List<Sonde> satellites;
    protected Timer timerAttacks;
    protected List<Coroutine> coroutinesOfNextAttacks;
    protected RandomEvent randomEventToRemove = null;
    protected EventManager.DeathReason currentDeathReason = EventManager.DeathReason.FIRST_BOSS_HIT;
    protected int nbGeneratorsOfPhase2Collected = 0;
    protected int nbGeneratorsOfPhase3Collected = 0;

    public override void Start () {
        base.Start();
        name = "FirstBoss";
        coroutinesOfNextAttacks = new List<Coroutine>();
        satellites = new List<Sonde>(GetComponentsInChildren<Sonde>());
        satellites.Remove(this);
        SetSatellitesActivation(false);
        GoToPhase1();
    }

    public override void UpdateSpecific () {
        base.UpdateSpecific();

        if (Time.timeSinceLevelLoad < GetComponent<IController>().tempsInactifDebutJeu)
            return;

        Attack();
	}

    public void Attack() {
        if (timerAttacks.IsOver()) {
            timerAttacks.Reset();
            Vector3 direction = (player.transform.position - transform.position).normalized;
            coroutinesOfNextAttacks.Add(StartCoroutine(CAttackInDelay(direction)));
        }
    }

    protected IEnumerator CAttackInDelay(Vector3 direction) {
        yield return new WaitForSeconds(attackDelay);
        Vector3 spawnPosition = transform.position + direction * (transform.localScale[0] / 2.0f + 0.55f);
        Ennemi attack = gm.ennemiManager.GenerateEnnemiFromPrefab(attacksPrefab, spawnPosition);
        GoToDirectionController projectile = attack.GetComponent<GoToDirectionController>();
        projectile.direction = direction;
    }

    public void SetAttackRate(float newAttackRate) {
        foreach(Coroutine coroutine in coroutinesOfNextAttacks) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
            }
        }
        coroutinesOfNextAttacks.Clear();
        timerAttacks = new Timer(newAttackRate);
    }

    public void UpdateAttackRate(int phaseIndice) {
        SetAttackRate(attackRateByPhases[phaseIndice - 1]);
    }

    public void SetSatellitesActivation(bool activation) {
        if (!activation) {
            foreach (Sonde satellite in satellites) {
                satellite.gameObject.SetActive(activation);
            }
        } else {
            StartCoroutine(CActivateSatellitesProgressively());
        }
    }

    protected IEnumerator CActivateSatellitesProgressively() {
        foreach (Sonde satellite in satellites) {
            satellite.gameObject.SetActive(true);
            gm.ennemiManager.RegisterAlreadyExistingEnnemi(satellite);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }

    protected void UpdateRandomEvent(int phaseIndice) {
        gm.eventManager.RemoveEvent(randomEventToRemove);
        RandomEvent randomEvent = gm.eventManager.AddRandomEvent(generateRandomFillingEventPrefab);
        randomEvent.esperanceApparition = esperanceApparitionRandomFillingByPhases[phaseIndice - 1];
        randomEventToRemove = randomEvent;
    }

    protected void UpdateOrbTrigger(int phaseIndice) {
        OrbTrigger orbTrigger = orbTriggersByPhases[phaseIndice - 1];
        orbTrigger.gameObject.SetActive(true);
        orbTrigger.Initialize(orbTrigger.rayon, orbTrigger.durationToActivate);
    }

    protected void UpdateConsoleMessage(int phaseIndice) {
        gm.console.FirstBossChangementDePhase(phaseIndice, explosionAttackTotalTime);
    }

    protected void AddTimeItem() {
        Item item = gm.itemManager.PopItem(itemToPopPrefab);
        GenerateLightningTo(item.transform.position, lightningToItemPrefab);
    }

    protected void GenerateLightningTo(Vector3 position, GameObject lightningPrefab) {
        Lightning lightning = Instantiate(lightningPrefab, position, Quaternion.identity).GetComponent<Lightning>();
        lightning.Initialize(transform.position, position, Lightning.PivotType.EXTREMITY);
    }

    protected void RemovePouvoirs() {
        gm.console.PouvoirsDesactives();
        gm.itemManager.RemoveAllPouvoirsGivers();
        gm.player.RemoveAllPouvoirs();
        //gm.player.SetNbDoubleJumps(0); // C'est la seule façon de s'en sortir du joueur au stade 4 :)
        gm.player.SetPouvoir(pouvoirLocalisationPrefab, PouvoirGiverItem.PouvoirBinding.E);
    }

    protected void PopAllDatas() {
        gm.console.ApparitionDesDatas();
        FixNbLumieres fixNbLumieres = gm.map.gameObject.AddComponent<FixNbLumieres>();
        fixNbLumieres.lumiereType = Lumiere.LumiereType.NORMAL;
        gm.map.nbLumieresInitial = nbLumieres;
        fixNbLumieres.Initialize();
        fixNbLumieres.Activate();
        foreach(Vector3 lumierePosition in gm.map.GetAllLumieresPositions()) {
            GenerateLightningTo(lumierePosition, lightningToDataPrefab);
        }
    }

    public void GoToPhase1() {
        UpdateRandomEvent(phaseIndice: 1);
        UpdateAttackRate(phaseIndice: 1);
        UpdateOrbTrigger(phaseIndice: 1);
    }

    public void GoToPhase2() {
        StartCoroutine(CGoToPhase2());
    }
    protected IEnumerator CGoToPhase2() {
        yield return StartCoroutine(CGiveDash333());
        UpdateConsoleMessage(phaseIndice: 2);
        AddTimeItem();
        yield return StartCoroutine(CExplosionAttackNormale());
        yield return StartCoroutine(CDropGenerators(generatorPhase3Prefabs));
        //yield return StartCoroutine(CDropGenerators(generatorPhase2Prefabs));
        UpdateAttackRate(phaseIndice: 2);
        UpdateRandomEvent(phaseIndice: 2);
    }

    protected IEnumerator CDropGenerators(List<GameObject> generatorPrefabs) {
        List<Vector3> playerPos = new List<Vector3>() { player.transform.position };
        List<Vector3> optimalySpacedPositions = GetOptimalySpacedPositions.GetSpacedPositions(gm.map, generatorPrefabs.Count, playerPos, nbTriesByGeneratorPositions, GetOptimalySpacedPositions.Mode.MAX_MIN_DISTANCE);
        for(int i = 0; i < generatorPrefabs.Count; i++) {
            GameObject generatorPrefab = generatorPrefabs[i];
            Vector3 pos = optimalySpacedPositions[i];
            IGenerator generator = Instantiate(generatorPrefab, pos, Quaternion.identity, parent: gm.map.zonesFolder).GetComponent<IGenerator>();
            generator.Initialize();
            GenerateLightningTo(generator.transform.position, generator.lightningPrefab);
            yield return new WaitForSeconds(timeBetweenDropGenerators);
        }
    }

    public void CollectGeneratorsOfPhase2() {
        nbGeneratorsOfPhase2Collected++;
        if(nbGeneratorsOfPhase2Collected >= generatorPhase2Prefabs.Count) {
            UpdateOrbTrigger(phaseIndice: 2);
        }
    }

    public void CollectGeneratorsOfPhase3() {
        nbGeneratorsOfPhase3Collected++;
        if(nbGeneratorsOfPhase3Collected >= generatorPhase3Prefabs.Count) {
            UpdateOrbTrigger(phaseIndice: 3);
        }
    }

    protected IEnumerator CGiveDash333() {
        IController controller = GetComponent<IController>();
        float oldVitesse = controller.vitesse;
        controller.vitesse = 0.0f;

        StartCoroutine(CStartParticlesGainPouvoir());
        yield return new WaitForSeconds(timeBeforeGivingDash);
        GiveDash333();
        yield return new WaitForSeconds(timeAfterGivingDash);

        controller.vitesse = oldVitesse;
    }

    protected void GiveDash333()
    {
        PouvoirGiverItem.PouvoirBinding pouvoirBinding = PouvoirGiverItem.PouvoirBinding.LEFT_CLICK;
        player.SetPouvoir(pouvoirDash333Prefab, pouvoirBinding);
        IPouvoir pouvoir = player.GetPouvoirLeftClick().GetComponent<IPouvoir>();
        gm.console.CapturePouvoirGiverItem(pouvoir.nom, pouvoirBinding);
        gm.pointeur.Initialize();
        gm.soundManager.PlayGainDash333();
    }

    protected IEnumerator CStartParticlesGainPouvoir() {
        particlesGainPouvoir.SendEvent("Explode");
        Timer timerToStop = new Timer(timeToStopParticles);
        Timer timerToUpdate = new Timer(timeToStopParticles + particlesGainPouvoir.GetVector2("Lifetime").y);
        while(!timerToUpdate.IsOver()) {
            particlesGainPouvoir.SetVector3("PositionToGoTo", player.transform.position);
            if(timerToStop.IsOver()) {
                particlesGainPouvoir.SendEvent("Stop");
                timerToStop.SetDuree(timerToUpdate.GetDuree());
            }
            yield return null;
        }
    }

    public void GoToPhase3() {
        StartCoroutine(CGoToPhase3());
    }
    public IEnumerator CGoToPhase3() {
        SetAttackRate(99999);
        UpdateConsoleMessage(phaseIndice: 3);
        AddTimeItem();
        yield return StartCoroutine(CExplosionAttackNormale());
        //yield return StartCoroutine(CDropGenerators(generatorPhase3Prefabs));
        SetSatellitesActivation(true);
        UpdateAttackRate(phaseIndice: 3);
        UpdateRandomEvent(phaseIndice: 3);
    }

    public void GoToPhase4() {
        StartCoroutine(CGoToPhase4());
    }
    public IEnumerator CGoToPhase4() {
        SetAttackRate(99999);
        UpdateConsoleMessage(phaseIndice: 4);
        AddTimeItem();
        yield return StartCoroutine(CExplosionAttackNormale());
        //RemovePouvoirs();
        PopAllDatas();
        UpdateAttackRate(phaseIndice: 4);
        UpdateRandomEvent(phaseIndice: 4);
        SwapControllers();
    }

    protected void SwapControllers() {
        if(sondeController.enabled) {
            Debug.Log("TracerMode !");
            sondeController.enabled = false;
            tracerController.enabled = true;
        } else {
            Debug.Log("SondeMode !");
            tracerController.enabled = false;
            sondeController.enabled = true;
        }
    }

    public IEnumerator CExplosionAttackNormale() {
        yield return StartCoroutine(CExplosionAttack(explosionAttackDelay, explosionAttackTotalTime, explosionAttackParticulesPrefab));
    }
    public IEnumerator CExplosionAttackFast() {
        yield return StartCoroutine(CExplosionAttack(fastExplosionAttackDelay, fastExplosionAttackTotalTime, fastExplosionAttackParticulesPrefab));
    }
    public void ExplosionAttackFast() {
        StartCoroutine(CExplosionAttackFast());
    }

    public IEnumerator CExplosionAttack(float delay, float totalTime, GameObject particlesPrefab) {
        // Start decreasing
        PlayParticles(totalTime, particlesPrefab);
        gm.soundManager.PlayDecreasingBallFirstBoss(transform.position, delay);
        IController controller = GetComponent<IController>();
        float oldVitesse = controller.vitesse;
        controller.vitesse = 0.0f;

        // Wait until end of decreasing ...
        yield return new WaitForSeconds(delay);

        // Increasing !!! Explosion !!!
        gm.soundManager.PlayIncreasingBallFirstBoss(transform.position, totalTime - delay);
        Timer timer = new Timer(totalTime - delay);
        while(!timer.IsOver()) {
            if (Vector3.Distance(player.transform.position, transform.position) <= explosionAttackDistance)
                HitPlayerCustom(EventManager.DeathReason.FIRST_BOSS_BLAST, explosionAttackDamage);
            List<Cube> nearCubes = gm.map.GetCubesInSphere(transform.position, explosionAttackDistance);
            foreach (Cube cube in nearCubes) {
                if(cube != null)
                    cube.Explode();
            }
            yield return null;
        }

        // End of explosion
        controller.vitesse = oldVitesse;
    }

    protected void PlayParticles(float totalTime, GameObject particlesPrefab) {
        GameObject go = Instantiate(particlesPrefab, transform.position, Quaternion.identity, componentsFolder);

        Destroy(go, totalTime * 2);
    }

    public override Vector3 PopPosition(MapManager map) {
        return map.GetFreeSphereLocation(2.5f);
    }

    public override bool IsMoving() {
        SondeController sondeController = GetComponent<SondeController>();
        return Time.timeSinceLevelLoad >= sondeController.tempsInactifDebutJeu;
    }

    public override bool IsInactive() {
        return !IsMoving();
    }

    public override EventManager.DeathReason GetDeathReason() {
        return currentDeathReason;
    }

    protected void HitPlayerCustom(EventManager.DeathReason deathReason, float timeMalus) {
        EventManager.DeathReason oldDeathReason = currentDeathReason;
        currentDeathReason = deathReason;
        HitPlayer(useCustomTimeMalus: true, customTimeMalus: timeMalus);
        currentDeathReason = oldDeathReason;
    }
}
