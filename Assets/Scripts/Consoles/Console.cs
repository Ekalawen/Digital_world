using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public struct TimerMessage {
    public TimedMessage message;
    public Timer timer;

    public TimerMessage(TimedMessage timedMessage) {
        this.message = timedMessage;
        this.timer = new Timer(timedMessage.timing);
    }
}

// La Console a pour but de gérer l'interface graphique de l'utilisateur.
// Elle gérera notemment tous les affichages dans le Terminal du personnage.
public class Console : MonoBehaviour {

	public enum TypeText {BASIC_TEXT, ENNEMI_TEXT, ALLY_TEXT};

	public Color basicColor; // La couleur avec laquelle on écrit la plupart du temps
	public Color ennemiColor; // La couleur des messages ennemis
	public Color allyColor; // La couleur des messages alliées
	public string basicPrefix; // Le préfixe à mettre devant chaque message basic
	public string ennemiPrefix; // Le préfixe à mettre devant chaque message ennemi
	public string allyPrefix; // Le préfixe à mettre devant chaque message allié
	public Font font; // La police de charactère des messages
	public GameObject textContainer; // Là où l'on va afficher les lignes
	public int tailleTexte; // La taille du texte affiché
	public Vector2 tempsAvantPhraseRandom; // Le temps avant de générer une phrase aléatoire dans la console
	public Vector2 tempsAvantConseiller; // Le temps avant de générer un conseil
	public Text importantText; // Là où l'on affiche les informations importantes
    public bool useAltitudeCritique = true; // Si on doit utiliser altitude critique ou pas dans cette partie !
    public List<string> conseils; // Les conseils à dispenser au joueur !
    public List<TimedMessage> timedMessages;

    public PouvoirDisplayInGame pouvoirDisplayA;
    public PouvoirDisplayInGame pouvoirDisplayE;
    public PouvoirDisplayInGame pouvoirDisplayLeftClick;
    public PouvoirDisplayInGame pouvoirDisplayRightClick;

	[HideInInspector]
	public GameManager gm;
	[HideInInspector]
	public MapManager map;
	[HideInInspector]
	public Player player;
	[HideInInspector]
	public EventManager eventManager;
	protected List<GameObject> lines; // Les lignes de la console, constitués d'un RectTransform et d'un Text
	protected List<int> numLines; // Les numéros de lignes
	protected float lastTimeImportantText;
	protected float tempsImportantText;
	protected float timeLastLumiereAttrapee; // Le dernier temps auquel le joueur n'a pas attrapé d'Orbe
    protected bool playerIsFollowed = false; // C'est pas vrai, mais c'est pour que l'algo fonctionne ^^
    protected Timer timerPhraseRandom;
    protected Timer timerConseiller;
    protected List<TimerMessage> timersMessages;

	void Start () {
	}

    public virtual void Initialize() {
		// Initialisation des variables
		name = "Console";
        gm = GameManager.Instance;
        map = gm.map;
		lines = new List<GameObject> ();
		numLines = new List<int> ();
        player = gm.player;
		importantText.text = "";
        eventManager = gm.eventManager;

		// Les premiers messages
		PremiersMessages();
        timerPhraseRandom = new Timer(UnityEngine.Random.Range(tempsAvantPhraseRandom.x, tempsAvantPhraseRandom.y));
        timerConseiller = new Timer(UnityEngine.Random.Range(tempsAvantConseiller.x, tempsAvantConseiller.y));

        // Setup les timers des timedMessages
        InitTimersMessages();

        // Init les pouvoirs displays
        InitPouvoirsDisplays();
    }

    protected void InitTimersMessages() {
        timersMessages = new List<TimerMessage>();
        foreach(TimedMessage tm in timedMessages) {
            timersMessages.Add(new TimerMessage(tm));
        }
    }

