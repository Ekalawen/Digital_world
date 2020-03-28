using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class MenuLevel : MonoBehaviour {

    public static string LEVEL_NAME_KEY = "levelNameKey";
    public static string CURRENT_INPUT_FIELD_KEY = "currentInputField";
    public static string NB_WINS_KEY = "nbVictoires";
    public static string NB_TRIES_KEY = "nbTries";
    public static string HIGHEST_SCORE_KEY = "highestScore";
    public static string TRACE_KEY = "trace";
    public static string HAS_JUST_WIN_KEY = "hasJustWin";
    public static string HAS_ALREADY_DISCOVER_LEVEL_KEY = "hasAlreadyDiscoverLevel";
    public static string SUPER_CHEATED_PASSWORD = "lecreateurdecejeuestmonuniquedieuetmaitre";

    public string levelSceneName;
    public string levelFolderName;
    public MenuLevelSelector menuLevelSelector;
    public MenuBackgroundBouncing menuBouncingBackground;
    public Text textLevelName;
    public InputField inputFieldNext;
    public string nextPassword = "passwd";
    public GameObject joueurPrefab;
    public GameObject consolePrefab;
    public TexteExplicatif texteInformations;
    public TexteExplicatif texteExplicatifPasswdError;
    public TexteExplicatif texteExplicatifDonneesHackes;
    public TexteExplicatif texteExplicatifDonneesHackesSuccess;
    public TexteExplicatif texteExplicatifIntroduction;
    public Text score_nbTries, score_nbWins, score_winrate, score_highestScore;

    // Les propriétés du background de ce level
    public float probaSource = 0.00035f; // La probabilité d'être une source
    public int distanceSource = 8; // La distance d'action de la source
    public float decroissanceSource = 0.01f; // La vitesse de décroissance de la source
    public List<ColorSource.ThemeSource> themes; // Les couleurs des sources :)

    protected bool playStarted = false;

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS) {
            if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space))
            {
                Play();
            }
            // Les cotes pour changer de niveau
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) {
                Previous();
            }
        }
    }

    private void OnEnable() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
        ReadScores();

        MenuManager.DISABLE_HOTKEYS = false;
        InitTextesExplicatifs();

        DisplayPopupUnlockLevel();
        DisplayPopupUnlockNewTreshold();

        string key = GetName() + CURRENT_INPUT_FIELD_KEY;
        inputFieldNext.text = PlayerPrefs.GetString(key);
    }

    public void Play() {
        menuLevelSelector.Play(levelSceneName);
    }

    public void Next() {
        if (inputFieldNext.text == GetPassword()) {
            menuLevelSelector.Next();
        } else if (inputFieldNext.text == SUPER_CHEATED_PASSWORD) {
            menuLevelSelector.Next();
        } else {
            texteExplicatifPasswdError.Run();
        }
    }

    public void NextIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Next();
        else
            texteInformations.EnableHotkeysNextFrame();
    }

    public void Previous() {
        menuLevelSelector.Previous();
    }
    public void Back() {
        menuLevelSelector.Back();
    }

    public void OpenInformations() {
        texteInformations.Run(GetNbWins());
    }

    public void OpenDonneesHackes() {
        // Changer le texte des données hackés en fonction du nombre de fois où l'on a gagné ce niveau !
        string key = textLevelName.text + NB_WINS_KEY;
        int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        if (nbVictoires == 0) {
            texteExplicatifDonneesHackes.Run(GetNbWins());
        } else {
            texteExplicatifDonneesHackesSuccess.Run(GetNbWins());
            AddNextPallierMessageToAllFragments();
        }
    }

    protected void AddNextPallierMessageToAllFragments() {
        TresholdText tresholdText = texteExplicatifDonneesHackesSuccess.GetTresholdText();
        List<TresholdFragment> fragments = tresholdText.GetAllFragmentsOrdered();
        for (int i = 0; i < fragments.Count; i++) {
            if (i < fragments.Count - 1) {
                int nextTreshold = fragments[i + 1].treshold;
                fragments[i].ApplyReplacementEvaluator(
                    new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
                    (Match match) => "Prochain pallier à " + nextTreshold + " victoires.\n\n\n"));
            } else {
                fragments[i].ApplyReplacementEvaluator(
                    new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
                    (Match match) => "Dernier pallier.\n\n\n"));
            }
        }
        texteExplicatifDonneesHackesSuccess.ComputeText(GetNbWins());
    }

    public void SaveNextInputField() {
        if (inputFieldNext.text == GetPassword() || inputFieldNext.text == SUPER_CHEATED_PASSWORD) {
            string key = textLevelName.text + CURRENT_INPUT_FIELD_KEY;
            PlayerPrefs.SetString(key, inputFieldNext.text);
        }
    }

    protected void ReadScores() {
        score_nbWins.text = ChangeLastWord(score_nbWins.text, GetNbWins().ToString());
        score_nbTries.text = ChangeLastWord(score_nbTries.text, GetNbTries().ToString());
        string winrateString = (100.0f * GetWinrate()).ToString("N2") + "%";
        score_winrate.text = ChangeLastWord(score_winrate.text, winrateString);
        string highestScoreString = (HasHighestScore()) ? GetHighestScore().ToString("N2") : "null";
        score_highestScore.text = ChangeLastWord(score_highestScore.text, highestScoreString);
    }

    public int GetNbWins() {
        string key = textLevelName.text + NB_WINS_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public int GetNbTries() {
        string key = textLevelName.text + NB_TRIES_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public float GetWinrate() {
        return (float)GetNbWins() / ((float)GetNbWins() + (float)GetNbTries());
    }

    public bool HasHighestScore() {
        string key = textLevelName.text + HIGHEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key);
    }

    public float GetHighestScore() {
        string key = textLevelName.text + HIGHEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0.0f;
    }

    public static string ChangeLastWord(string str, string lastWordReplacement) {
        string[] splited = str.Split(' ');
        splited[splited.Length - 1] = lastWordReplacement;
        return string.Join(" ", splited);
    }

    public string GetTrace() {
        string key = textLevelName.text + TRACE_KEY;
        if (!PlayerPrefs.HasKey(key))
            InitTrace();
        return PlayerPrefs.GetString(key);
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = textLevelName.text + TRACE_KEY;
        PlayerPrefs.SetString(key, trace);
    }

    public string GetName() {
        return textLevelName.text;
    }

    public string GetPasse() {
        return nextPassword;
    }

    public string GetPassword() {
        return GetPasse() + GetTrace();
    }

    public static string SurroundWithBlueColor(Match match) {
        return "<color=blue>" + match.Value + "</color>";
    }

    protected void InitTextesExplicatifs() {
        string rootPath = "Assets/Texts/Levels/" + levelFolderName + "/";
        texteInformations.SetRootPath(rootPath);
        texteExplicatifPasswdError.SetRootPath(rootPath);
        texteExplicatifDonneesHackes.SetRootPath(rootPath);
        texteExplicatifDonneesHackesSuccess.SetRootPath(rootPath);
        if(texteExplicatifIntroduction != null)
            texteExplicatifIntroduction.SetRootPath(rootPath);

        MatchEvaluator evaluator = new MatchEvaluator(SurroundWithBlueColor);
        texteExplicatifDonneesHackesSuccess.AddReplacement("%Trace%", GetTrace());
        texteExplicatifDonneesHackesSuccess.AddReplacement("%Passe%", nextPassword);
        texteExplicatifDonneesHackesSuccess.AddReplacementEvaluator(@"Passes?", evaluator);
        texteExplicatifDonneesHackesSuccess.AddReplacementEvaluator(@"Traces?", evaluator);
        //texteExplicatifDonneesHackesSuccess.AddReplacementEvaluator(@"$", (Match match) => "blabla ! :D");
    }

    public bool HasJustWin() {
        string key = GetName() + HAS_JUST_WIN_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == "True";
    }

    public void SetNotJustWin() {
        string key = GetName() + HAS_JUST_WIN_KEY;
        PlayerPrefs.SetString(key, "False");
    }

    protected void DisplayPopupUnlockNewTreshold() {
        if (HasJustWin()) {
            List<int> tresholds = texteExplicatifDonneesHackesSuccess.GetAllTresholds();
            if(tresholds.Contains(GetNbWins())) {
                MenuManager.Instance.RunPopup("Pallier débloqué !", "Félicitation ! Vous venez de débloquer le pallier de" + (GetNbWins() > 1 ? "s" : "") + " " + GetNbWins() + " victoire" + ((GetNbWins() > 1) ? "s" : "") + " !\nAllez le consulter dans les Données Hackées !");
            }
            SetNotJustWin();
        }
    }
    
    protected void DisplayPopupUnlockLevel() {
        if (!HasAlreadyDiscoverLevel()) {
            if (menuLevelSelector.GetLevelIndice() != 0) {
                MenuManager.Instance.RunPopup("Niveau débloqué !", "Félicitation ! Vous venez de débloquer le niveau " + GetName() + " !\nContinuez comme ça !\nEt Happy Hacking ! :)");
            } else {
                texteExplicatifIntroduction.Run();
            }
            SetAlreadyDiscoverLevel();
        }
    }

    public bool HasAlreadyDiscoverLevel() {
        string key = GetName() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == "True";
    }

    public void SetAlreadyDiscoverLevel() {
        string key = GetName() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        PlayerPrefs.SetString(key, "True");
    }

    public bool IsPlayStarted() {
        return playStarted;
    }
    public void SetPlayStarted() {
        playStarted = true;
    }
}
