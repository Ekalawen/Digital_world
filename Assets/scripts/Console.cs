using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// La Console a pour but de gérer l'interface graphique de l'utilisateur.
// Elle gérera notemment tous les affichages dans le Terminal du personnage.
public class Console : MonoBehaviour {

	public enum TypeText {BASIC_TEXT, ENNEMI_TEXT, ALLY_TEXT};

	public Color basicColor; // La couleur avec laquelle on écrit la plupart du temps
	public Color ennemiColor; // La couleur des messages ennemis
	public Color allyColor; // La couleur des messages alliées
	public Font font; // La police de charactère des messages
	public GameObject textContainer; // Là où l'on va afficher les lignes
	public int tailleTexte; // La taille du texte affiché
	public float probaPhraseRandom; // La probabilité de générer une phrase aléatoire dans la console
	public Text importantText; // Là où l'on affiche les informations importantes

	[HideInInspector]
	public GameManager gm;
	[HideInInspector]
	public MapManager mapManager;
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public EventManager eventManager;
	private List<GameObject> lines; // Les lignes de la console, constitués d'un RectTransform et d'un Text
	private List<int> numLines; // Les numéros de lignes
	private float lastTimeImportantText;
	private float tempsImportantText;
	private float timeLastLumiereAttrapee; // Le dernier temps auquel le joueur n'a pas attrapé d'Orbe

	void Start () {
	}

    public void Initialize() {
		// Initialisation des variables
		name = "Console";
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
		lines = new List<GameObject> ();
		numLines = new List<int> ();
		player = GameObject.Find ("Joueur");
		importantText.text = "";
		eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();

		// Les premiers messages
		PremiersMessages();
    }

	public void PremiersMessages() {
		AjouterMessage ("Chargement de la Matrix ...", TypeText.BASIC_TEXT);
		AjouterMessage ("Détection des Bases de Données ...", TypeText.BASIC_TEXT);
		AjouterMessageImportant (mapManager.lumieres.Count + " Datas trouvées ! À vous de jouer !", TypeText.ALLY_TEXT, 5);
		AjouterMessage ("DÉTECTION INTRUSION ...", TypeText.ENNEMI_TEXT);
		AjouterMessage ("ANTI-VIRUS ACTIVÉS DANS 5 SECONDES !", TypeText.ENNEMI_TEXT);
		AjouterMessage ("On détecte " + gm.ennemiManager.sondes.Count + " Sondes !", TypeText.ALLY_TEXT);
	}
	
	void Update () {
		// On regarde si le joueur n'est pas trop haut en altitude
		if (Time.timeSinceLevelLoad >= 10f
		&& player.transform.position.y > mapManager.tailleMap + 3) {
			AjouterMessage ("Altitude critique !", TypeText.BASIC_TEXT);
		}

		// On lance des phrases random desfois !
		if (Random.Range (0f, 1f) < probaPhraseRandom) {
			LancerPhraseRandom ();
		}

		// On efface l'important texte si ça fait suffisamment longtemps qu'il est affiché
		if (Time.timeSinceLevelLoad - lastTimeImportantText > tempsImportantText) {
			importantText.text = "";
		}

        // On conseille d'appuyer sur TAB si le joueur galère a trouver des orbes
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (mapManager.lumieres.Count > 0) {
			if (Time.timeSinceLevelLoad - timeLastLumiereAttrapee > 30) {
				timeLastLumiereAttrapee = Time.timeSinceLevelLoad;
				AjouterMessage ("On peut te géolocaliser les Datas si tu appuies sur E !", TypeText.ALLY_TEXT);
			}
		}

	}

	public void UpdateLastLumiereAttrapee() {
		timeLastLumiereAttrapee = Time.timeSinceLevelLoad;
	}

	public void AjouterMessageImportant(string message, TypeText type, float tempsAffichage) {
		// Déjà on affiche le message dans la console
		AjouterMessage (message, type);

		// Et en plus on l'affiche en gros !
		importantText.text = message;
		switch (type) {
		case TypeText.BASIC_TEXT:
			importantText.color = basicColor;
			break;
		case TypeText.ENNEMI_TEXT:
			importantText.color = ennemiColor;
			break;
		case TypeText.ALLY_TEXT:
			importantText.color = allyColor;
			break;
		}
		lastTimeImportantText = Time.timeSinceLevelLoad;
		tempsImportantText = tempsAffichage;
	}

