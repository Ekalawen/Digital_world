using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonde : MonoBehaviour {

	public enum EtatEnnemi {WAITING, TRACKING, DEFENDING, RUSHING};

	public float vitesseMin; // On veut une vitesse aléatoire comprise
	public float vitesseMax; // entre min et max !
	public float puissancePoussee; // La force avec laquelle on va pousser le joueur en cas de contact !
	public float tempsPouseePersonnage; // Le temps pendant lequel le personnage est poussé !
	public float distanceDeDetection; // La distance à partir de laquelle le probe peut pourchasser l'ennemi
	public float coefficiantDeRushVitesse; // Le multiplicateur de vitesse lorsque les drones rushs
	public float coefficiantDeRushDistanceDeDetection; // Le multiplicateur de la portée de détection quand on est en rush
    public float tempsInactifDebutJeu; // Le temps pendant lequel la sonde n'agira pas en début de partie

	protected GameManager gm;
	protected Player player;
	[HideInInspector]
	public CharacterController controller;
	protected EtatEnnemi etat;
	protected Vector3 lastPositionSeen; // La dernière position à laquelle le joueur a été vu !
	protected float vitesse;

	private Vector3 pousee; // Lorsque le personnage est poussé
	private float debutPousee; // Le début de la poussée
	private float tempsPousee; // Le temps pendant lequel le personnage reçoit cette poussée

	void Start () {
		// Initialisation
		name = "Sonde_" + Random.Range (0, 9999);
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = gm.player;
		controller = this.GetComponent<CharacterController> ();
		etat = EtatEnnemi.WAITING;
		lastPositionSeen = transform.position;
		vitesse = Mathf.Exp(Random.Range(Mathf.Log(vitesseMin), Mathf.Log(vitesseMax)));
	}
	
	// Update is called once per frame
	void Update () {
        // Si le temps est freeze, on ne fait rien
        if(GameManager.Instance.timeFreezed) {
            return;
        }

        GetEtat();

        // Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
        // Sinon on le pourchasse !
        if (Time.timeSinceLevelLoad >= tempsInactifDebutJeu) {
            switch (etat) {
                case EtatEnnemi.WAITING:
                    // On va là où on a vu le joueur pour la dernière fois !
                    Vector3 move = Move(lastPositionSeen);

                    // Si le mouvement est trop petit, c'est que l'on est bloqué, donc on arrête le mouvement en lui ordonnant d'aller sur place
                    if (Vector3.Magnitude(move) <= 0.001f && Vector3.Magnitude(move) != 0f)
                    {
                        lastPositionSeen = transform.position;
                    }
                    break;

                case EtatEnnemi.TRACKING:
                    // Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
                    Move(player.transform.position);

                    // Et on retient la position actuelle du joueur !
                    lastPositionSeen = player.transform.position;
                    break;
            }
        }

        // On pousse !
		if (Time.timeSinceLevelLoad - debutPousee < tempsPousee) {
            controller.Move(pousee * Time.deltaTime);
		}
	}

    // On récupère l'état dans lequel doit être notre sonde
    void GetEtat() {
        if(IsPlayerVisible()) {
            EtatEnnemi previousEtat = etat;
            etat = EtatEnnemi.TRACKING;
            // Si la sonde vient juste de le repérer, on l'annonce
            if(etat != previousEtat)
                gm.console.JoueurDetecte(name);
        } else {
            EtatEnnemi previousEtat = etat;
            etat = EtatEnnemi.WAITING;
            // Si la sonde vient juste de perdre sa trace, on l'annonce
            if (etat != previousEtat)
                gm.console.JoueurPerduDeVue(name);
        }
    }

    // Permet de savoir si la sonde voit le joueur
    public bool IsPlayerVisible() {
        // Si l'ennemie est suffisament proche et qu'il est visible !
        RaycastHit hit;
        Ray ray = new Ray (transform.position, player.transform.position - transform.position);
        return Physics.Raycast(ray, out hit, distanceDeDetection) && hit.collider.name == "Joueur";
    }

	// Permet de savoir si le drone est en mouvement
	public bool IsMoving() {
		return (etat == EtatEnnemi.TRACKING || etat == EtatEnnemi.RUSHING) || Vector3.Distance(lastPositionSeen, transform.position) > 1f;
	}

	// On vérifie si on a touché le joueur !!!
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
			// Si c'est le cas, on l'envoie ballader !
			Vector3 directionPoussee = hit.collider.transform.position - transform.position;
			directionPoussee.Normalize();
            player.EtrePoussee(directionPoussee * puissancePoussee, tempsPouseePersonnage);

			// Et on affiche un message dans la console !
			if (!gm.partieDejaTerminee) {
				gm.console.JoueurTouche();
			}
		}
	}

    // Permet à d'autres éléments de pousser la sonde
	public void EtrePoussee(Vector3 directionPoussee, float tempsDeLaPousee) {
		pousee = directionPoussee;
		tempsPousee = tempsDeLaPousee;
		debutPousee = Time.timeSinceLevelLoad;
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
}
