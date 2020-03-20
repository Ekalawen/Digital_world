using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ennemi : Character {

	public float vitesseMin; // On veut une vitesse aléatoire comprise
	public float vitesseMax; // entre min et max !
    public float tempsInactifDebutJeu; // Le temps pendant lequel la sonde n'agira pas en début de partie
	public float distanceDeDetection; // La distance à partir de laquelle le probe peut pourchasser l'ennemi
    public float timeMalusOnHit = 5.0f; // Le temps que perd le joueur lorsqu'il se fait touché !
    public float timeBetweenTwoHits = 1.0f;

	protected GameManager gm;
	protected Player player;
	protected float vitesse;
    protected float lastTimeHit;

	public override void Start () {
        base.Start();
        gm = GameManager.Instance;
        player = gm.player;
		controller = this.GetComponent<CharacterController> ();
		//vitesse = Mathf.Exp(Random.Range(Mathf.Log(vitesseMin), Mathf.Log(vitesseMax)));
		vitesse = Random.Range(vitesseMin, vitesseMax);
        lastTimeHit = Time.timeSinceLevelLoad;
	}

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(GameManager.Instance.IsTimeFreezed()) {
            return;
        }

        if (Time.timeSinceLevelLoad >= tempsInactifDebutJeu)
            UpdateSpecific();

        ApplyPoussees();
	}

    public abstract void UpdateSpecific();

    // Permet de savoir si l'ennemi voit le joueur
    public bool IsPlayerVisible() {
        // Si l'ennemie est suffisament proche et qu'il est visible !
        RaycastHit hit;
        Ray ray = new Ray (transform.position, player.transform.position - transform.position);
        return Physics.Raycast(ray, out hit, distanceDeDetection) && hit.collider.name == "Joueur";
    }

    protected Vector3 Move(Vector3 target) {
        Vector3 direction = (target - transform.position).normalized;
        Vector3 finalMouvement = direction * vitesse * Time.deltaTime;

        // Si c'est trop long, on ajuste
        if (Vector3.Magnitude(finalMouvement) > Vector3.Distance(transform.position, target)) {
            finalMouvement = target - transform.position;
        }

        controller.Move(finalMouvement);

        return finalMouvement;
    }

    protected virtual void HitPlayer() {
        HitContinuousPlayerSpecific();
        if(Time.timeSinceLevelLoad - lastTimeHit > timeBetweenTwoHits) {
            HitPlayerSpecific();

            lastTimeHit = Time.timeSinceLevelLoad;
            gm.timerManager.AddTime(-timeMalusOnHit);
            DisplayHitMessage();
            PlayHitSound();
        }
    }

    public virtual void DisplayHitMessage() {
        // Et on affiche un message dans la console !
        if (!gm.eventManager.IsGameOver()) {
            gm.console.JoueurToucheSonde();
        }
    }

    public virtual void PlayHitSound() {
        if(!gm.eventManager.IsGameOver())
            gm.soundManager.PlayHitClip(transform.position);
    }

    protected abstract void HitPlayerSpecific();
    protected abstract void HitContinuousPlayerSpecific();

    public abstract bool IsInactive();
}