	public virtual void PremiersMessages() {
        string levelName = PlayerPrefs.GetString(MenuLevel.LEVEL_NAME_KEY);
		AjouterMessage ("[Niveau]: " + levelName, TypeText.BASIC_TEXT, bUsePrefix: false);
		AjouterMessage ("[Niveau]: Initialisation de la Matrix ...", TypeText.BASIC_TEXT, bUsePrefix: false);
		AjouterMessageImportant (map.GetLumieres().Count + " Datas trouvées !", TypeText.ALLY_TEXT, 5);
		AjouterMessage (gm.ennemiManager.ennemis.Count + " Ennemis détectés !", TypeText.ALLY_TEXT);

		//AjouterMessage ("Chargement de la Matrix ...", TypeText.BASIC_TEXT);
		//AjouterMessage ("Détection des Bases de Données ...", TypeText.BASIC_TEXT);
		//AjouterMessageImportant (map.lumieres.Count + " Datas trouvées ! À vous de jouer !", TypeText.ALLY_TEXT, 5);
		//AjouterMessage ("DÉTECTION INTRUSION ...", TypeText.ENNEMI_TEXT);
		//AjouterMessage ("ANTI-VIRUS ACTIVÉS DANS 5 SECONDES !", TypeText.ENNEMI_TEXT);
		//AjouterMessage ("On détecte " + gm.ennemiManager.ennemis.Count + " Ennemis !", TypeText.ALLY_TEXT);
	}

    public void PouvoirsDesactives() {
		AjouterMessageImportant ("Pouvoirs Désactivés !", TypeText.ALLY_TEXT, 3);
		AjouterMessageImportant ("Pouvoirs Désactivés !", TypeText.ENNEMI_TEXT, 3);
		AjouterMessageImportant ("Pouvoirs Désactivés !", TypeText.BASIC_TEXT, 3);
    }

    protected void LancerConseils() {
        if (timerConseiller.IsOver()) {
            Conseiller();
            timerConseiller = new Timer(UnityEngine.Random.Range(tempsAvantConseiller.x, tempsAvantConseiller.y));
        }
    }

    protected void EffacerImportantText() {
        if (Time.timeSinceLevelLoad - lastTimeImportantText > tempsImportantText) {
			importantText.text = "";
		}
    }

    protected void ConseillerUtiliserDetection() {
        if (map.GetLumieres().Count > 0) {
			if (Time.timeSinceLevelLoad - timeLastLumiereAttrapee > 25) {
				timeLastLumiereAttrapee = Time.timeSinceLevelLoad;
				AjouterMessage ("On peut te géolocaliser les Datas si tu appuies sur E ou A !", TypeText.ALLY_TEXT);
			}
		}
    }

    protected void DetectPlayerSafeOrNot() {
        if (!gm.eventManager.IsGameOver()) {
            bool oldState = playerIsFollowed;
            playerIsFollowed = gm.ennemiManager.IsPlayerFollowed();
            if (playerIsFollowed && !oldState) {
                JoueurDetecte();
            }
            if (!playerIsFollowed && oldState) {
                SemerEnnemis();
                gm.soundManager.PlayDissimuleClip();
            }
        }
    }

    protected virtual void RunTimedMessages() {
        for(int i = 0; i < timersMessages.Count; i++) {
            TimerMessage timerMessage = timersMessages[i];
            if(timerMessage.timer.IsOver()) {
                AjouterMessageImportant(timerMessage.message.message, timerMessage.message.type, timerMessage.message.duree);
                timersMessages.Remove(timerMessage);
                timedMessages.Remove(timerMessage.message);
                i--;
            }
        }
    }
	