	public void AjouterMessage(string message, TypeText type) {
		// On crée le nouveau texte !
		GameObject newText = new GameObject(message, typeof(RectTransform));

		// On définit son parent
		newText.transform.SetParent (textContainer.transform);

		// On définit sa position !
		RectTransform rt = newText.GetComponent<RectTransform> ();
		rt.anchoredPosition = new Vector2 (150, tailleTexte + 2);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.zero;
		rt.sizeDelta = new Vector2 (290, tailleTexte + 2);
		rt.localScale = new Vector3 (1, 1, 1);

		// On définit le texte
		Text text = newText.AddComponent<Text> ();
		text.text = message;
		text.font = font;
		text.alignment = TextAnchor.LowerLeft;
		text.fontSize = tailleTexte;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
		switch (type) {
		case TypeText.BASIC_TEXT:
			text.color = basicColor;
			break;
		case TypeText.ENNEMI_TEXT:
			text.color = ennemiColor;
			break;
		case TypeText.ALLY_TEXT:
			text.color = allyColor;
			break;
		}

		// On augmente tous les numéros de lignes
		List<int> toDelete = new List<int>();
		for (int i = 0; i < numLines.Count; i++) {
			numLines [i] += 1;
			if (numLines [i] * (tailleTexte+2) >= 150) {
				toDelete.Add (i);
			}
		}
		foreach (int i in toDelete) {
			numLines.RemoveAt (i);
			Destroy (lines [i]);
			lines.RemoveAt (i);
		}

		// Et on met à jour leur hauteur d'affichage en conséquence !
		for (int i = 0; i < lines.Count; i++) {
			lines [i].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (150, (tailleTexte + 2) * numLines [i]);
		}

		// On l'ajoute à la liste de lines !
		lines.Add(newText);
		numLines.Add (1);
	}

	void LancerPhraseRandom() {
		List<string> phrases = new List<string> ();
		phrases.Add ("Wow vous êtes en forme !");
		phrases.Add ("Happy Hacking ! :D");
		phrases.Add ("pos=" + player.transform.position + " rot=" + player.transform.rotation);
		phrases.Add ("On rajoute le plugin du double-saut bientôt :p");
		phrases.Add ("Instruction 1 : utilisez les sondes pour sauter plus haut.");
		phrases.Add ("Instruction 2 : Ne regardez pas une sonde dans les yeux.");
		phrases.Add ("Instruction 3 : Faites attention quand vous rentrez dans une grotte ...");
		phrases.Add ("Instruction 3 : Restez toujours en mouvement !");
		phrases.Add ("Durée d'existence de la Matrix : " + Time.timeSinceLevelLoad);
		phrases.Add ("Numéro d'identification de la Matrix : " + Random.Range (0, 10000000));
		phrases.Add ("Il y a une sonde dernière toi ! Non je déconne :D");
		phrases.Add ("Tu as oublié les touches ? RTFM !");
		phrases.Add ("Récursif ou Itératif ?");
		phrases.Add ("POO = Parfaite Optimisation Originelle");
		phrases.Add ("Il y a " + mapManager.nbEnnemis + " Sondes à votre recherche.");

		string phrase = phrases [Random.Range (0, phrases.Count)];
		AjouterMessage (phrase, TypeText.BASIC_TEXT);
	}

	// Lorsqu'une sonde repère le joueur
	public void JoueurDetecte(string nomDetecteur) {
		AjouterMessage (nomDetecteur + " vous a détecté, je sais où vous êtes.", Console.TypeText.ENNEMI_TEXT);
	}
	
	// Lorsqu'une sonde perd le joueur de vue
	public void JoueurPerduDeVue(string nomDetecteur) {
		AjouterMessage ("On a déconnecté " + nomDetecteur + ".", Console.TypeText.ALLY_TEXT);
	}

	// Lorsque le joueur est touché
	public void JoueurTouche() {
		AjouterMessageImportant ("TOUCHÉ ! Je vais vous avoir !", Console.TypeText.ENNEMI_TEXT, 1);
	}

	// Lorsque toutes les lumieres ont été attrapés
	public void ToutesLesLumieresAttrapees() {
		AjouterMessage ("Vous ne vous en sortirez pas comme ça !", Console.TypeText.ENNEMI_TEXT);
		AjouterMessage ("OK Super maintenant faut sortir d'ici !", Console.TypeText.ALLY_TEXT);
	}

