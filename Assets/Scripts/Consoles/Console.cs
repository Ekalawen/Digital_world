using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;

public struct TimerMessage {
    public TimedMessage message;
    public Timer timer;

    public TimerMessage(TimedMessage timedMessage) {
        this.message = timedMessage;
        this.timer = new Timer(timedMessage.timing);
    }
}

public class Console : MonoBehaviour {

	public enum TypeText {GREEN_TEXT, RED_TEXT, BLUE_TEXT};

    public enum UIVisibility { ALL, JUST_CROSSHAIR, NOTHING };

    public enum ConseilType { RANDOM, CYCLIC };

    [Header("Messages")]
    public LocalizedString levelVisualName;
    public List<LocalizedString> conseils; // Les conseils à dispenser au joueur !
    public List<TimedMessage> timedMessages;
    public ConsoleStrings strings;
    public LocalizedString customInitMessage = null;
    public bool dontDisplayAnythingOnInit = false;
    public LocalizedString customWinMessage = null;

    [Header("OnLumiereCapturedMessages")]
    public List<int> nbLumieresTriggers;
    public List<TimedLocalizedMessage> nbLumieresTriggeredMessages;

    [Header("Special Messages")]
    public bool useAltitudeCritique = true; // Si on doit utiliser altitude critique ou pas dans cette partie !

    [Header("Random messages")]
	public Vector2 tempsAvantPhraseRandom; // Le temps avant de générer une phrase aléatoire dans la console
	public Vector2 tempsAvantConseiller; // Le temps avant de générer un conseil

    [Header("Text parameters")]
	public Color basicColor; // La couleur avec laquelle on écrit la plupart du temps
    public Color ennemiColor; // La couleur des messages ennemis
    public Color allyColor; // La couleur des messages alliées
	public LocalizedString basicPrefixLocalizedString; // Le préfixe à mettre devant chaque message basic
    public LocalizedString ennemiPrefixLocalizedString; // Le préfixe à mettre devant chaque message ennemi
    public LocalizedString allyPrefixLocalizedString; // Le préfixe à mettre devant chaque message allié
	public Font font; // La police de charactère des messages
	public int tailleTexte; // La taille du texte affiché

    [Header("Pouvoirs")]
    public GameObject pouvoirsCanvas;
    public PouvoirDisplayInGame pouvoirDisplayA;
    public PouvoirDisplayInGame pouvoirDisplayE;
    public PouvoirDisplayInGame pouvoirDisplayLeftClick;
    public PouvoirDisplayInGame pouvoirDisplayRightClick;
    public float pouvoirZoomInScale = 5.0f;
    public float pouvoirZoomInDuration = 1.5f;
    public AnimationCurve pouvoirZoomInCurve;

    [Header("Links")]
	public GameObject consoleBackground; // Là où l'on va afficher les lignes
	public TMP_Text importantText; // Là où l'on affiche les informations importantes
    public GameObject escapeButton; // Le truc qui clignote pour nous dire d'appuyer sur Escape à la fin du jeu !
    public TMP_Text escapeButtonText;
    public GameObject pauseMenu;
    public CounterDisplayer dataCountDisplayer;
    public FrameRateManager frameRateManager;
    public GameObject deathAstuce;
    public GameObject selectorManagerPrefab; // Used to know if it is a demo or not ! x)


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

    protected Timer timerConseiller;
    protected List<TimerMessage> timersMessages;
    protected Transform messagesFolder;
    protected Timer arrowKeysTimer;
    protected string basicPrefix;
    protected string ennemiPrefix;
    protected string allyPrefix;
    protected bool isLocalizationLoaded = false;
    protected List<float> deltaTimeList;
    protected bool isConsoleVisible = true;
    protected UIVisibility uiVisibility = UIVisibility.ALL;
    protected UIVisibilitySaver uiVisibilitySaver = null;
    protected Timer altitudeCritiqueTimer;

    public virtual void Initialize()
    {
        // Initialisation des variables
        name = "Console";
        gm = GameManager.Instance;
        map = gm.map;
        messagesFolder = new GameObject("Messages").transform;
        messagesFolder.parent = transform;
        lines = new List<GameObject>();
        numLines = new List<int>();
        player = gm.player;
        importantText.text = "";
        eventManager = gm.eventManager;
        InitFrameRateCounter();
        arrowKeysTimer = new Timer(2, setOver: true);
        altitudeCritiqueTimer = new Timer(3, setOver: true);
        HideDataCountDisplayerIfIR();
        DisplayOrNotConsole();
        ToggleUIVisibilityBasedOnSaver();

        StartCoroutine(CInitialize());
    }

    public virtual void DisplayOrNotConsole() {
        bool shouldDisplayConsole = PrefsManager.GetBool(PrefsManager.DISPLAY_CONSOLE_KEY, MenuOptions.defaultDisplayConsole);
        consoleBackground.SetActive(shouldDisplayConsole);
        frameRateManager.SetToAccordingConsolePosition(shouldDisplayConsole);
    }

    private IEnumerator CInitialize()
    {
        yield return LocalizationSettings.InitializationOperation;
        isLocalizationLoaded = true;

        // Les premiers messages
        InitPrefixs();
        PremiersMessages();
        timerConseiller = new Timer(UnityEngine.Random.Range(tempsAvantConseiller.x, tempsAvantConseiller.y));

        // Setup les timers des timedMessages
        InitTimersMessages();

        // Init les pouvoirs displays
        InitPouvoirsDisplays();

        InitializeDataCountText();
    }

    protected virtual void InitializeDataCountText() {
        if (gm.goalManager.GetGoalType() == GoalManager.GoalType.DATA) {
            SetDataCountText(Lumiere.GetCurrentDataCount());
        } else if (gm.goalManager.GetGoalType() == GoalManager.GoalType.VICTORY) {
            SetNbWinsText();
            gm.eventManager.onWinGame.AddListener(SetNbWinsText);
        }
    }

    public void OnLumiereCaptured() {
        int nbLumieres = gm.map.GetLumieres().Count;
        for(int i = 0; i < nbLumieresTriggers.Count; i++) {
            int nbLumieresTrigger = nbLumieresTriggers[i];
            if(nbLumieresTrigger == nbLumieres) {
                TimedLocalizedMessage timedMessage = nbLumieresTriggeredMessages[i];
                AjouterMessageImportant(timedMessage);
            }
        }
    }