	protected virtual void Update () {
        if(useAltitudeCritique)
            AltitudeCritique();

        //// On lance des phrases random desfois !
        //if (timerPhraseRandom.IsOver()) {
        //	LancerPhraseRandom ();
        //  timerPhraseRandom = new Timer(Random.Range(tempsAvantPhraseRandom.x, tempsAvantPhraseRandom.y));
        //}

        // On lance des conseils !
        LancerConseils();

        //// On efface l'important texte si ça fait suffisamment longtemps qu'il est affiché
        //EffacerImportantText();

        // On conseille d'appuyer sur TAB si le joueur galère a trouver des orbes
        ConseillerUtiliserDetection();

        // On détecte si le joueur est safe ou pas !
        DetectPlayerSafeOrNot();

        // On lance les messages timed
        RunTimedMessages();
	}

	public void UpdateLastLumiereAttrapee() {
		timeLastLumiereAttrapee = Time.timeSinceLevelLoad;
	}

    public void AjouterMessageImportant(
        TimedMessage timedMessage,
        bool bAfficherInConsole = true,
        string messageToReplace ="") {
        AjouterMessageImportant(
            timedMessage.message,
            timedMessage.type,
            timedMessage.duree,
            bAfficherInConsole,
            messageToReplace);
    }

    public void AjouterMessageImportant(
        string message,
        TypeText type,
        float tempsAffichage,
        bool bAfficherInConsole = true,
        string messageToReplace = "") {

		// Déjà on affiche le message dans la console
        if(bAfficherInConsole)
            AjouterMessage (message, type);

        int ind = EffacerImportantMessage(messageToReplace);

        AjouterImportantMessageAt(message, type, ind);

        StartCoroutine(CEffacerImportantTextIn(message, tempsAffichage + 0.01f)); // On attends un peu pour laisse le temps à d'autres systèmes de prendre la main ^^'
    }

    protected int EffacerImportantMessage(string messageToReplace) {
        List<string> lines = GetImportantTextLines();
        if (messageToReplace == "")
            return 0;
        for(int i = 0; i < lines.Count; i++) {
            if(lines[i].Contains(messageToReplace)) {
                lines.RemoveAt(i);
                importantText.text = String.Join("\n", lines);
                return i;
            }
        }
        return 0;
    }

    protected void AjouterImportantMessageAt(string message, TypeText type, int ind = 0) {
        List<string> lines = GetImportantTextLines();
        lines.Insert(ind, SurroundWithColor(message, type));
        importantText.text = String.Join("\n", lines);
    }

    protected string SurroundWithColor(string message, TypeText type) {
        string colorString = ColorUtility.ToHtmlStringRGBA(GetTextTypeColor(type));
        return string.Format("<color=#{0}>{1}</color>", colorString, message);
    }

    public Color GetTextTypeColor(TypeText type) {
        switch (type) {
            case TypeText.BASIC_TEXT:
                return basicColor;
            case TypeText.ENNEMI_TEXT:
                return ennemiColor;
            case TypeText.ALLY_TEXT:
                return allyColor;
        }
        throw new Exception("Couleur inconnue !");
    }

    protected IEnumerator CEffacerImportantTextIn(string message, float duree) {
        yield return new WaitForSeconds(duree);
        EffacerImportantMessage(message);
    }

    public List<string> GetImportantTextLines() {
        return importantText.text.Split('\n').OfType<string>().ToList();
    }

    public void AjouterMessage(
        TimedMessage timedMessage,
        bool bUsePrefix = true) {
        AjouterMessage(
            timedMessage.message,
            timedMessage.type,
            bUsePrefix);
    }