	// Lorsque le joueur est éjecté
	public void JoueurEjecte() {
		AjouterMessageImportant ("MENACE ÉJECTÉE !", Console.TypeText.ENNEMI_TEXT, 5);
		AjouterMessage ("Désactivation du processus défensif ...", Console.TypeText.ENNEMI_TEXT);
		AjouterMessage ("Vous avez été éjecté de la Matrix. ", Console.TypeText.BASIC_TEXT);
		StartCoroutine (SeMoquer ());
	}

	// Lorsque le joueur réussi à s'échapper
	public void JoueurEchappe() {
		AjouterMessage ("NOOOOOOOOOOOOOOOOOON ...", Console.TypeText.ENNEMI_TEXT);
		AjouterMessage ("J'ai échouée ...", Console.TypeText.ENNEMI_TEXT);
		AjouterMessageImportant ("NOUS AVONS RÉUSSI !!!", Console.TypeText.ALLY_TEXT, 5);
		StartCoroutine (Recompenser ());
	}

	// Text de récompense
	IEnumerator Recompenser() {
		yield return new WaitForSeconds (2);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < Random.Range (0, 6); i++)
				message += " ";
			message += "On a réussi YES";
			for (int i = 0; i < Random.Range (3, 12); i++)
				message += "S";
			message += " ";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "!";
			AjouterMessage (message, Console.TypeText.ALLY_TEXT);
			yield return null;
		}
	}

	// Lorsque le joueur a été bloqué par les drones
	public void JoueurCapture() {
		AjouterMessageImportant ("MENACE ÉLIMINÉE !", Console.TypeText.ENNEMI_TEXT, 5);
		StartCoroutine (SeMoquer ());
	}

	// On se moque de lui
	IEnumerator SeMoquer() {
		yield return new WaitForSeconds (2);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < Random.Range (0, 6); i++)
				message += " ";
			message += "Nous vous avons attrapé ... MOU";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "HA";
			message += " ";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "!";
			AjouterMessage (message, Console.TypeText.ENNEMI_TEXT);
			yield return null;
		}
	}

	// Quand on attrape une lumière
	public void AttraperLumiere(int nbLumieresRestantes) {
		AjouterMessage ("ON A DES INFOS !", Console.TypeText.ALLY_TEXT);
		if (nbLumieresRestantes > 0) {
			AjouterMessageImportant ("Plus que " + nbLumieresRestantes + " !", Console.TypeText.ALLY_TEXT, 2);
		} else {
			AjouterMessage ("ON LES A TOUTES !", Console.TypeText.ALLY_TEXT);
			AjouterMessageImportant ("FAUT SE BARRER MAINTENANT !!!", Console.TypeText.ALLY_TEXT, 2);
		}
	}

	// Quand le joueur lance les trails
	public void RunLocalisation() {
		int nbLumieresRestantes = mapManager.lumieres.Count;
		if(nbLumieresRestantes > 0) {
			AjouterMessage ("On t'envoie les données ! Il te restes " + nbLumieresRestantes + " objectifs !", Console.TypeText.ALLY_TEXT);
		} else {
			AjouterMessage ("On a hacké toute la base, faut s'enfuir maintenant !", Console.TypeText.ALLY_TEXT);
		}
	}

    // Quand on essaye de faire une localisation alors qu'on peut pas !
    public void FailLocalisation() {
		AjouterMessage ("Ils brouillent le réseau, objectifs introuvables !", Console.TypeText.ALLY_TEXT);
    }

	// Quand le joueur attérit d'un grand saut
	public void GrandSaut(float hauteurSaut) {
		AjouterMessage("Wow quel saut ! " + ((int) hauteurSaut) + " mètres !", Console.TypeText.BASIC_TEXT);
		AjouterMessage("duree = " + Time.timeSinceLevelLoad, TypeText.BASIC_TEXT);
	}

	// Quand le joueur réussit à semer toutes les sondes
	public void SemerSondes() {
		AjouterMessageImportant ("On les a semés, on est plus suivi !", TypeText.ALLY_TEXT, 2f);
	}

	// Quand le joueur se fait voir au début par les sondes !
	public void JoueurRepere() {
		AjouterMessageImportant ("Nous t'avons trouvé !", TypeText.ENNEMI_TEXT, 2f);
	}

    // Lorsque le joueur tente de construire un pont avec une cible invalide !
    public void PouvoirBridgeBuilderInvalide() {
        AjouterMessageImportant("Ce n'est pas une cible valide !", TypeText.ENNEMI_TEXT, 1f);
    }
}
