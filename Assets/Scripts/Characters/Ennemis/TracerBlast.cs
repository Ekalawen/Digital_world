using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerBlast : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    [Header("Blast")]
    public float blastLoadDuree = 2.0f;
    public float blastPousseeDuree = 0.3f;
    public float blastPousseeDistance = 10f;

    protected Coroutine blastCoroutine = null;

    public override void Start() {
        base.Start();
    }

    public override void UpdateSpecific() {
    }

	void OnControllerColliderHit(ControllerColliderHit hit) {
        Cube cube = hit.collider.gameObject.GetComponent<Cube>();
        Player player = hit.collider.gameObject.GetComponent<Player>();
        GameManager gm = GameManager.Instance;

        if(cube != null) {
            if (!cube.bIsRegular) {
                cube.Explode();
                //GameObject go = Instantiate(explosionParticlesPrefab, cube.transform.position, Quaternion.identity);
                //ParticleSystem particle = go.GetComponent<ParticleSystem>();
                //ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
                //Material mat = psr.material;
                //Material newMaterial = new Material(mat);
                //newMaterial.color = cube.GetColor();
                //psr.material = newMaterial;
                //float particuleTime = particle.main.duration;
                //Destroy(go, particuleTime);
                //gm.map.DeleteCube(cube);
            }
        }

        if (player != null) {
            HitPlayer();
        }
    }

    public void StartBlast() {
        blastCoroutine = StartCoroutine(CStartBlast());
    }

    protected IEnumerator CStartBlast() {
        LoadBlast();
        yield return new WaitForSeconds(blastLoadDuree);
        Blast();
    }

    public void LoadBlast() {
        // Start loading animation
    }

    public void Blast() {
        // Start blast animation
        EnnemiController ennemiController = GetComponent<EnnemiController>();
        if(ennemiController != null && ennemiController.IsPlayerVisible()) {
            HitPlayer();
        }
    }
    protected override void HitPlayerSpecific() {
        Vector3 direction = player.transform.position - transform.position;
        Poussee poussee = new Poussee(direction, blastPousseeDuree, blastPousseeDistance);
        player.AddPoussee(poussee);
        player.ResetGrip();
        gm.postProcessManager.UpdateHitEffect();
    }

    protected override void HitContinuousPlayerSpecific() {
    }

    public void StopBlast() {
        if (blastCoroutine != null) {
            StopCoroutine(blastCoroutine);
        }
    }

    public override void DisplayHitMessage() {
        if (!gm.eventManager.IsGameOver()) {
            gm.console.JoueurToucheTracer();
        }
    }

    public override EventManager.DeathReason GetDeathReason() {
        return EventManager.DeathReason.TRACER_HIT;
    }
}
