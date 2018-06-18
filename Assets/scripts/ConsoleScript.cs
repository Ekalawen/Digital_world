using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// La Console a pour but de gérer l'interface graphique de l'utilisateur.
// Elle gérera notemment tous les affichages dans le Terminal du personnage.
public class ConsoleScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ENUMERATION
	//////////////////////////////////////////////////////////////////////////////////////

	public enum TypeText {BASIC_TEXT, ENNEMI_TEXT, ALLY_TEXT};

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public Color basicColor; // La couleur avec laquelle on écrit la plupart du temps
	public Color ennemiColor; // La couleur des messages ennemis
	public Color allyColor; // La couleur des messages alliées
	public Font font; // La police de charactère des messages
	public GameObject textContainer; // Là où l'on va afficher les lignes
	public int tailleTexte; // La taille du texte affiché
	public float probaPhraseRandom; // La probabilité de générer une phrase aléatoire dans la console
	public Text importantText; // Là où l'on affiche les informations importantes

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVEES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public GameManagerScript gameManager;
	[HideInInspector]
	public MapManagerScript mapManager;
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public DataBaseScript dataBase;
	private List<GameObject> lines; // Les lignes de la console, constitués d'un RectTransform et d'un Text
	private List<int> numLines; // Les numéros de lignes
	private float lastTimeImportantText;
	private float tempsImportantText;
	private float lastOrbeAttrapee; // Le dernier temps auquel le joueur n'a pas attrapé d'Orbe

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisation des variables
		name = "Console";
		gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManagerScript>();
		lines = new List<GameObject> ();
		numLines = new List<int> ();
		player = GameObject.Find ("Joueur");
		importantText.text = "";
		dataBase = GameObject.Find("DataBase").GetComponent<DataBaseScript>();

		// Les premiers messages
		premiersMessages();
	}

	public void premiersMessages() {
		ajouterMessage ("Chargement de la Matrix ...", TypeText.BASIC_TEXT);
		ajouterMessage ("Détection des Bases de Données ...", TypeText.BASIC_TEXT);
		ajouterMessageImportant (mapManager.nbLumieres + " Datas trouvées ! À vous de jouer !", TypeText.ALLY_TEXT, 5);
		ajouterMessage ("DÉTECTION INTRUSION ...", TypeText.ENNEMI_TEXT);
		ajouterMessage ("ANTI-VIRUS ACTIVÉS DANS 5 SECONDES !", TypeText.ENNEMI_TEXT);
		ajouterMessage ("On détecte " + mapManager.nbEnnemis + " Sondes !", TypeText.ALLY_TEXT);
	}
	
	void Update () {
		// On regarde si le joueur n'est pas trop haut en altitude
		if (Time.timeSinceLevelLoad >= 10f
		&& player.transform.position.y > mapManager.tailleMap + 3) {
			ajouterMessage ("Altitude critique !", TypeText.BASIC_TEXT);
		}

		// On lance des phrases random desfois !
		if (Random.Range (0f, 1f) < probaPhraseRandom) {
			lancerPhraseRandom ();
		}

		// On efface l'important texte si ça fait suffisamment longtemps qu'il est affiché
		if (Time.timeSinceLevelLoad - lastTimeImportantText > tempsImportantText) {
			importantText.text = "";
		}

		// On conseille d'appuyer sur TAB si le joueur galère a trouver des orbes
		if (Time.timeSinceLevelLoad - lastOrbeAttrapee > 30) {
			lastOrbeAttrapee = Time.timeSinceLevelLoad;
			ajouterMessage ("On peut te géolocaliser les Datas si tu appuies sur E !", TypeText.ALLY_TEXT);
		}

		// On vérifie si le joueur est suivi ou pas
		bool nonSuivi = dataBase.joueurSuivi();
		if (Time.timeSinceLevelLoad >= 10 && nonSuivi && player.GetComponent<PersonnageScript>().vu == true) {
			ajouterMessageImportant ("On les a semés, on est plus suivi !", TypeText.ALLY_TEXT, 2f);
			player.GetComponent<PersonnageScript> ().vu = false;
		}
	}

	public void updateLastOrbeAttrapee() {
		lastOrbeAttrapee = Time.timeSinceLevelLoad;
	}

	public void ajouterMessageImportant(string message, TypeText type, float tempsAffichage) {
		// Déjà on affiche le message dans la console
		ajouterMessage (message, type);

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

	public void ajouterMessage(string message, TypeText type) {
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

	void lancerPhraseRandom() {
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
		ajouterMessage (phrase, TypeText.BASIC_TEXT);
	}

	// Lorsqu'une sonde repère le joueur
	public void joueurDetecte(string nomDetecteur) {
		ajouterMessage (nomDetecteur + " vous a détecté, je sais où vous êtes.", ConsoleScript.TypeText.ENNEMI_TEXT);
	}
	
	// Lorsqu'une sonde perd le joueur de vue
	public void joueurPerduDeVue(string nomDetecteur) {
		ajouterMessage ("On a déconnecté " + nomDetecteur + ".", ConsoleScript.TypeText.ALLY_TEXT);
	}

	// Lorsque le joueur est touché
	public void joueurTouche() {
		ajouterMessageImportant ("TOUCHÉ ! Je vais vous avoir !", ConsoleScript.TypeText.ENNEMI_TEXT, 1);
	}

	// Lorsque toutes les lumieres ont été attrapés
	public void toutesLumieresAttrapees() {
		ajouterMessage ("Vous ne vous en sortirez pas comme ça !", ConsoleScript.TypeText.ENNEMI_TEXT);
		ajouterMessage ("OK Super maintenant faut sortir d'ici !", ConsoleScript.TypeText.ALLY_TEXT);
	}

	// Lorsque le joueur est éjecté
	public void joueurEjecte() {
		ajouterMessageImportant ("MENACE ÉJECTÉE !", ConsoleScript.TypeText.ENNEMI_TEXT, 5);
		ajouterMessage ("Désactivation du processus défensif ...", ConsoleScript.TypeText.ENNEMI_TEXT);
		ajouterMessage ("Vous avez été éjecté de la Matrix. ", ConsoleScript.TypeText.BASIC_TEXT);
		StartCoroutine (seMoquer ());
	}

	// Lorsque le joueur réussi à s'échapper
	public void joueurEchappe() {
		ajouterMessage ("NOOOOOOOOOOOOOOOOOON ...", ConsoleScript.TypeText.ENNEMI_TEXT);
		ajouterMessage ("J'ai échouée ...", ConsoleScript.TypeText.ENNEMI_TEXT);
		ajouterMessageImportant ("NOUS AVONS RÉUSSI !!!", ConsoleScript.TypeText.ALLY_TEXT, 5);
		StartCoroutine (recompenser ());
	}

	// Text de récompense
	IEnumerator recompenser() {
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
			ajouterMessage (message, ConsoleScript.TypeText.ALLY_TEXT);
			yield return null;
		}
	}

	// Lorsque le joueur a été bloqué par les drones
	public void joueurCapture() {
		ajouterMessageImportant ("MENACE ÉLIMINÉE !", ConsoleScript.TypeText.ENNEMI_TEXT, 5);
		StartCoroutine (seMoquer ());
	}

	// On se moque de lui
	IEnumerator seMoquer() {
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
			ajouterMessage (message, ConsoleScript.TypeText.ENNEMI_TEXT);
			yield return null;
		}
	}

	// Quand on attrape une lumière
	public void attraperLumiere(int nbLumieresRestantes) {
		ajouterMessage ("ON A DES INFOS !", ConsoleScript.TypeText.ALLY_TEXT);
		if (nbLumieresRestantes > 0) {
			ajouterMessageImportant ("Plus que " + nbLumieresRestantes + " !", ConsoleScript.TypeText.ALLY_TEXT, 2);
		} else {
			ajouterMessage ("ON LES A TOUTES !", ConsoleScript.TypeText.ALLY_TEXT);
			ajouterMessageImportant ("FAUT SE BARRER MAINTENANT !!!", ConsoleScript.TypeText.ALLY_TEXT, 2);
		}
	}

	// Quand le joueur lance les trails
	public void envoyerTrails() {
		ajouterMessage ("On t'envoie les données !", ConsoleScript.TypeText.ALLY_TEXT);
	}

	// Quand le joueur attérit d'un grand saut
	public void grandSaut(float hauteurSaut) {
		ajouterMessage("Wow quel saut ! " + ((int) hauteurSaut) + " mètres !", ConsoleScript.TypeText.BASIC_TEXT);
	}

}
