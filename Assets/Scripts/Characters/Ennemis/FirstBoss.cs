using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstBoss : Sonde {

    public Transform componentsFolder;
    public float attackDelay = 0.5f;
    public List<float> attackRateByPhases;
    public GameObject attacksPrefab;
    public float explosionAttackStandardDelay = 3.8f;
    public float explosionAttackDelay = 3.8f;
    public float explosionAttackTotalTime = 7.0f;
    public float explosionAttackDistance = 5.5f;
    public float explosionAttackDamage = 100f;
    public GameObject explosionAttackParticulesPrefab;
    public List<GameObject> timeZoneByPhases;
    public List<float> esperanceApparitionRandomFillingByPhases;
    public GameObject generateRandomFillingEventPrefab;

    protected List<Sonde> satellites;
    protected Timer timerAttacks;
    protected Transform projectilesFolder;
    protected List<Coroutine> coroutinesOfNextAttacks;
    protected RandomEvent randomEventToRemove = null;

	public override void Start () {
        base.Start();
        name = "FirstBoss";
        projectilesFolder = new GameObject("Projectiles").transform;
        projectilesFolder.transform.parent = transform;
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
        Vector3 spawn = transform.position + direction * (transform.localScale[0] / 2.0f + 0.55f);
        Ennemi attack = gm.ennemiManager.GenerateEnnemiFromPrefab(attacksPrefab, spawn);
        attack.transform.parent = projectilesFolder;
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
        gm.console.FirstBossChangementDePhase(phaseIndice);
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
        yield return StartCoroutine(ExplosionAttack());
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
        yield return StartCoroutine(ExplosionAttack());
        SetSatellitesActivation(true);
        UpdateTimeZone(phaseIndice: 3);
        UpdateAttackRate(phaseIndice: 3);
        UpdateRandomEvent(phaseIndice: 3);
    }

    public void GoToPhase4() {
        StartCoroutine(CGoToPhase4());
    }
    public IEnumerator CGoToPhase4() {
        UpdateConsoleMessage(phaseIndice: 4);
        SetAttackRate(99999);
        yield return StartCoroutine(ExplosionAttack());
        //SetSatellitesActivation(true);
        //UpdateTimeZone(phaseIndice: 3);
        //UpdateAttackRate(phaseIndice: 3);
        //UpdateRandomEvent(phaseIndice: 3);
    }

    public IEnumerator ExplosionAttack() {
        // Start decreasing
        PlayParticles();
        // Play SOUND here !!!
        IController controller = GetComponent<IController>();
        float oldVitesse = controller.vitesse;
        controller.vitesse = 0.0f;

        // Wait until end of decreasing ...
        yield return new WaitForSeconds(explosionAttackDelay);

        // Increasing !!! Explosion !!!
        Timer timer = new Timer(explosionAttackTotalTime - explosionAttackDelay);
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

    protected void PlayParticles() {
        GameObject go = Instantiate(explosionAttackParticulesPrefab, transform.position, Quaternion.identity, componentsFolder);
        //// Set time of first part
        //float acceleration = explosionAttackDelay / explosionAttackStandardDelay;
        //GameObject decreasingBall = go.transform.Find("DecreasingBall").gameObject;
        //ParticleSystem[] decreasingParticles = decreasingBall.GetComponentsInChildren<ParticleSystem>();
        //for (int i = 0; i < decreasingParticles.Length; i++)
        //{
        //    ParticleSystem ps = decreasingParticles[i];
        //    ps.Stop();
        //    ParticleSystem.MainModule main = ps.main;
        //    main.duration = main.duration * acceleration; // On peut pas modifier la duration pendant que le système joue !
        //    ps.Play();
        //}

        //// Set time of second part
        //GameObject increasingBall = go.transform.Find("IncreasingBall").gameObject;
        //ParticleSystem[] increasingParticles = increasingBall.GetComponentsInChildren<ParticleSystem>();
        //for (int i = 0; i < decreasingParticles.Length; i++)
        //{
        //    ParticleSystem ps = decreasingParticles[i];
        //    ps.Stop();
        //    ParticleSystem.MainModule main = ps.main;
        //    main.startDelay = explosionAttackDelay;
        //    ps.Play();
        //}

        Destroy(go, explosionAttackTotalTime * 2);
    }
}
