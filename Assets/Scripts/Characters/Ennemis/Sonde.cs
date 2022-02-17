using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonde : Ennemi {

    [Header("Hit")]
	public float distancePoussee; // La distance sur laquelle on va pousser le personnage !
	public float tempsPoussee; // Le temps pendant lequel le personnage est poussé !
    public float spikesTimeOnHit = 0.3f;
    public float pousseeStraightTreshold = 2.0f;
    public TimeMultiplier pousseeTimeMultiplier;

    [Header("Shader")]
    public float wavesSizeWaiting = 0.0f;
    public float wavesSizeActive = 0.1f;
    public float maxSpikesSize = 1.0f;

    protected Poussee pousseeCurrent;
    protected Coroutine spikesSizeCoroutine = null;
    protected Material material;

    public override void Start ()
    {
        base.Start();
        // Initialisation
        name = "Sonde_" + UnityEngine.Random.Range(0, 9999);
        gm = GameManager.Instance;
        player = gm.player;
        InitMaterial();
    }

    protected void InitMaterial() {
        material = GetComponent<Renderer>().material;
        ActivateWaves(false);
        SetSpikesSize(0.0f);
    }

    public void ChangeMaterial(Material newMaterial) {
        GetComponent<Renderer>().material = newMaterial;
        material = GetComponent<Renderer>().material;
        ActivateWaves(true);
        SetSpikesSize(0.0f);
    }

    protected void SetSpikesSize(float size) {
        material.SetFloat("_SpikesSize", size);
    }

    // Update is called once per frame
    public override void UpdateSpecific () {
	}

    // Only works if the Sonde is moving !
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
            if (!player.IsPowerDashing()) {
                HitPlayer();
            }
		}
	}

    protected override void HitPlayerSpecific() {
        ActivateSpikes(spikesTimeOnHit);
        gm.timerManager.AddTimeMultiplierForEnnemiImpact(pousseeTimeMultiplier);
        player.onHitBySonde.Invoke();
    }

    protected override void HitContinuousPlayerSpecific() {
        // Si c'est le cas, on l'envoie ballader !
        if (pousseeCurrent != null && !pousseeCurrent.IsOver()) {
            pousseeCurrent.Stop();
        }
        pousseeCurrent = new PousseeFromTransformWithTreshold(transform, tempsPoussee, distancePoussee, pousseeStraightTreshold);
        player.AddPoussee(pousseeCurrent, isNegative: true);
        player.ResetGrip(); // Pour que le joueur puisse à nouveau s'accrocher aux murs !
    }

    public override EventManager.DeathReason GetDeathReason() {
        return EventManager.DeathReason.SONDE_HIT;
    }

    public void ActivateWaves(bool value) {
        float size = value ? wavesSizeActive : wavesSizeWaiting;
        material.SetFloat("_WavesSize", size);
    }

    public void ActivateSpikes(float time) {
        if(spikesSizeCoroutine != null) {
            StopCoroutine(spikesSizeCoroutine);
        }
        spikesSizeCoroutine = StartCoroutine(CActivateSpikes(time));
    }

    protected IEnumerator CActivateSpikes(float time) {
        Timer timer = new Timer(time);
        while(!timer.IsOver()) {
            float avancement = 1 - timer.GetAvancement();
            SetSpikesSize(maxSpikesSize * avancement);
            yield return null;
        }
    }
}
