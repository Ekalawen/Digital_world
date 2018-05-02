using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleScript : MonoBehaviour {

	public enum TypeText {BASIC_TEXT, ENNEMI_TEXT, ALLY_TEXT};

	public Color basicColor; // La couleur avec laquelle on écrit la plupart du temps
	public Color ennemiColor; // La couleur des messages ennemis
	public Color allyColor; // La couleur des messages alliées
	public Font font; // La police de charactère des messages
	public GameObject textContainer; // Là où l'on va afficher les lignes
	public Text importantText; // Là où l'on affiche les informations importantes
	public float probaPhraseRandom;
	public int tailleTexte; // La taille du texte affiché

	private List<GameObject> lines; // Les lignes de la console, constitués d'un RectTransform et d'un Text
	private List<int> numLines; // Les numéros de lignes
	private GameObject player;
	private float lastTimeImportantText;
	private float tempsImportantText;
	private float lastOrbeAttrapee; // Le dernier temps auquel le joueur n'a pas attrapé d'orbe

	// Use this for initialization
	void Start () {
		name = "Console";
		lines = new List<GameObject> ();
		numLines = new List<int> ();
		player = GameObject.Find ("Joueur");
		importantText.text = "";

		// Les premiers messages
		ajouterMessage ("Chargement de la Matrix ...", TypeText.BASIC_TEXT);
		ajouterMessage ("Détection des Bases de Données ...", TypeText.BASIC_TEXT);
		ajouterMessageImportant (GameObject.Find("GameManager").GetComponent<GamaManagerScript>().nbLumieres + " Datas trouvées ! À vous de jouer !", TypeText.ALLY_TEXT, 5);
		ajouterMessage ("DÉTECTION INTRUSION ...", TypeText.ENNEMI_TEXT);
		ajouterMessage ("ANTI-VIRUS ACTIVÉS DANS 5 SECONDES !", TypeText.ENNEMI_TEXT);
		ajouterMessage ("On détecte " + GameObject.Find("GameManager").GetComponent<GamaManagerScript>().nbEnnemis + " Sondes !", TypeText.ALLY_TEXT);
	}
	
	// Update is called once per frame
	void Update () {
		// On regarde si le joueur n'est pas trop haut en altitude
		if (Time.time >= 10f && player.transform.position.y > GameObject.Find("GameManager").GetComponent<GamaManagerScript>().tailleMap + 3) {
			ajouterMessage ("Altitude critique !", TypeText.BASIC_TEXT);
		}

		// On lance des phrases random desfois !
		if (Random.Range (0f, 1f) < probaPhraseRandom) {
			lancerPhraseRandom ();
		}

		// On efface l'important texte si ça fait suffisamment longtemps qu'il est affiché
		if (Time.time - lastTimeImportantText > tempsImportantText) {
			importantText.text = "";
		}

		// On conseille d'appuyer sur TAB si le joueur galère a trouver des orbes
		if (Time.time - lastOrbeAttrapee > 30) {
			lastOrbeAttrapee = Time.time;
			ajouterMessage ("On peut te géolocaliser les Datas si tu appuies sur TAB !", TypeText.ALLY_TEXT);
		}
	}

	public void updateLastOrbeAttrapee() {
		lastOrbeAttrapee = Time.time;
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
		lastTimeImportantText = Time.time;
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
		phrases.Add ("Durée d'existence de la Matrix : " + Time.time);
		phrases.Add ("Numéro d'identification de la Matrix : " + Random.Range (0, 10000000));
		phrases.Add ("Il y a une sonde dernière toi ! Non je déconne :D");
		phrases.Add ("Tu as oublié les touches ? RTFM !");
		phrases.Add ("Récursif ou Itératif ?");
		phrases.Add ("POO = Parfaite Optimisation Originelle");
		phrases.Add ("Il y a " + GameObject.Find ("GameManager").GetComponent<GamaManagerScript> ().nbEnnemis + " Sondes à votre recherche.");

		string phrase = phrases [Random.Range (0, phrases.Count)];
		ajouterMessage (phrase, TypeText.BASIC_TEXT);
	}
}