	public void AjouterMessage(string message, TypeText type, bool bUsePrefix = true) {
        // On préfixe ! :)
        if (bUsePrefix) {
            switch (type) {
                case TypeText.ALLY_TEXT:
                    message = allyPrefix + message;
                    break;
                case TypeText.BASIC_TEXT:
                    message = basicPrefix + message;
                    break;
                case TypeText.ENNEMI_TEXT:
                    message = ennemiPrefix + message;
                    break;
            }
        }

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
        AddBlankLine();

		// Et on met à jour leur hauteur d'affichage en conséquence !
		for (int i = 0; i < lines.Count; i++) {
			lines [i].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (150, (tailleTexte + 2) * numLines [i]);
		}

		// On l'ajoute à la liste de lines !
		lines.Add(newText);
		numLines.Add (1);

        // On s'assure que le message rentre dans la console ! <3
        int weightMax = 280;
        for(int i = 0; i < message.Length; i++) {
            text.text = message.Substring(0, i);
            if(text.preferredWidth > weightMax) {
                string nextMessage = message.Substring(i, message.Length - i);
                if(gm != null)
                    AddTimedMessageToHistory(text.text, type, gm.timerManager.GetElapsedTime(), 0.0f);
                AjouterMessage(nextMessage, type, bUsePrefix: false);
                return;
            }
        }
        text.text = message;

        // On sauvegarde le message si celui-ci a été sauvegardé ! :3
        if (gm != null)
            AddTimedMessageToHistory(message, type, gm.timerManager.GetElapsedTime(), 0.0f);
	}

    public void AddTimedMessageToHistory(string message, Console.TypeText type, float time, float duree) {
        TimedMessage tm = new GameObject().AddComponent<TimedMessage>();
        tm.Initialize(message, type, time, duree);
        gm.historyManager.AddConsoleMessage(tm);
    }

    public void AddBlankLine() {
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
    }

    public void CleanAllLines() {
        while (numLines.Count > 0)
            AddBlankLine();
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
		phrases.Add ("Numéro d'identification de la Matrix : " + UnityEngine.Random.Range (0, 10000000));
		phrases.Add ("Il y a une sonde dernière toi ! Non je déconne :D");
		phrases.Add ("Tu as oublié les touches ? RTFM !");
		phrases.Add ("Récursif ou Itératif ?");
		phrases.Add ("POO = Parfaite Optimisation Originelle");
        phrases.Add (gm.ennemiManager.ennemis.Count + " Ennemis détectés !");
        //for(int i = 0; i < gm.ennemiManager.nbEnnemis.Count; i++) {
        //    int nbEnnemis = gm.ennemiManager.nbEnnemis[i];
        //    string ennemisName = gm.ennemiManager.ennemisPrefabs[i].name;
        //    phrases.Add ("Il y a " + nbEnnemis + " " + ennemisName + " à votre recherche.");
        //}

		string phrase = phrases [UnityEngine.Random.Range (0, phrases.Count)];
		AjouterMessage (phrase, TypeText.BASIC_TEXT);
	}

    protected void Conseiller() {
        if (conseils.Count == 0)
            return;

		string phrase = conseils[UnityEngine.Random.Range (0, conseils.Count)];
		AjouterMessage(phrase, TypeText.BASIC_TEXT);
    }

    // Lorsqu'un ennemi repère le joueur
    public void JoueurDetecte() {
        string message = "DÉTECTÉ !";
        AjouterMessageImportant (message, Console.TypeText.ENNEMI_TEXT, 2, bAfficherInConsole: false, message);
        AjouterMessage ("Je vous ai détecté, je sais où vous êtes !", Console.TypeText.ENNEMI_TEXT);
	}
	//public void JoueurDetecte(string nomDetecteur) {
		//AjouterMessage (nomDetecteur + " vous a détecté, je sais où vous êtes.", Console.TypeText.ENNEMI_TEXT);
	//}

	// Quand le joueur réussit à semer toutes les sondes
	public void SemerEnnemis() {
        string message = "DISSIMULÉ !";
		AjouterMessageImportant (message, TypeText.ALLY_TEXT, 2f, bAfficherInConsole: false, message);
        AjouterMessage("On les a semés, on est plus suivi !", TypeText.ALLY_TEXT);
	}
	
	// Lorsqu'une sonde perd le joueur de vue
	public void JoueurPerduDeVue(string nomDetecteur) {
		//AjouterMessage ("On a déconnecté " + nomDetecteur + ".", Console.TypeText.ALLY_TEXT);
	}

