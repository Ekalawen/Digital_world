using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LevenshteinDifferences {
    public int nbSuppressions = 0;
    public int nbAjouts = 0;
    public int nbRemplacements = 0;
}

public class Trace {

    public enum AdviceType {
        COMPLETE,
        ONLY_TRACE,
        ONLY_PASSE,
        NO_ADVICE,
    };

    public static string GenerateTrace() {
        string trace = "";
        trace += GetRandomBit();
        trace += GetRandomBit();
        trace += GetRandomHexadecimal();
        trace += GetRandomHexadecimal();
        return trace;
    }

    public static char GetRandomMajLetter() {
        int start = 65; // A
        int end = 90; // Z
        return (char)UnityEngine.Random.Range(start, end + 1);
    }

    public static char GetRandomLowerLetter() {
        int start = 97; // a
        int end = 122; // z
        return (char)UnityEngine.Random.Range(start, end + 1);
    }

    public static char GetRandomBit() {
        List<char> bitPossibilities = Trace.GetBitPossibilities();
        return bitPossibilities[UnityEngine.Random.Range(0, bitPossibilities.Count)];
    }

    public static char GetRandomDigit() {
        int start = 48; // 0
        int end = 57; // 9
        return (char)UnityEngine.Random.Range(start, end + 1);
    }

    public static char GetRandomHexadecimal() {
        List<char> hexaPossibilities = Trace.GetHexadecimalPossibilities();
        return hexaPossibilities[UnityEngine.Random.Range(0, hexaPossibilities.Count)];
    }

    public static List<char> GetBitPossibilities() {
        return new List<char> { '0', '1' };
    }

    public static List<char> GetHexadecimalPossibilities() {
        return new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
    }

