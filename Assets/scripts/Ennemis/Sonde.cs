using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonde : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ENUMERATION
	//////////////////////////////////////////////////////////////////////////////////////

	public enum EtatEnnemi {WAITING, TRACKING, DEFENDING, RUSHING};

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public float vitesseMin; // On veut une vitesse aléatoire comprise
	public float vitesseMax; // entre min et max !
	public float puissancePoussee; // La force avec laquelle on va pousser le joueur en cas de contact !
	public float tempsPouseePersonnage; // Le temps pendant lequel le personnage est poussé !
	public float distanceDeDetection; // La distance à partir de laquelle le probe peut pourchasser l'ennemi
	public float coefficiantDeRushVitesse; // Le multiplicateur de vitesse lorsque les drones rushs
	public float coefficiantDeRushDistanceDeDetection; // Le multiplicateur de la portée de détection quand on est en rush

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public GameManager gameManager;
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public CharacterController controller;
	[HideInInspector]
	public DataBase dataBase; // La dataBase qui envoie les ordres
	[HideInInspector]
	public Console console; // la console
	[HideInInspector]
	private EtatEnnemi etat;
	private Vector3 lastPositionSeen; // La dernière position à laquelle le joueur a été vu !
	private float vitesse;
    [HideInInspector]
    public Vector3 positionGrilleDefense; // La position de la grille de défense !

	private Vector3 pousee; // Lorsque le personnage est poussé
	private float debutPousee; // Le début de la poussée
	private float tempsPousee; // Le temps pendant lequel le personnage reçoit cette poussée


	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisation
		name = "Sonde_" + Random.Range (0, 9999);
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		player = GameObject.Find ("Joueur");
		controller = this.GetComponent<CharacterController> ();
        dataBase = DataBase.Instance;
		console = GameObject.Find ("Console").GetComponent<Console> ();
		etat = EtatEnnemi.WAITING;
		lastPositionSeen = transform.position;
		vitesse = Mathf.Exp(Random.Range (Mathf.Log(vitesseMin), Mathf.Log(vitesseMax)));

        // L'ennemi doit se renseigner auprès de la database !
        dataBase.sondes.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
        // Si le temps est freeze, on ne fait rien
        if(GameManager.Instance.timeFreezed) {
            return;
        }

        getEtat();

		// Tant que l'ennemi n'est pas visible et/ou trop proche, on reste sur place
		// Sinon on le pourchasse !
		Vector3 direction;
		Vector3 finalMouvement;
		switch (etat) {
		case EtatEnnemi.WAITING:
			// On va là où on a vu le joueur pour la dernière fois !
			direction = lastPositionSeen - transform.position;
			direction.Normalize ();

			// Si c'est trop long, on ajuste
			if (Vector3.Magnitude(direction * vitesse * Time.deltaTime) > Vector3.Magnitude(lastPositionSeen - transform.position)) {
				finalMouvement = lastPositionSeen - transform.position;
			} else {
				finalMouvement = direction * vitesse * Time.deltaTime;
			}
			controller.Move(finalMouvement);

			// Si le mouvement est trop petit, c'est que l'on est bloqué, donc on arrête le mouvement
			if(Vector3.Magnitude(finalMouvement) <= 0.001f && Vector3.Magnitude(finalMouvement) != 0f) {
				lastPositionSeen = transform.position;
			}
			break;

		case EtatEnnemi.TRACKING:
			// Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
			direction = player.transform.position - transform.position;
			direction.Normalize ();

			controller.Move (direction* vitesse * Time.deltaTime);
			// Et on retient la position actuelle du joueur !
			lastPositionSeen = player.transform.position;
			break;

		case EtatEnnemi.DEFENDING:
            // En défensif, dès qu'on perd le joueur de vue, on rentre à la maison :D
            // On va tout en haut pour empêcher le joueur de sortir !
            Vector3 cible = positionGrilleDefense;
            direction = cible - transform.position;
			direction.Normalize ();
			direction *= coefficiantDeRushVitesse;

			// Si c'est trop long, on ajuste
			if (Vector3.Magnitude(direction * vitesse * Time.deltaTime) > Vector3.Magnitude(cible - transform.position)) {
				finalMouvement = cible - transform.position;
			} else {
				finalMouvement = direction * vitesse * Time.deltaTime;
			}
			controller.Move(finalMouvement);

			// Si le mouvement est trop petit, c'est que l'on est bloqué, donc on arrête le mouvement
			if(Vector3.Magnitude(finalMouvement) <= 0.01f && Vector3.Magnitude(finalMouvement) != 0f) {
				Debug.Log("On arrête les petits mouvements");
				lastPositionSeen = transform.position;
			}
			break;

		case EtatEnnemi.RUSHING:
			// Les ennemis se déplacent toujours vers le joueur de façon tout à fait linéaire !		
			direction = player.transform.position - transform.position;
			direction.Normalize ();
			direction*= coefficiantDeRushVitesse;

			controller.Move (direction* vitesse * Time.deltaTime);
			// Et on retient la position actuelle du joueur !
			lastPositionSeen = player.transform.position;
			break;
		}

        // On pousse !
		if (Time.timeSinceLevelLoad - debutPousee < tempsPousee) {
            controller.Move(pousee * Time.deltaTime);
		}
	}

    // On récupère l'état dans lequel doit être notre sonde
    void getEtat() {
        DataBase.EtatDataBase etatDataBase = dataBase.demanderOrdre();
        switch(etatDataBase) {
            case DataBase.EtatDataBase.NORMAL:
                // On regarde si le joueur est visible
                if(Time.timeSinceLevelLoad >= 10f && isPlayerVisible(etatDataBase)) {
                    // Si la sonde vient juste de le repérer, on l'annonce
                    if (etat == EtatEnnemi.WAITING) {
                        console.joueurDetecte(name);
                    }
                    etat = EtatEnnemi.TRACKING;
                } else {
                    // Si la sonde vient juste de perdre sa trace, on l'annonce
                    if (etat == EtatEnnemi.TRACKING) {
                        console.joueurPerduDeVue(name);
                    }
                    etat = EtatEnnemi.WAITING;
                }
                break;
            case DataBase.EtatDataBase.DEFENDING:
                // On regarde si le joueur est visible
                if(isPlayerVisible(etatDataBase)) {
                    // Si la sonde vient juste de le repérer, on l'annonce
                    if (etat == EtatEnnemi.DEFENDING) {
                        console.joueurDetecte(name);
                    }
                    etat = EtatEnnemi.RUSHING;
                } else {
                    // Si la sonde vient juste de perdre sa trace, on l'annonce
                    if (etat == EtatEnnemi.RUSHING) {
                        console.joueurPerduDeVue(name);
                    }
                    etat = EtatEnnemi.DEFENDING;
                }
                break;
        }
    }

    // Permet de savoir si la sonde voit le joueur
    public bool isPlayerVisible(DataBase.EtatDataBase etatDataBase) {
        // On calcul la distance de détection
        float distance = distanceDeDetection;
        if(etatDataBase == DataBase.EtatDataBase.DEFENDING) {
            distance *= coefficiantDeRushDistanceDeDetection;
        }

        // Si l'ennemie est suffisament proche et qu'il est visible !
        RaycastHit hit;
        Ray ray = new Ray (transform.position, player.transform.position - transform.position);
        if(Physics.Raycast(ray, out hit, distance) && hit.collider.name == "Joueur") {
            return true;
        } else {
            return false;
        }
    }

	// Permet de savoir si le drone est en mouvement
	public bool isMoving() {
		return (etat == EtatEnnemi.TRACKING || etat == EtatEnnemi.RUSHING) || Vector3.Distance(lastPositionSeen, transform.position) > 1f;
	}

	// On vérifie si on a touché le joueur !!!
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.collider.name == "Joueur") {
			// Si c'est le cas, on l'envoie ballader !
			Vector3 directionPoussee = hit.collider.transform.position - transform.position;
			directionPoussee.Normalize ();
			hit.collider.GetComponent<Personnage> ().etrePoussee (directionPoussee * puissancePoussee, tempsPouseePersonnage);

			// Et on affiche un message dans la console !
			if (!gameManager.partieDejaTerminee) {
				console.joueurTouche();
			}
		}
	}

    // Permet à d'autres éléments de pousser la sonde
	public void etrePoussee(Vector3 directionPoussee, float tempsDeLaPousee) {
		pousee = directionPoussee;
		tempsPousee = tempsDeLaPousee;
		debutPousee = Time.timeSinceLevelLoad;
	}
}