	// Lorsque le joueur est touché
	public void JoueurToucheSonde() {
        string message = "TOUCHÉ !";
		AjouterMessageImportant (message, Console.TypeText.ENNEMI_TEXT, 1, bAfficherInConsole: false, message);
        AjouterMessage("TOUCHÉ par une Sonde !", Console.TypeText.ENNEMI_TEXT);
		//AjouterMessageImportant ("TOUCHÉ ! Je vais vous avoir !", Console.TypeText.ENNEMI_TEXT, 1);
	}

	// Lorsque le joueur est touché
	public void JoueurToucheTracer() {
        string message = "TOUCHÉ !";
		AjouterMessageImportant (message, Console.TypeText.ENNEMI_TEXT, 1, bAfficherInConsole: false, message);
        AjouterMessage("TOUCHÉ par un Tracer !", Console.TypeText.ENNEMI_TEXT);
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
	public void WinGame() {
		AjouterMessage ("NOOOOOOOOOOOOOOOOOON ...", Console.TypeText.ENNEMI_TEXT);
		AjouterMessage ("J'ai échouée ...", Console.TypeText.ENNEMI_TEXT);
		AjouterMessageImportant ("NOUS AVONS RÉUSSI !!!", Console.TypeText.ALLY_TEXT, 5);
		StartCoroutine (Recompenser ());
	}

	// Text de récompense
	IEnumerator Recompenser() {
		yield return new WaitForSeconds (4);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < UnityEngine.Random.Range (0, 6); i++)
				message += " ";
			message += "On a réussi YES";
			for (int i = 0; i < UnityEngine.Random.Range (3, 12); i++)
				message += "S";
			message += " ";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				message += "!";
			AjouterMessage (message, Console.TypeText.ALLY_TEXT);
			yield return null;
		}
	}

	// Lorsque le joueur a été bloqué par les drones
	public void LoseGame(EventManager.DeathReason reason) {
        switch(reason) {
            case EventManager.DeathReason.CAPTURED:
                AjouterMessageImportant ("MENACE CAPTURÉE !", Console.TypeText.ENNEMI_TEXT, 5);
                AjouterMessage("Cause de la mort : Vous avez été capturé par un ennemi !", Console.TypeText.ENNEMI_TEXT);
                break;
            case EventManager.DeathReason.FALL_OUT:
                AjouterMessageImportant ("MENACE ÉJECTÉE !", Console.TypeText.ENNEMI_TEXT, 5);
                AjouterMessage("Cause de la mort : Vous êtes tombé !", Console.TypeText.ENNEMI_TEXT);
                break;
            case EventManager.DeathReason.TIME_OUT:
                AjouterMessageImportant ("MENACE DÉSYNCHRONISÉE !", Console.TypeText.ENNEMI_TEXT, 5);
                AjouterMessage("Cause de la mort : Vous n'aviez plus de temps !", Console.TypeText.ENNEMI_TEXT);
                break;
            case EventManager.DeathReason.TOUCHED_DEATH_CUBE:
                AjouterMessageImportant ("MENACE ÉLIMINÉE !", Console.TypeText.ENNEMI_TEXT, 5);
                AjouterMessage("Cause de la mort : Vous avez touché un Cube de la Mort !", Console.TypeText.ENNEMI_TEXT);
                break;
            case EventManager.DeathReason.OUT_OF_BLOCKS:
                AjouterMessageImportant ("MENACE ÉLIMINÉE !", Console.TypeText.ENNEMI_TEXT, 5);
                AjouterMessage("Cause de la mort : Vous avez été rattrapé par la Chute !", Console.TypeText.ENNEMI_TEXT);
                break;
        }
		StartCoroutine (SeMoquer());
	}

	// On se moque de lui
	IEnumerator SeMoquer() {
		yield return new WaitForSeconds (4);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < UnityEngine.Random.Range (0, 6); i++)
				message += " ";
			message += "Nous vous avons attrapé ... MOU";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				message += "HA";
			message += " ";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				message += "!";
			AjouterMessage (message, Console.TypeText.ENNEMI_TEXT);
			yield return null;
		}
	}

	// Quand on attrape une lumière
	public void AttraperLumiere(int nbLumieresRestantes) {
		//AjouterMessage ("ON A DES INFOS !", Console.TypeText.ALLY_TEXT);
		if (!gm.eventManager.IsEndGameStarted() && nbLumieresRestantes > 0) {
            string messageCurrent = "Plus que " + nbLumieresRestantes + " !";
            string messagePrecedent = "Plus que " + (nbLumieresRestantes + 1) + " !";
            //EffacerImportantMessage(messageCurrent);
            AjouterMessageImportant(messageCurrent, Console.TypeText.ALLY_TEXT, 1.2f, true, messagePrecedent);
		} else {
            if (!gm.eventManager.IsWin() && gm.eventManager.GetComponent<EventManagerWhileTrue>() == null) { // Ehhhh x)
                AjouterMessage("ON LES A TOUTES !", Console.TypeText.ALLY_TEXT);
                AjouterMessageImportant("FAUT SE BARRER MAINTENANT !!!", Console.TypeText.ALLY_TEXT, 2f);
            }
        }
	}

	// Quand le joueur lance les trails
	public void RunLocalisation(int nbLumieres, int nbItems) {
        if (nbLumieres > 0)
            AjouterMessage("Il te restes " + nbLumieres+ " Datas !", Console.TypeText.ALLY_TEXT);
        if (nbItems > 0)
            AjouterMessage("Il te restes " + nbItems + " Items !", Console.TypeText.ALLY_TEXT);
        if(nbItems + nbLumieres == 0) 
            AjouterMessage("On trouver rien à localiser !", Console.TypeText.ALLY_TEXT);
	}

	// Quand le joueur lance la détection
	public void RunDetection(Vector3 position) {
        AjouterMessage ("Ok on l'a trouvé, va en " + position + " !", Console.TypeText.ALLY_TEXT);
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

	// Quand le joueur se fait voir au début par les sondes !
	public void JoueurRepere() {
		AjouterMessageImportant ("Nous t'avons trouvé !", TypeText.ENNEMI_TEXT, 2f);
	}

    // Lorsque le joueur tente de construire un pont avec une cible invalide !
    public void PouvoirBridgeBuilderInvalide() {
        string message = "Ce n'est pas une cible valide !";
        AjouterMessageImportant(message, TypeText.ENNEMI_TEXT, 1f, true, message);
    }

    // Lorsque le joueur rentre dans l'EndGame
    public void StartEndGame() {
        AjouterMessageImportant("Faut trouver la sortie maintenant !", TypeText.ALLY_TEXT, 1f);
    }

    // Si le joueur est trop haut, on l'informe !
    public virtual void AltitudeCritique() {
		// On regarde si le joueur n'est pas trop haut en altitude
		if (player.transform.position.y > map.tailleMap.y + 3) {
			AjouterMessage ("Altitude critique !", TypeText.BASIC_TEXT);
		}
    }

    // Message lorsqu'un event de gravité se déclenche !
    public void GravityEventMessage(GravityManager.Direction direction, float intensité) {
        string message = "Changement de gravité !";
        AjouterMessageImportant(message, TypeText.ENNEMI_TEXT, 2, bAfficherInConsole: false, message);
        float pourcentage = intensité / 5.0f * 100.0f;
        AjouterMessage("Gravité : direction = " + direction.ToString() + " intensité = " + pourcentage.ToString("N2") + "%", TypeText.ENNEMI_TEXT);
    }

    // Message lors d'un blackout !
    public void BlackoutMessage() {
        string message = "Blackout !";
        AjouterMessageImportant(message, TypeText.ENNEMI_TEXT, 2, bAfficherInConsole: true, message);
    }

    // Lorsque l'on alerte les tracers !
    public void AlerterTracers() {
        string message = "Tracers Alertés !";
        AjouterMessageImportant(message, TypeText.ENNEMI_TEXT, 2, bAfficherInConsole: true, message);
    }

    // Lorsque l'on capture la première lumière dans la map Analyze
    public void AnalyzeLevelDeuxiemeSalve() {
        AjouterMessageImportant("On les a alertés et ils ont répliqué les Datas !", TypeText.ALLY_TEXT, 3);
    }

    // Lorsque le joueur tombe, typiquement dans le tutoriel !
    public void SavedFromFalling() {
        AjouterMessageImportant("Attention à ne être éjecté la prochaine fois !", TypeText.ALLY_TEXT, 3);
    }

    // Lorsque le joueur capture un item double saut !
    public void CaptureAddDoubleJump() {
        if (player.GetNbDoubleSautsMax() == 1) {
            AjouterMessageImportant("Double Saut Activé !", TypeText.ALLY_TEXT, 2, bAfficherInConsole: false);
        } else {
            AjouterMessageImportant("Double Saut + 1", TypeText.ALLY_TEXT, 2, bAfficherInConsole: false);
        }
        AjouterMessage("Tu peux effectuer un Double Saut en appuyant à nouveau sur Espace !", TypeText.ALLY_TEXT);
    }

    // Lorsque le joueur capture un item pour voler !
    public void CapturePouvoirGiverVoler() {
        AjouterMessageImportant("Vol Activé !", TypeText.ALLY_TEXT, 2, bAfficherInConsole: false);
        AjouterMessage("Tu peux maintenant Voler ! Appuie sur Espace pour monter et sur Shift pour descendre !", TypeText.ALLY_TEXT);
    }

    public void CapturePouvoirGiverItem(string pouvoirName, PouvoirGiverItem.PouvoirBinding pouvoirBinding) {
        AjouterMessageImportant(pouvoirName + " Activé !", TypeText.ALLY_TEXT, 2, bAfficherInConsole: false);
        string strBinding = "";
        switch(pouvoirBinding) {
            case PouvoirGiverItem.PouvoirBinding.A: strBinding = "A"; break;
            case PouvoirGiverItem.PouvoirBinding.E: strBinding = "E"; break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK: strBinding = "le click gauche"; break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK: strBinding = "le click droit"; break;
        }
        AjouterMessage("Tu peux utiliser le pouvoir " + pouvoirName + " en appuyant sur " + strBinding + " !", TypeText.ALLY_TEXT);
    }

    public void FirstBossChangementDePhase(int newPhaseIndice, float duree) {
        StartCoroutine(CFirstBossChangementDePhase(newPhaseIndice, duree));
    }
    protected IEnumerator CFirstBossChangementDePhase(int newPhaseIndice, float duree) {
        AjouterMessageImportant("Loading phase " + newPhaseIndice + " ...", TypeText.ENNEMI_TEXT, duree, bAfficherInConsole: true);
        yield return new WaitForSeconds(duree);
        AjouterMessageImportant("Phase " + newPhaseIndice + " chargé !", TypeText.ENNEMI_TEXT, 2, bAfficherInConsole: true);
    }

    public void InitPouvoirsDisplays() {
        Player player = gm.player;
        InitPouvoir(player.GetPouvoirA(), pouvoirDisplayA);
        InitPouvoir(player.GetPouvoirE(), pouvoirDisplayE);
        InitPouvoir(player.GetPouvoirLeftClick(), pouvoirDisplayLeftClick);
        InitPouvoir(player.GetPouvoirRightClick(), pouvoirDisplayRightClick);
    }

    protected void InitPouvoir(IPouvoir pouvoir, PouvoirDisplayInGame display) {
        display.Initialize(pouvoir);
    }

    public void RewardBestScore() {
        AjouterMessageImportant("Meilleur Score !!!", TypeText.ALLY_TEXT, 3, bAfficherInConsole: true);
    }
}
