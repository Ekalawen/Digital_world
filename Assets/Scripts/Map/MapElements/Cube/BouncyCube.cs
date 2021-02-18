using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class BouncyCube : Cube {

    public float distancePoussee = 3.0f;
    public float dureePoussee = 0.3f;
    public float dammageOnHit = 0.0f;
    public Transform particlesFolder;

    protected Timer timerAddPoussee;
    protected float particulesConstantSpawnRate;
    protected float particulesPeriodicSpawnRate;
    protected List<VisualEffect> particles = null;

    public override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        timerAddPoussee = new Timer(0.1f);
        timerAddPoussee.SetOver();
        particulesConstantSpawnRate = GetParticules()[0].GetFloat("ConstantSpawnRate");
        particulesPeriodicSpawnRate = GetParticules()[0].GetFloat("PeriodicSpawnRate");
        StopParticles(); // On les lances dans StartDissolveEffect ! :)
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        if (!timerAddPoussee.IsOver())
            return;
        Player player = gm.player;
        Vector3 direction = GetDirectionPoussee(player.transform.position);
        player.AddPoussee(new Poussee(direction, dureePoussee, distancePoussee));
        player.ResetGrip();
        if (Vector3.Dot(direction, gm.gravityManager.Up()) > 0 && Input.GetKey(KeyCode.Space)) {
            player.SetCarefulJumping(Player.EtatPersonnage.AU_SOL);
        } else {
            player.RemoveGravityEffectFor(dureePoussee); // La gravité est déjà artificiellement annulée lors d'un saut :)
            player.PlayJumpSound(); // Juste ici car il est déjà joué dans la simulation du saut ! :)
        }
        if (dammageOnHit > 0.0f) {
            gm.timerManager.RemoveTime(dammageOnHit, EventManager.DeathReason.TOUCHED_BOUNCY_CUBE);
        }
        timerAddPoussee.Reset();
    }

    protected Vector3 GetDirectionPoussee(Vector3 position) {
        List<Vector3> normals = GetAllNormals();
        Vector3 direction = (position - transform.position).normalized;
        return normals.OrderBy(n => Vector3.Dot(direction, n)).Last();
    }

    protected List<Vector3> GetAllNormals() {
        return new List<Vector3>() {
            transform.forward,
            - transform.forward,
            transform.right,
            - transform.right,
            transform.up,
            - transform.up,
        };
    }

    protected override IEnumerator CDestroyIn(float duree) {
        float dureeMaxParticles = GetParticules()[0].GetVector2("Lifetime").y;
        float duree1 = Mathf.Max(duree - dureeMaxParticles, 0);
        float duree2 = duree - duree1;
        yield return new WaitForSeconds(duree1);
        StopParticles();
        yield return new WaitForSeconds(duree2);
        Destroy();
    }

    protected List<VisualEffect> GetParticules() {
        if (particles == null) {
            particles = particlesFolder.GetComponentsInChildren<VisualEffect>().ToList();
            Debug.Log($"particules = {particles.Count}");
        }
        return particles;
    }

    protected void StopParticles() {
        foreach(VisualEffect vfx in GetParticules()) {
            vfx.SetFloat("ConstantSpawnRate", 0);
            vfx.SetFloat("PeriodicSpawnRate", 0);
        }
    }

    protected void StartParticles() {
        foreach(VisualEffect vfx in GetParticules()) {
            vfx.SetFloat("ConstantSpawnRate", particulesConstantSpawnRate);
            vfx.SetFloat("PeriodicSpawnRate", particulesPeriodicSpawnRate);
        }
    }

    public override void StartDissolveEffect(float dissolveTime, float playerProximityCoef = 0) {
        base.StartDissolveEffect(dissolveTime, playerProximityCoef);
        // On refait le calcul compliqué qui se trouve dans le shader pour trouver quand le cube commencera à se dissolve, et donc quand il faudra StartParticules ! :)
        float dissolveStartingTime = Time.time;
        float playerDistance = Vector3.Distance(transform.position, gm.player.transform.position);
        float playerInfluence = playerDistance * playerProximityCoef;
        // On cherche time tel que finalTime = 1; ==> parce que finalTime = 0 ça marche pas :'(
        // float time = Time.time;
        // float finalTime = 1 - ((Mathf.Max(time - dissolveStartingTime, 0) / dissolveTime) + playerInfluence);
        // 1 - ((Mathf.Max(time - dissolveStartingTime, 0) / dissolveTime) + playerInfluence) = 1
        // (Mathf.Max(time - dissolveStartingTime, 0) / dissolveTime) + playerInfluence = 0
        // Mathf.Max(time - dissolveStartingTime, 0) = (0 - playerInfluence) * dissolveTime
        // time - dissolveStartingTime = (0 - playerInfluence) * dissolveTime // car time >= dissolveStartingTime ! :)
        // time = (0 - playerInfluence) * dissolveTime + dissolveStartingTime
        float timeActivation = (0 - playerInfluence) * dissolveTime + dissolveStartingTime;
        //timeActivation = timeActivation + (0.5f - (timeActivation % 0.5f));
        float dureeActivation = timeActivation - Time.time;
        StartCoroutine(StartParticlesIn(dureeActivation));
    }

    protected IEnumerator StartParticlesIn(float dureeActivation) {
        yield return new WaitForSeconds(dureeActivation);
        StartParticles();
    }
}