    static string RemoveDiacritics(string text) {
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString) {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool IsWellFormatedPassword(string password) {
        if (password.Length < 4)
            return false;
        string trace = password.Substring(0, 4);
        string passe = password.Substring(4, password.Length - 4);
        return IsWellFormatedTrace(trace) && IsWellFormatedPasse(passe);
    }

    protected static string GetStringForLocalizedStringReference(string reference, params object[] arguments) {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("PasswordAdvices", reference, arguments).Result;
    }

    public static string GetPasswordAdvice(string password, string truePassword, AdviceType adviceType) {
        switch (adviceType) {
            case AdviceType.COMPLETE:
                return GetPasswordAdviceComplete(password, truePassword);
            case AdviceType.ONLY_TRACE:
                return GetPasswordAdviceOnlyTrace(password, truePassword);
            case AdviceType.ONLY_PASSE:
                return GetPasswordAdviceOnlyPasse(password, truePassword);
            case AdviceType.NO_ADVICE:
                return "";
            default:
                return "Nope.";
        }
    }

    public static string GetPasswordAdviceComplete(string password, string truePassword) {
        if (password.Length < 4)
            return GetStringForLocalizedStringReference("AuMoins4Caracteres");
        string trace = password.Substring(0, 4);
        string passe = password.Substring(4, password.Length - 4);
        if(HasSwapTraceAndPasse(password, truePassword)) {
            return GetStringForLocalizedStringReference("TraceAvantPasse");
        }
        if(!IsWellFormatedTrace(trace)) {
            return GetTraceAdvice(trace);
        }
        if(!IsWellFormatedPasse(passe)) {
            return GetPasseAdvice(passe);
        }
        return GetWellFormatedPasswordAdvice(password, truePassword);
    }

    public static string GetPasswordAdviceOnlyTrace(string password, string truePassword) {
        string trace = password;
        if (trace.Length != 4)
            return GetStringForLocalizedStringReference("TraceExactement4Caracteres");
        if(!IsWellFormatedTrace(trace)) {
            return GetTraceAdvice(trace);
        }
        return GetWellFormatedOnlyTraceAdvice(password, truePassword);
    }

    public static string GetPasswordAdviceOnlyPasse(string password, string truePassword) {
        string passe = password;
        if(!IsWellFormatedPasse(passe)) {
            return GetPasseAdvice(passe);
        }
        return GetWellFormatedOnlyPasseAdvice(password, truePassword);
    }

    public static bool HasSwapTraceAndPasse(string password, string truePassword) {
        string trueTrace = truePassword.Substring(0, 4);
        return password.Length > 4 && password.Substring(password.Length - 4, 4) == trueTrace;
    }

    public static bool IsWellFormatedPasse(string passe) {
        bool noWhiteSpace = !passe.Any(c => Char.IsWhiteSpace(c));
        bool noUppercase = passe.ToLower() == passe;
        bool noAccents = RemoveDiacritics(passe) == passe;
        return noWhiteSpace && noUppercase && noAccents;
    }

    public static string GetPasseAdvice(string passe) {
        bool noWhiteSpace = !passe.Any(c => Char.IsWhiteSpace(c));
        bool noUppercase = passe.ToLower() == passe;
        bool noAccents = RemoveDiacritics(passe) == passe;
        if (!noWhiteSpace) {
            return GetStringForLocalizedStringReference("PassePasDEspaces");
        }
        if(!noUppercase) {
            return GetStringForLocalizedStringReference("PassePasDeMajuscules");
        }
        if(!noAccents) {
            return GetStringForLocalizedStringReference("PassePasDAccents");
        }
        return "";
    }

    public static bool IsWellFormatedTrace(string trace) {
        if (trace.Length != 4)
            return false;
        return IsBit(trace[0]) && IsBit(trace[1]) && IsHexadecimal(trace[2]) && IsHexadecimal(trace[3]);
    }

    public static string GetTraceAdvice(string trace) {
        if (trace.Length != 4)
            return GetStringForLocalizedStringReference("Trace4Caracteres");
        if(!IsBit(trace[0])) {
            return GetStringForLocalizedStringReference("TracePremierCaractere");
        }
        if(!IsBit(trace[1])) {
            return GetStringForLocalizedStringReference("TraceDeuxiemeCaractere");
        }
        if(!IsHexadecimal(trace[2])) {
            return GetStringForLocalizedStringReference("TraceTroisiemeCaractere");
        }
        if(!IsHexadecimal(trace[3])) {
            return GetStringForLocalizedStringReference("TraceQuatriemeCaractere");
        }
        return "";
    }

    public static bool IsBit(char c) {
        return GetBitPossibilities().Contains(c);
    }

    public static bool IsHexadecimal(char c) {
        return GetHexadecimalPossibilities().Contains(c);
    }

    public static string GetWellFormatedPasswordAdvice(string password, string truePassword) {
        string trace = password.Substring(0, 4);
        string trueTrace = truePassword.Substring(0, 4);
        string passe = password.Substring(4, password.Length - 4);
        string truePasse = truePassword.Substring(4, truePassword.Length - 4);
        bool goodTrace = trace == trueTrace;
        bool goodPasse = passe == truePasse;
        if (!goodTrace) {
            return GetStringForLocalizedStringReference("MauvaiseTrace");
        }
        int[,] levenshteinMatrice = GetLevenshteinMatrice(passe, truePasse);
        if (GetDistanceDeLevenshtein(passe, truePasse, levenshteinMatrice) <= 4) {
            LevenshteinDifferences differences = GetDifferencesDeLevenshtein(passe, truePasse, levenshteinMatrice);
            Debug.Log($"{differences.nbRemplacements} {differences.nbAjouts} {differences.nbSuppressions}");
            return GetStringForLocalizedStringReference("PasseLevenshtein", differences.nbAjouts, differences.nbSuppressions, differences.nbRemplacements);
        }
        return GetStringForLocalizedStringReference("MauvaisPasse");
    }

    public static string GetWellFormatedOnlyTraceAdvice(string password, string truePassword) {
        string trace = password;
        string trueTrace = truePassword;
        bool goodTrace = trace == trueTrace;
        if (!goodTrace) {
            return GetStringForLocalizedStringReference("MauvaiseTrace");
        }
        return ""; // On ne devrait pas arriver ici pour les only Trace !
    }

    public static string GetWellFormatedOnlyPasseAdvice(string password, string truePassword) {
        string passe = password;
        string truePasse = truePassword;
        bool goodPasse = passe == truePasse;
        int[,] levenshteinMatrice = GetLevenshteinMatrice(passe, truePasse);
        if (GetDistanceDeLevenshtein(passe, truePasse, levenshteinMatrice) <= 4) {
            LevenshteinDifferences differences = GetDifferencesDeLevenshtein(passe, truePasse, levenshteinMatrice);
            Debug.Log($"{differences.nbRemplacements} {differences.nbAjouts} {differences.nbSuppressions}");
            return GetStringForLocalizedStringReference("PasseLevenshtein", differences.nbAjouts, differences.nbSuppressions, differences.nbRemplacements);
        }
        return GetStringForLocalizedStringReference("MauvaisPasse");
    }

    public static int[,] GetLevenshteinMatrice(string passe, string truePasse) {
        int N = passe.Length;
        int M = truePasse.Length;
        int[,] dists = new int[N + 1, M + 1];
        for(int i = 0; i <= N; i++) {
            dists[i, 0] = i;
        }
        for(int j = 0; j <= M; j++) {
            dists[0, j] = j;
        }

        for(int i = 1; i <= N; i++) {
            for(int j = 1; j <= M; j++) {
                int coutSubstitution = 1;
                if (passe[i-1] == truePasse[j-1])
                    coutSubstitution = 0;
                int modifyPasse = dists[i - 1, j] + 1;
                int modifyTruePasse = dists[i, j - 1] + 1;
                int substitution = dists[i - 1, j - 1] + coutSubstitution;
                dists[i, j] = Mathf.Min(modifyPasse, modifyTruePasse, substitution);
            }
        }
        return dists;
    }

    public static int GetDistanceDeLevenshtein(string passe, string truePasse, int[,] matrice) {
        int N = passe.Length;
        int M = truePasse.Length;
        return matrice[N, M];
    }

    public static LevenshteinDifferences GetDifferencesDeLevenshtein(string passe, string truePasse, int[,] matrice) {
        Debug.Log($"{passe} + {truePasse}");
        int N = passe.Length;
        int M = truePasse.Length;
        LevenshteinDifferences differences = new LevenshteinDifferences();
        int i = N;
        int j = M;
        while(i != 0 || j != 0) {
            if(i == 0) {
                differences.nbAjouts++;
                j--;
            } else if (j == 0) {
                differences.nbSuppressions++;
                i--;
            } else {
                if (passe[i - 1] == truePasse[j - 1]) {
                    i--;
                    j--;
                } else {
                    int dist = matrice[i, j];
                    int preDist = Mathf.Max(0, dist - 1);
                    if (matrice[i - 1, j - 1] == preDist) {
                        differences.nbRemplacements++;
                        i--;
                        j--;
                    } else if (matrice[i - 1, j] == preDist) {
                        differences.nbSuppressions++;
                        i--;
                    } else {
                        differences.nbAjouts++;
                        j--;
                    }
                }
            }
        }
        return differences;
    }
}