    public virtual void Update () {
        if (!isLocalizationLoaded)
            return;

        AltitudeCritique();

        LancerConseils();

        // On détecte si le joueur est safe ou pas !
        DetectPlayerSafeOrNot();

        // On lance les messages timed
        RunTimedMessages();

        UpdateFrameRate();
	}

    protected void InitTimersMessages() {
        timersMessages = new List<TimerMessage>();
        foreach(TimedMessage tm in timedMessages) {
            timersMessages.Add(new TimerMessage(tm));
        }
    }

    public virtual void PremiersMessages()
    {
        DisplayLevelName();

        DisplayInitMessage();

        PremierGrandConseil();
    }

    protected void DisplayInitMessage() {
        if(dontDisplayAnythingOnInit) {
            return;
        }

        bool displayImportant = customInitMessage.IsEmpty;
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            DisplayDatasAndEnnemisCounts(displayImportant);
        } else {
            VaAussiLoinQueTuPeux(displayImportant);
        }

        if (!displayImportant) {
            AjouterMessageImportant(customInitMessage, TypeText.BLUE_TEXT, 3);
        }

        DisplayInitMessageWithBinding displayer = GetComponent<DisplayInitMessageWithBinding>();
        if(displayer != null) {
            displayer.DisplayInitMessage(this);
        }
    }

    protected void DisplayLevelName() {
        string levelName = levelVisualName.GetLocalizedString().Result;
        LocalizedString niveauString = strings.initialisationNiveau;
        niveauString.Arguments = new object[] { levelName };
        AjouterMessage(niveauString, TypeText.GREEN_TEXT, bUsePrefix: false);
        AjouterMessage(strings.initialisationMatrice, TypeText.GREEN_TEXT, bUsePrefix: false);
    }

    protected void VaAussiLoinQueTuPeux(bool displayImportant) {
        if (displayImportant) {
            AjouterMessageImportant(strings.vaAussiLoinQueTuPeux, TypeText.BLUE_TEXT, 3);
        } else {
            AjouterMessage(strings.vaAussiLoinQueTuPeux, TypeText.BLUE_TEXT);
        }
    }

    public void DisplayDatasAndEnnemisCounts(bool displayImportant) {
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            if (displayImportant) {
                int nbEnnemis = gm.ennemiManager.ennemis.Count;
                if (nbEnnemis > 0) {
                    AjouterMessageImportant(GetEnnemisCountDisplayMessage(), TypeText.BLUE_TEXT, 3, bAfficherInConsole: false);
                }
                int nbLumieres = map.GetLumieres().Count;
                if (nbLumieres > 0) {
                    AjouterMessageImportant(GetDataCountDisplayMessage(), TypeText.BLUE_TEXT, 3, bAfficherInConsole: false);
                }
            }
            AjouterMessage(GetDataCountDisplayMessage(), TypeText.BLUE_TEXT);
            AjouterMessage(GetEnnemisCountDisplayMessage(), TypeText.BLUE_TEXT);
        }
    }

    protected void PremierGrandConseil() {
        if(PrefsManager.GetBool(PrefsManager.ADVICE_ON_START_KEY, MenuOptions.defaultConseilOnStart)) {
            StartCoroutine(CPremierGrandConseil());
        }
    }

    protected IEnumerator CPremierGrandConseil() {
        yield return new WaitForSeconds(3.1f);
        Conseiller(ConseilType.CYCLIC, withImportantText: true);
    }

    protected LocalizedString GetDataCountDisplayMessage() {
        int nbLumieres = map.GetLumieres().Count;
        LocalizedString ls = strings.nbLumieresTrouvees;
        ls.Arguments = new object[] { nbLumieres };
        return ls;
    }

    protected LocalizedString GetEnnemisCountDisplayMessage() {
        int nbEnnemis = gm.ennemiManager.ennemis.Count;
        LocalizedString ls = strings.nbEnnemisTrouvees;
        ls.Arguments = new object[] { nbEnnemis };
        return ls;
    }

    public void PouvoirsDesactives() {
		AjouterMessageImportant (strings.pouvoirsDesactives, TypeText.BLUE_TEXT, 3);
		AjouterMessageImportant (strings.pouvoirsDesactives, TypeText.RED_TEXT, 3);
		AjouterMessageImportant (strings.pouvoirsDesactives, TypeText.GREEN_TEXT, 3);
    }

    protected void LancerConseils() {
        if (timerConseiller.IsOver()) {
            Conseiller(ConseilType.RANDOM);
            timerConseiller = new Timer(UnityEngine.Random.Range(tempsAvantConseiller.x, tempsAvantConseiller.y));
        }
    }

    protected void EffacerImportantText() {
        if (Time.timeSinceLevelLoad - lastTimeImportantText > tempsImportantText) {
			importantText.text = "";
		}
    }

    protected virtual void ConseillerUtiliserDetection() {
        if (map.GetLumieres().Count > 0) {
			if (Time.timeSinceLevelLoad - timeLastLumiereAttrapee > 25) {
				timeLastLumiereAttrapee = Time.timeSinceLevelLoad;
				AjouterMessage (strings.geolocaliserData, TypeText.BLUE_TEXT);
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
        TimedLocalizedMessage timedMessage,
        bool bAfficherInConsole = true,
        LocalizedString messageToReplace = null) {
        AjouterMessageImportant(
            timedMessage.localizedString,
            timedMessage.type,
            timedMessage.duree,
            bAfficherInConsole,
            messageToReplace);
    }

    public void AjouterMessageImportant(
        LocalizedString localizedString,
        object[] argumentsString,
        TypeText type,
        float tempsAffichage,
        bool bAfficherInConsole,
        LocalizedString messageToReplace = null,
        object[] argumentsReplace = null) {
        StartCoroutine(CAjouterMessageImportant(localizedString, argumentsString, type, tempsAffichage, bAfficherInConsole, messageToReplace, argumentsReplace));
    }

    protected IEnumerator CAjouterMessageImportant(
        LocalizedString localizedString,
        object[] argumentsString,
        TypeText type,
        float tempsAffichage,
        bool bAfficherInConsole,
        LocalizedString messageToReplace,
        object[] argumentsReplace) {
        AsyncOperationHandle<string> handle = localizedString.GetLocalizedString(argumentsString);
        yield return handle;
        string handleResult = handle.Result;
        if (messageToReplace != null) {
            AsyncOperationHandle<string> handleToReplace = messageToReplace.GetLocalizedString(argumentsReplace);
            yield return handleToReplace;
            AjouterMessageImportant(handleResult, type, tempsAffichage, bAfficherInConsole, handleToReplace.Result);
        } else {
            AjouterMessageImportant(handleResult, type, tempsAffichage, bAfficherInConsole);
        }
    }

    public void AjouterMessageImportant(
        LocalizedString localizedString,
        TypeText type,
        float tempsAffichage,
        bool bAfficherInConsole = true,
        LocalizedString messageToReplace = null) {
        StartCoroutine(CAjouterMessageImportant(localizedString, type, tempsAffichage, bAfficherInConsole, messageToReplace));
    }

    protected IEnumerator CAjouterMessageImportant(
        LocalizedString localizedString,
        TypeText type,
        float tempsAffichage,
        bool bAfficherInConsole,
        LocalizedString messageToReplace) {
        AsyncOperationHandle<string> handle = localizedString.GetLocalizedString();
        yield return handle;
        string stringResult = handle.Result;
        if (messageToReplace != null) {
            AsyncOperationHandle<string> handleToReplace = messageToReplace.GetLocalizedString();
            yield return handleToReplace;
            AjouterMessageImportant(stringResult, type, tempsAffichage, bAfficherInConsole, handleToReplace.Result);
        } else {
            AjouterMessageImportant(stringResult, type, tempsAffichage, bAfficherInConsole);
        }
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

    protected void EffacerImportantMessage(LocalizedString messageToErase) {
        StartCoroutine(CEffacerImportantMessage(messageToErase));
        //return EffacerImportantMessage(messageToErase.GetLocalizedString().Result);
    }

    protected IEnumerator CEffacerImportantMessage(LocalizedString messageToErase) {
        AsyncOperationHandle<string> handle = messageToErase.GetLocalizedString();
        yield return handle;
        EffacerImportantMessage(handle.Result);
    }

    protected int EffacerImportantMessage(string messageToErase) {
        List<string> lines = GetImportantTextLines();
        if (messageToErase == "")
            return 0;
        for(int i = 0; i < lines.Count; i++) {
            if(lines[i].Contains(messageToErase)) {
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
            case TypeText.GREEN_TEXT:
                return basicColor;
            case TypeText.RED_TEXT:
                return ennemiColor;
            case TypeText.BLUE_TEXT:
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
        LocalizedString localizedString,
        TypeText type,
        bool bUsePrefix = true) {
        //AjouterMessage(localizedString.GetLocalizedString().Result, type, bUsePrefix);
        StartCoroutine(CAjouterMessage(localizedString, type, bUsePrefix));
    }

    protected IEnumerator CAjouterMessage(LocalizedString localizedString, TypeText type, bool bUsePrefix) {
        AsyncOperationHandle<string> handle = localizedString.GetLocalizedString();
        yield return handle;
        AjouterMessage(
            handle.Result,
            type,
            bUsePrefix);
    }

    public void AjouterMessage(
        TimedMessage timedMessage,
        bool bUsePrefix = true) {
        AjouterMessage(
            timedMessage.message,
            timedMessage.type,
            bUsePrefix);
    }

    public void AjouterMessage(
        TimedLocalizedMessage timedLocalizedMessage,
        bool bUsePrefix = true) {
        AjouterMessage(
            timedLocalizedMessage.localizedString,
            timedLocalizedMessage.type,
            bUsePrefix);
    }


	public void AjouterMessage(string message, TypeText type, bool bUsePrefix = true) {
        // On préfixe ! :)
        if (bUsePrefix) {
            switch (type) {
                case TypeText.BLUE_TEXT:
                    message = allyPrefix + message;
                    break;
                case TypeText.GREEN_TEXT:
                    message = basicPrefix + message;
                    break;
                case TypeText.RED_TEXT:
                    message = ennemiPrefix + message;
                    break;
            }
        }

		// On crée le nouveau texte !
		GameObject newText = new GameObject(message, typeof(RectTransform));

        // On définit son parent
        newText.transform.SetParent (consoleBackground.transform);

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
		case TypeText.GREEN_TEXT:
			text.color = basicColor;
			break;
		case TypeText.RED_TEXT:
			text.color = ennemiColor;
			break;
		case TypeText.BLUE_TEXT:
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
        TimedMessage tm = new GameObject(message).AddComponent<TimedMessage>();
        tm.transform.parent = messagesFolder;
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
        while (numLines.Count > 0) {
            AddBlankLine();
        }
    }

	void LancerPhraseRandom() {
        // Cette fonction n'est plus utilisée, mais je la laisse parce que elle est cool :3
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
		AjouterMessage (phrase, TypeText.GREEN_TEXT);
	}

    protected void Conseiller(ConseilType conseilType, bool withImportantText = false) {
        if (conseils.Count == 0)
            return;

        int conseilIndice;
        if (conseilType == ConseilType.CYCLIC) {
            string conseilKey = gm.eventManager.GetKeyFor(PrefsManager.CONSEIL_INDICE_KEY);
            conseilIndice = PrefsManager.GetInt(conseilKey, 0);
            PrefsManager.SetInt(conseilKey, (conseilIndice + 1) % conseils.Count);
        } else {
            conseilIndice = UnityEngine.Random.Range(0, conseils.Count);
        }
        LocalizedString conseil = conseils[conseilIndice];
        StartCoroutine(CConseiller(conseil, withImportantText));
    }

    protected IEnumerator CConseiller(LocalizedString localizedString, bool withImportantText) {
        AsyncOperationHandle<string> handle = localizedString.GetLocalizedString();
        yield return handle;
        if (!withImportantText) {
            AjouterMessage(handle.Result, TypeText.GREEN_TEXT);
        } else {
            float duration = Mathf.Max(3, 3 + (handle.Result.Length - 40) * 0.05f);
            AjouterMessageImportant(handle.Result, TypeText.GREEN_TEXT, duration);
        }
    }

    // Lorsqu'un ennemi repère le joueur
    public void JoueurDetecte() {
        EffacerImportantMessage(strings.dissimule);
        AjouterMessageImportant(strings.detecte, Console.TypeText.RED_TEXT, 2, bAfficherInConsole: false, strings.detecte);
        AjouterMessage (strings.jeSaisOuVousEtes, Console.TypeText.RED_TEXT);
	}

	// Quand le joueur réussit à semer toutes les sondes
	public void SemerEnnemis() {
        EffacerImportantMessage(strings.detecte);
		AjouterMessageImportant (strings.dissimule, TypeText.BLUE_TEXT, 2, bAfficherInConsole: false, strings.dissimule);
        AjouterMessage(strings.onLesASemes, TypeText.BLUE_TEXT);
	}
	
	// Lorsqu'une sonde perd le joueur de vue
	public void JoueurPerduDeVue(string nomDetecteur) {
		//AjouterMessage ("On a déconnecté " + nomDetecteur + ".", Console.TypeText.ALLY_TEXT);
	}

	public void JoueurToucheSonde() {
		AjouterMessageImportant (strings.toucheParUneSondeImportant, Console.TypeText.RED_TEXT, 1, bAfficherInConsole: false, strings.toucheParUneSondeImportant);
        AjouterMessage(strings.toucheParUneSondeConsole, Console.TypeText.RED_TEXT);
	}

	public void JoueurToucheTracer() {
		AjouterMessageImportant (strings.toucheParUnTracerImportant, Console.TypeText.RED_TEXT, 1, bAfficherInConsole: false, strings.toucheParUnTracerImportant);
        AjouterMessage(strings.toucheParUnTracerConsole, Console.TypeText.RED_TEXT);
	}

	public void JoueurBlasteTracer() {
		AjouterMessageImportant (strings.blasteParUnTracerImportant, Console.TypeText.RED_TEXT, 1, bAfficherInConsole: false, strings.blasteParUnTracerImportant);
        AjouterMessage(strings.blasteParUnTracerConsole, Console.TypeText.RED_TEXT);
	}

	public void JoueurBlasteTracerLearning() {
		AjouterMessageImportant (strings.blasteParUnTracerImportantLearning, Console.TypeText.RED_TEXT, 3, bAfficherInConsole: false, strings.blasteParUnTracerImportantLearning);
        AjouterMessage(strings.blasteParUnTracerConsole, Console.TypeText.RED_TEXT);
	}

	// Lorsque le joueur réussi à s'échapper
	public void WinGame() {
		AjouterMessage (strings.winGameConsole1, Console.TypeText.RED_TEXT);
		AjouterMessage (strings.winGameConsole2, Console.TypeText.RED_TEXT);
        if (!customWinMessage.IsEmpty) {
            AjouterMessageImportant(customWinMessage, Console.TypeText.BLUE_TEXT, 5);
        }
        AjouterMessageImportant(strings.winGameImportant, Console.TypeText.BLUE_TEXT, 5);
        DisplayEscapeButton();
		StartCoroutine (Recompenser ());
	}

	// Text de récompense
	IEnumerator Recompenser() {
		yield return new WaitForSeconds (4);
		while (true) {
            LocalizedString ls = strings.winRecompense;
            string spaces = "";
            string ss = "";
            string exclamations = "";
			for (int i = 0; i < UnityEngine.Random.Range (0, 6); i++)
				spaces += " ";
			for (int i = 0; i < UnityEngine.Random.Range (3, 12); i++)
				ss += "S";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				exclamations += "!";
            ls.Arguments = new object[] { spaces, ss, exclamations };
			AjouterMessage (ls, Console.TypeText.BLUE_TEXT);
			yield return null;
		}
	}

	// Lorsque le joueur a été bloqué par les drones
	public void LoseGame(EventManager.DeathReason reason) {
        float timeDeathMessage = 5;
        switch(reason) {
            case EventManager.DeathReason.TIME_OUT:
                AjouterMessageImportant (strings.deathTimeOut2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathTimeOut1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.FALL_OUT:
                AjouterMessageImportant (strings.deathFallOut2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathFallOut1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.CAPTURED:
                AjouterMessageImportant (strings.deathCaptured2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathCaptured1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.TOUCHED_DEATH_CUBE:
                AjouterMessageImportant (strings.deathTouchedDeathCube2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathTouchedDeathCube1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.TOUCHED_BOUNCY_CUBE:
                AjouterMessageImportant (strings.deathTouchedBouncyCube2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathTouchedBouncyCube1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.POUVOIR_COST:
                AjouterMessageImportant (strings.deathPouvoirCost2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathPouvoirCost1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.OUT_OF_BLOCKS:
                AjouterMessageImportant (strings.deathOutOfBlocks2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathOutOfBlocks1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.FAILED_JUMP_EVENT:
                AjouterMessageImportant (strings.deathFailedJumpEvent2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathFailedJumpEvent1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.SONDE_HIT:
                AjouterMessageImportant (strings.deathSondeHit2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathSondeHit1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.TRACER_HIT:
                AjouterMessageImportant (strings.deathTracerHit2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathTracerHit1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.TRACER_BLAST:
                AjouterMessageImportant (strings.deathTracerBlast2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathTracerBlast1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.FLIRD_HIT:
                AjouterMessageImportant (strings.deathFlirdHit2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathFlirdHit1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.FIRST_BOSS_BLAST:
                AjouterMessageImportant (strings.deathFirstBossBlast2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathFirstBossBlast1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.FIRST_BOSS_HIT:
                AjouterMessageImportant (strings.deathFirstBossHit2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathFirstBossHit1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.SOUL_ROBBER_ASPIRATION:
                AjouterMessageImportant (strings.deathSoulRobberAspiration2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathSoulRobberAspiration1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
            case EventManager.DeathReason.SECOND_BOSS_LASER:
                AjouterMessageImportant (strings.deathSecondBossLaser2, Console.TypeText.RED_TEXT, timeDeathMessage);
                AjouterMessageImportant (strings.deathSecondBossLaser1, Console.TypeText.RED_TEXT, timeDeathMessage);
                break;
        }
        DisplayEscapeButton();
        if (gm.eventManager.ShouldQuitOrReload() == EventManager.QuitType.RELOAD) {
            DisplayDeathAstuces();
        }
		StartCoroutine (SeMoquer());
	}

    // On se moque de lui
    IEnumerator SeMoquer() {
		yield return new WaitForSeconds (4);
		while (true) {
			string spaces = "";
			string has = "";
			string exclamations = "";
			for (int i = 0; i < UnityEngine.Random.Range (0, 6); i++)
				spaces += " ";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				has += "HA";
			for (int i = 0; i < UnityEngine.Random.Range (1, 6); i++)
				exclamations += "!";
            LocalizedString ls = strings.loseRecompense;
            ls.Arguments = new object[] { spaces, has, exclamations };
            AjouterMessage(ls, Console.TypeText.RED_TEXT);
			yield return null;
		}
	}

	// Quand on attrape une lumière
	public void AttraperLumiere(int nbLumieresRestantes) {
        LocalizedString messagePrecedent = strings.plusQueX;
		if (!gm.eventManager.IsEndGameStarted() && nbLumieresRestantes > 0) {
            LocalizedString messageCurrent = strings.plusQueX;
            messageCurrent.Arguments = new object[] { nbLumieresRestantes };
            EffacerImportantMessage(messageCurrent);
            AjouterMessageImportant(messageCurrent, new object[]{ nbLumieresRestantes }, Console.TypeText.BLUE_TEXT, 1.2f, true, messagePrecedent, new object[]{ nbLumieresRestantes + 1});
		} else if (!gm.eventManager.IsEndGameStarted() && nbLumieresRestantes == 0 && !gm.eventManager.NoMoreElementsToBeDoneBeforeEndGame()) {
            LocalizedString messageOne = strings.plusQueX;
            messageOne.Arguments = new object[] { 1 };
            EffacerImportantMessage(messageOne);
            IlResteQuelqueChoseAFaire();
		} else {
            if (!gm.eventManager.IsGameWin() && gm.eventManager.GetComponent<EventManagerWhileTrue>() == null) { // Ehhhh x)
                LocalizedString messageOne = strings.plusQueX;
                messageOne.Arguments = new object[] { 1 };
                EffacerImportantMessage(messageOne);
                FautTrouverLaSortie();
            }
        }
	}

	// Quand le joueur lance les trails
	public void RunLocalisation(int nbLumieres, int nbItems) {
        if (nbLumieres > 0) {
            LocalizedString ls = strings.localisationIlTeResteXData;
            ls.Arguments = new object[] { nbLumieres };
            AjouterMessage(ls, Console.TypeText.BLUE_TEXT);
        }
        if (nbItems > 0) {
            LocalizedString ls = strings.localisationIlTeResteXItems;
            ls.Arguments = new object[] { nbItems };
            AjouterMessage(ls, Console.TypeText.BLUE_TEXT);
        }
        if (nbItems + nbLumieres == 0) {
            LocalizedString ls = strings.localisationRienTrouve;
            AjouterMessage(ls, Console.TypeText.BLUE_TEXT);
        }
	}

	// Quand le joueur lance la détection
	public void RunPathfinder(Vector3 position) {
        LocalizedString ls = strings.runDetection;
        ls.Arguments = new object[] { position.ToString() };
        AjouterMessage (ls, Console.TypeText.BLUE_TEXT);
	}

	//public void SummarizePathfinder(List<bool> hasFound, List<int> nbFound) {
 //       for(int i = hasFound.Count - 1; i >= 0; i--) {
 //           if(hasFound[i]) {
 //               LocalizedString ls = strings.summarizePathfinder[i];
 //               ls.Arguments = new object[] { nbFound[i] };
 //               AjouterMessageImportant(ls, TypeText.BLUE_TEXT, 2.0f);
 //           }
 //       }
	//}

	public void SummarizePathfinder(List<PathfinderData> pathfinderDatas) {
        for(int i = 2; i >= 0; i--) {
            List<PathfinderData> currentDatas = pathfinderDatas.FindAll(d => d.stringIndice == i && d.haveFoundSomething);
            if(currentDatas.Count > 0) {
                LocalizedString ls = strings.summarizePathfinder[i];
                int nbFound = currentDatas.Select(d => d.nbPositionsTheoretical).Sum();
                ls.Arguments = new object[] { nbFound };
                AjouterMessageImportant(ls, TypeText.BLUE_TEXT, 2.0f);
            }
        }
	}

    // Quand on essaye de faire une localisation alors qu'il n'a pas le droit !
    public void FailPathfinderUnauthorized() {
		AjouterMessageImportant(strings.failLocalisationUnauthorized, Console.TypeText.RED_TEXT, 2.0f, true, strings.failLocalisationUnauthorized);
    }

    // Quand on essaye de faire une localisation et qu'on ne trouve pas de chemin !
    public void FailPathfinderObjectifInateignable() {
		AjouterMessageImportant(strings.failLocalisationObjectifInateignable, Console.TypeText.RED_TEXT, 2.0f, true, strings.failLocalisationObjectifInateignable);
    }
    public void FailPathfinderInEndEvent() {
		AjouterMessageImportant(strings.failLocalisationInEndEvent, Console.TypeText.RED_TEXT, 2.0f, true, strings.failLocalisationInEndEvent);
    }


	// Quand le joueur attérit d'un grand saut
	public virtual void GrandSaut(float hauteurSaut) {
        LocalizedString ls = strings.wowQuelSaut;
        ls.Arguments = new object[] { (int)hauteurSaut };
		AjouterMessage(ls, Console.TypeText.GREEN_TEXT);
	}

    // Lorsque le joueur tente de construire un pont avec une cible invalide !
    public void PouvoirBridgeBuilderInvalide() {
        AjouterMessageImportant(strings.bridgeBuilderInvalid, TypeText.RED_TEXT, 1f, true, strings.bridgeBuilderInvalid);
    }

    // Lorsque le joueur rentre dans l'EndGame
    public void FautTrouverLaSortie() {
        AjouterMessageImportant(strings.fautTrouverLaSortie, TypeText.BLUE_TEXT, 1f);
    }

    public void IlResteQuelqueChoseAFaire() {
        AjouterMessageImportant(strings.ilResteQuelqueChoseAFaire, TypeText.BLUE_TEXT, 1f);
    }

    // Si le joueur est trop haut, on l'informe !
    public virtual void AltitudeCritique() {
        if (!useAltitudeCritique || !altitudeCritiqueTimer.IsOver())
            return;
		// On regarde si le joueur n'est pas trop haut en altitude
		if (player.transform.position.y > map.GetBoundingBox().yMax + 4) {
			AjouterMessage (strings.altitudeCritique, TypeText.GREEN_TEXT);
            altitudeCritiqueTimer.Reset();
		}
    }

    // Message lorsqu'un event de gravité se déclenche !
    public void GravityEventMessage(GravityManager.Direction direction, float intensité) {
        AjouterMessageImportant(strings.changementDeGravite, TypeText.RED_TEXT, 2, bAfficherInConsole: false, strings.changementDeGravite);
        float pourcentage = intensité / 5.0f * 100.0f;
        LocalizedString ls = strings.changementDeGraviteDirection;
        ls.Arguments = new object[] { direction.ToString(), pourcentage.ToString("N2") };
        AjouterMessage(ls, TypeText.RED_TEXT);
    }

    // Message lors d'un blackout !
    public void BlackoutMessage() {
        AjouterMessageImportant(strings.blackout, TypeText.RED_TEXT, 2, bAfficherInConsole: true, strings.blackout);
    }

    // Lorsque l'on alerte les tracers !
    public void AlerterTracers() {
        AjouterMessageImportant(strings.tracersAlertes, TypeText.RED_TEXT, 2, bAfficherInConsole: true, strings.tracersAlertes);
    }

    // Lorsque l'on capture la première lumière dans la map Analyze
    public void AnalyzeLevelDeuxiemeSalve() {
        StartCoroutine(CAnalyzeLevelDeuxiemeSalve());
    }
    protected IEnumerator CAnalyzeLevelDeuxiemeSalve() {
        float tempsPremierMessage = 1.5f;
        float tempsDeuxiemeMessage = 1.5f;
        AjouterMessageImportant(strings.analyzeLevelDeuxiemeSalveIntrusion, TypeText.RED_TEXT, tempsPremierMessage);
        yield return new WaitForSeconds(tempsPremierMessage);
        yield return null;
        AjouterMessageImportant(strings.analyzeLevelDeuxiemeSalveReplication, TypeText.RED_TEXT, tempsDeuxiemeMessage);
    }

    // Lorsque le joueur tombe, typiquement dans le tutoriel !
    public void SavedFromFalling() {
        AjouterMessageImportant(strings.attentionANePasEtreEjecte, TypeText.BLUE_TEXT, 3);
    }

    // Lorsque le joueur touche un cube de la mort dans le tutoriel !
    public void SavedFromDeathCube() {
        AjouterMessageImportant(strings.attentionAuxCubesDeLaMort, TypeText.BLUE_TEXT, 3);
    }

    // Lorsque le joueur capture un item double saut !
    public void CaptureAddDoubleJump() {
        if (player.GetNbDoubleSautsMax() == 1) {
            AjouterMessageImportant(strings.doubleSautActive, TypeText.BLUE_TEXT, 2, bAfficherInConsole: false);
        } else {
            AjouterMessageImportant(strings.doubleSautPlusUn, TypeText.BLUE_TEXT, 2, bAfficherInConsole: false);
        }
        AjouterMessage(strings.doubleSautExplications, TypeText.BLUE_TEXT);
    }

    // Lorsque le joueur capture un item pour voler !
    public void CapturePouvoirGiverVoler() {
        AjouterMessageImportant(strings.volActive, TypeText.BLUE_TEXT, 2, bAfficherInConsole: false);
        AjouterMessage(strings.volExplications, TypeText.BLUE_TEXT);
    }

    public void CapturePouvoirGiverItem(LocalizedString pouvoirName, PouvoirGiverItem.PouvoirBinding pouvoirBinding, LocalizedString subPhrase = null) {
        StartCoroutine(CCapturePouvoirGiverTime(pouvoirName, pouvoirBinding, subPhrase));
    }

    private IEnumerator CCapturePouvoirGiverTime(LocalizedString pouvoirName, PouvoirGiverItem.PouvoirBinding pouvoirBinding, LocalizedString subPhrase = null) {
        AsyncOperationHandle<string> handlePouvoirName = pouvoirName.GetLocalizedString();
        yield return handlePouvoirName;
        string pouvoirNameString = handlePouvoirName.Result;
        LocalizedString pouvoirGiverActive = strings.pouvoirGiverActive;
        pouvoirGiverActive.Arguments = new object[] { pouvoirNameString };
        if(subPhrase != null) {
            AjouterMessageImportant(subPhrase, TypeText.BLUE_TEXT, 2);
        }
        AjouterMessageImportant(pouvoirGiverActive, TypeText.BLUE_TEXT, 2, bAfficherInConsole: false);
        LocalizedString strBinding = null;
        switch (pouvoirBinding) {
            case PouvoirGiverItem.PouvoirBinding.A: strBinding = strings.keyA; break;
            case PouvoirGiverItem.PouvoirBinding.E: strBinding = strings.keyE; break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK: strBinding = strings.keyClicGauche; break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK: strBinding = strings.keyClicDroit; break;
        }
        AsyncOperationHandle<string> handlePouvoirBinding = strBinding.GetLocalizedString();
        yield return handlePouvoirBinding;
        LocalizedString pouvoirGiverExplications = strings.pouvoirGiverExplications;
        pouvoirGiverExplications.Arguments = new object[] { pouvoirNameString, handlePouvoirBinding.Result };
        AjouterMessage(pouvoirGiverExplications, TypeText.BLUE_TEXT);
    }

    public void FirstBossChangementDePhase(int newPhaseIndice, float duree) {
        StartCoroutine(CFirstBossChangementDePhase(newPhaseIndice, duree));
    }
    protected IEnumerator CFirstBossChangementDePhase(int newPhaseIndice, float duree) {
        LocalizedString chargement = strings.bossChangementDePhaseChargement;
        chargement.Arguments = new object[] { newPhaseIndice };
        AjouterMessageImportant(chargement, TypeText.RED_TEXT, duree, bAfficherInConsole: true);
        yield return new WaitForSeconds(duree);
        LocalizedString termine = strings.bossChangementDePhaseTermine;
        termine.Arguments = new object[] { newPhaseIndice };
        AjouterMessageImportant(termine, TypeText.RED_TEXT, 2, bAfficherInConsole: true);
    }

    public void SecondBossChangementDePhase(int newPhaseIndice, float duree) {
        StartCoroutine(CSecondBossChangementDePhase(newPhaseIndice, duree));
    }
    protected IEnumerator CSecondBossChangementDePhase(int newPhaseIndice, float duree) {
        LocalizedString chargement = strings.bossChangementDePhaseChargement;
        chargement.Arguments = new object[] { newPhaseIndice };
        AjouterMessageImportant(chargement, TypeText.RED_TEXT, duree, bAfficherInConsole: true);
        yield return new WaitForSeconds(duree);
        LocalizedString termine = strings.bossChangementDePhaseTermine;
        termine.Arguments = new object[] { newPhaseIndice };
        AjouterMessageImportant(termine, TypeText.RED_TEXT, 2, bAfficherInConsole: true);
    }

    public void InitPouvoirsDisplays() {
        Player player = gm.player;
        InitPouvoir(player.GetPouvoirA(), pouvoirDisplayA);
        InitPouvoir(player.GetPouvoirE(), pouvoirDisplayE);
        InitPouvoir(player.GetPouvoirLeftClick(), pouvoirDisplayLeftClick);
        InitPouvoir(player.GetPouvoirRightClick(), pouvoirDisplayRightClick);
    }

    public void InitPouvoir(IPouvoir pouvoir, PouvoirDisplayInGame display) {
        display.Initialize(pouvoir);
        if (pouvoir != null) {
            pouvoir.SetPouvoirDisplay(display);
        }
    }

    public void RewardBestScore() {
        AjouterMessageImportant(strings.meilleurScore, TypeText.GREEN_TEXT, 3, bAfficherInConsole: true);
    }

    public void RewardNewRegularTreshold(int dataCount) {
        object[] argument = new object[] { dataCount };
        AjouterMessageImportant(strings.rewardNewRegularTreshold, argument, TypeText.GREEN_TEXT, 3, bAfficherInConsole: true);
    }

    public void RewardNewInfiniteTreshold(int dataCount) {
        object[] argument = new object[] { dataCount };
        AjouterMessageImportant(strings.rewardNewInfinitereshold, argument, TypeText.GREEN_TEXT, 3, bAfficherInConsole: true);
    }

    public void WhileTrueEndEventAutoDestructionEnclanchee(LocalizedString localizedString) {
        if (localizedString.IsEmpty) {
            AjouterMessageImportant(strings.autoDestructionEnclenchee, Console.TypeText.RED_TEXT, 2.0f);
        } else {
            AjouterMessageImportant(localizedString, Console.TypeText.RED_TEXT, 2.0f);
        }
    }

    // Lorsque l'on commence à détruire les blocks dans l'infinite runner :)
    public void InfiniteRunnerStartCubeDestruction() {
        AjouterMessageImportant(strings.deconnexionEnclenchee, Console.TypeText.RED_TEXT, 2.0f);
    }

    public void AddGapInConsole() {
        AjouterMessage("", TypeText.BLUE_TEXT, bUsePrefix: false);
    }

    public void CleanAllImportantTexts() {
        importantText.text = "";
    }

    public void DontUseArrowKeys() {
        if (arrowKeysTimer.IsOver()) {
            AjouterMessageImportant(strings.ZQSDinsteadOfArrows, TypeText.GREEN_TEXT, arrowKeysTimer.GetDuree(), false, strings.ZQSDinsteadOfArrows);
            arrowKeysTimer.Reset();
        }
    }

    public void JumpStun() {
        AjouterMessageImportant(strings.jumpStunImportant, TypeText.RED_TEXT, 2.0f, bAfficherInConsole: false, messageToReplace: strings.jumpActivation);
        AjouterMessage(strings.jumpStunConsole, TypeText.RED_TEXT);
    }

    public void DisplayEscapeButton() {
        escapeButton.SetActive(IsConsoleVisible());
        if (gm.eventManager.ShouldQuitOrReload() == EventManager.QuitType.RELOAD) {
            string binding = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.RESTART);
            escapeButtonText.text = strings.restartButtonRestart.GetLocalizedString(binding).Result;
        } else {
            string binding = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.PAUSE);
            escapeButtonText.text = strings.restartButtonContinue.GetLocalizedString(binding).Result;
        }
    }

    public void NotifyPlayerToPressShift() {
        string bindingArgument = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.SHIFT);

        LocalizedString lsImportant = strings.pressShiftImportant;
        lsImportant.Arguments = new object[] { bindingArgument };
        gm.console.AjouterMessageImportant(lsImportant, Console.TypeText.BLUE_TEXT, 2.0f, bAfficherInConsole: false);

        LocalizedString lsConsole = strings.pressShiftConsole;
        lsConsole.Arguments = new object[] { bindingArgument };
        gm.console.AjouterMessage(lsConsole, Console.TypeText.BLUE_TEXT);
    }

    protected void InitPrefixs() {
        basicPrefix = basicPrefixLocalizedString.GetLocalizedString().Result;
        ennemiPrefix = ennemiPrefixLocalizedString.GetLocalizedString().Result;
        allyPrefix = allyPrefixLocalizedString.GetLocalizedString().Result;
    }

    public void OpenPauseMenu() {
        pauseMenu.SetActive(IsConsoleVisible());
    }

    public void ClosePauseMenu() {
        pauseMenu.SetActive(false);
    }

    public void SetDataCountText(int dataCount) {
        StartCoroutine(CSetDataCountText(dataCount));
    }

    protected IEnumerator CSetDataCountText(int dataCount) {
        string nextTresholdSymbol = gm.goalManager.GetNextTresholdSymbolFor(dataCount);
        AsyncOperationHandle<string> handle = strings.dataCount.GetLocalizedString(dataCount, nextTresholdSymbol);
        yield return handle;
        dataCountDisplayer.Display(handle.Result);
    }

    public void SetNbWinsText() {
        int nbWins = eventManager.GetNbWins();
        StartCoroutine(CSetNbWinsText(nbWins));
    }

    protected IEnumerator CSetNbWinsText(int nbWins) {
        string nextTresholdSymbol = gm.goalManager.GetNextTresholdSymbolFor(nbWins);
        AsyncOperationHandle<string> handle = strings.nbWinsCount.GetLocalizedString(nbWins, nextTresholdSymbol);
        yield return handle;
        dataCountDisplayer.Display(handle.Result);
    }

    public void AddToDataCountText(int dataCount, int dataCountAdded) {
        SetDataCountText(dataCount);
        dataCountDisplayer.AddVolatileText($"+ {dataCountAdded.ToString()}", dataCountDisplayer.GetTextColor());
    }

    protected void HideDataCountDisplayerIfIR() {
        if(gm.GetMapType() == MenuLevel.LevelType.INFINITE) {
            dataCountDisplayer.gameObject.SetActive(false);
        }
    }

    public void ResetTimeItemMessage(float resetTime) {
        LocalizedString lsImportant = strings.timeResetImportant;
        lsImportant.Arguments = new object[] { resetTime };
        gm.console.AjouterMessageImportant(lsImportant, Console.TypeText.GREEN_TEXT, 2.0f, bAfficherInConsole: false);

        LocalizedString lsConsole = strings.timeResetConsole;
        lsConsole.Arguments = new object[] { resetTime };
        gm.console.AjouterMessage(lsConsole, Console.TypeText.GREEN_TEXT);
    }

    public void ControllerPlugIn() {
        AjouterMessageImportant(strings.controllerPlugIn, Console.TypeText.GREEN_TEXT, 3.0f);
    }

    public void ControllerPlugOut() {
        AjouterMessageImportant(strings.controllerPlugOut, Console.TypeText.GREEN_TEXT, 3.0f);
    }

    public void SwapToController(InputController inputController) {
        if (inputController.IsController()) {
            LocalizedString ls = strings.swapToController;
            ls.Arguments = new object[] { inputController.GetName() };
            AjouterMessageImportant(ls, Console.TypeText.GREEN_TEXT, 3.0f);
        } else {
            LocalizedString ls = strings.swapToKeyboard;
            ls.Arguments = new object[] { inputController.GetName() };
            AjouterMessageImportant(ls, Console.TypeText.GREEN_TEXT, 3.0f);
        }
    }

    public void ApparitionDesDatas() {
        AjouterMessageImportant(strings.apparitionDesDatas, Console.TypeText.BLUE_TEXT, 3.0f);
    }

    public void SetInvincible() {
        AjouterMessageImportant(strings.setInvincible, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void UnsetInvincible() {
        AjouterMessageImportant(strings.unsetInvincible, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void SetPouvoirsCooldownZero() {
        AjouterMessageImportant(strings.setPouvoirsCooldownZero, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void UnsetPouvoirsCooldownZero() {
        AjouterMessageImportant(strings.unsetPouvoirsCooldownZero, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void InitFrameRateCounter() {
        frameRateManager.Initialize();
    }

    protected void UpdateFrameRate() {
        frameRateManager.Tick();
    }

    public bool IsConsoleVisible() {
        return isConsoleVisible;
    }

    public void SetConsoleVisibility(bool isVisible) {
        isConsoleVisible = isVisible;
        consoleBackground.SetActive(isVisible);
        importantText.gameObject.SetActive(isVisible);
        dataCountDisplayer.gameObject.SetActive(isVisible);
        frameRateManager.SetVisibility(isVisible);
        pouvoirsCanvas.SetActive(isVisible);
        escapeButton.SetActive(isVisible && gm.eventManager.IsGameOver());
        pauseMenu.SetActive(isVisible && gm.IsPaused());
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            gm.timerManager.timerDisplayer.gameObject.SetActive(isVisible);
        } else {
            gm.GetInfiniteMap().nbBlocksDisplayer.gameObject.SetActive(isVisible);
        }
    }

    public void SwapConsoleVisibility() {
        switch (uiVisibility) {
            case UIVisibility.ALL:
                SetUIVisibilityTo(UIVisibility.JUST_CROSSHAIR);
                break;
            case UIVisibility.JUST_CROSSHAIR:
                SetUIVisibilityTo(UIVisibility.NOTHING);
                break;
            case UIVisibility.NOTHING:
                SetUIVisibilityTo(UIVisibility.ALL);
                break;
        }
        SaveUIVisibilityForNextScene();
    }

    private void SetUIVisibilityTo(UIVisibility newVisibility) {
        switch (newVisibility) {
            case UIVisibility.ALL:
                SetConsoleVisibility(true);
                gm.pointeur.gameObject.SetActive(true);
                break;
            case UIVisibility.JUST_CROSSHAIR:
                SetConsoleVisibility(false);
                gm.pointeur.gameObject.SetActive(true);
                break;
            case UIVisibility.NOTHING:
                SetConsoleVisibility(false);
                gm.pointeur.gameObject.SetActive(false);
                break;
        }
        uiVisibility = newVisibility;
    }

    protected void SaveUIVisibilityForNextScene() {
        if(uiVisibilitySaver == null) {
            uiVisibilitySaver = new GameObject("UIVisibilitySaver").AddComponent<UIVisibilitySaver>();
        }
        uiVisibilitySaver.uIVisibility = uiVisibility;
        DontDestroyOnLoad(uiVisibilitySaver);
    }

    public void DestroyUIVisibilitySaver() {
        if (uiVisibilitySaver != null) {
            Destroy(uiVisibilitySaver.gameObject);
        }
    }

    protected void ToggleUIVisibilityBasedOnSaver() {
        uiVisibilitySaver = FindObjectOfType<UIVisibilitySaver>();
        if(uiVisibilitySaver != null) {
            SetUIVisibilityTo(uiVisibilitySaver.uIVisibility);
        }
    }

    public void DisplayDeathAstuces() {
        StartCoroutine(CDisplayDeathAstuces());
    }

    public IEnumerator CDisplayDeathAstuces() {
        deathAstuce.SetActive(true);
        string conseilKey = gm.eventManager.GetKeyFor(PrefsManager.CONSEIL_INDICE_KEY);
        int conseilIndice = PrefsManager.GetInt(conseilKey, 0);
        PrefsManager.SetInt(conseilKey, (conseilIndice + 1) % conseils.Count);
        LocalizedString conseil = conseils[conseilIndice];
        AsyncOperationHandle<string> handle = conseil.GetLocalizedString();
        yield return handle.Task;
        TMP_Text text = deathAstuce.GetComponentInChildren<TMP_Text>();
        text.text = text.text.Substring(0, text.text.Count() - 1) + " "; // Delete ending '\n'
        text.text += handle.Result;
    }

    public void ZoomInPouvoir(PouvoirDisplayInGame pouvoirDisplay) {
        RectTransform pouvoirRect = pouvoirDisplay.GetComponent<RectTransform>();
        Vector3 initialScale = pouvoirRect.localScale;
        pouvoirRect.localScale = Vector3.one * pouvoirZoomInScale;
        LeanTween.scale(pouvoirRect, initialScale, pouvoirZoomInDuration).setEase(pouvoirZoomInCurve);
    }

    public bool IsDemo() {
        return selectorManagerPrefab.GetComponent<SelectorManager>().isDemo;
    }

    public void SwapEnnemisActivation(bool newState) {
        if(newState) {
            EnableEnnemis();
        } else {
            DisableEnnemis();
        }
    }

    public void DisableEnnemis() {
        AjouterMessageImportant(strings.disableEnnemis, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void EnableEnnemis() {
        AjouterMessageImportant(strings.enableEnnemis, Console.TypeText.BLUE_TEXT, 2.0f);
    }

    public void ExternalStartEndGame() {
        AjouterMessageImportant(strings.externalStartEndEvent, Console.TypeText.RED_TEXT, 2.0f);
    }

    public void UpdatePouvoirBindings() {
        pouvoirDisplayA?.UpdateBinding();
        pouvoirDisplayE?.UpdateBinding();
        pouvoirDisplayLeftClick?.UpdateBinding();
        pouvoirDisplayRightClick?.UpdateBinding();
    }
}
