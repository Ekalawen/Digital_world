using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstBoss : Sonde {

    public Transform componentsFolder;
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
    public List<GameObject> timeZoneByPhases;
    public List<float> esperanceApparitionRandomFillingByPhases;
    public GameObject generateRandomFillingEventPrefab;
    public IController sondeController;
    public IController tracerController;
    public GameObject itemToPopPrefab;
    public int nbLumieres = 15;
    public GameObject pouvoirLocalisationPrefab;

    protected List<Sonde> satellites;
    protected Timer timerAttacks;
    protected List<Coroutine> coroutinesOfNextAttacks;
    protected RandomEvent randomEventToRemove = null;

	public override void Start () {
        base.Start();
        name = "FirstBoss";
        coroutinesOfNextAttacks = new List<Coroutine>();
        satellites = new List<Sonde>(GetComponentsInChildren<Sonde>());
        satellites.Remove(this);
        SetSatellitesActivation(false);
        //gm.soundManager.PlayFirstBossPresenceClip(transform.position, transform); // ==> Ca rend pas bien pour le moment !
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
        Vector3 spawn = transform.position + direction * (transform.localScale[0] / 2.0f + 0.55f);
        Ennemi attack = gm.ennemiManager.GenerateEnnemiFromPrefab(attacksPrefab, spawn);
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
            foreach (Sonde satellite in satellites)
                satellite.gameObject.SetActive(activation);
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
        RandomEvent randomEvent = gm.eventManager.AddEvent(generateRandomFillingEventPrefab);
        randomEvent.esperanceApparition = esperanceApparitionRandomFillingByPhases[phaseIndice - 1];
        randomEventToRemove = randomEvent;
    }

    protected void UpdateTimeZone(int phaseIndice) {
        timeZoneByPhases[phaseIndice - 1].SetActive(true);
    }

    protected void UpdateConsoleMessage(int phaseIndice) {
        gm.console.FirstBossChangementDePhase(phaseIndice, explosionAttackTotalTime);
    }

    protected void AddTimeItem() {
        gm.itemManager.PopItem(itemToPopPrefab);
    }

    protected void RemovePouvoirs() {
        gm.console.PouvoirsDesactives();
        gm.itemManager.RemoveAllPouvoirsGivers();
        gm.player.RemoveAllPouvoirs();
        gm.player.SetNbDoubleJumps(0);
        gm.player.SetPouvoir(pouvoirLocalisationPrefab, PouvoirGiverItem.PouvoirBinding.E);
    }

    protected void PopAllDatas() {
        FixNbLumieres fixNbLumieres = gm.map.gameObject.AddComponent<FixNbLumieres>();
        fixNbLumieres.lumiereType = Lumiere.LumiereType.NORMAL;
        gm.map.nbLumieresInitial = nbLumieres;
        fixNbLumieres.Initialize();
        fixNbLumieres.Activate();
    }

    public void GoToPhase1() {
        UpdateRandomEvent(phaseIndice: 1);
        UpdateAttackRate(phaseIndice: 1);
        UpdateTimeZone(phaseIndice: 1);
    }

    public void GoToPhase2() {
        StartCoroutine(CGoToPhase2());
    }
    protected IEnumerator CGoToPhase2() {
        UpdateConsoleMessage(phaseIndice: 2);
        AddTimeItem();
        yield return StartCoroutine(CExplosionAttackNormale());
        UpdateTimeZone(phaseIndice: 2);
        UpdateAttackRate(phaseIndice: 2);
        UpdateRandomEvent(phaseIndice: 2);
    }

    public void GoToPhase3() {
        StartCoroutine(CGoToPhase3());
    }
    public IEnumerator CGoToPhase3() {
        SetAttackRate(99999);
        UpdateConsoleMessage(phaseIndice: 3);
        AddTimeItem();
        yield return StartCoroutine(CExplosionAttackNormale());
        SetSatellitesActivation(true);
        UpdateTimeZone(phaseIndice: 3);
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
        RemovePouvoirs();
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
                HitPlayer(useCustomTimeMalus: true, explosionAttackDamage);
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
}
