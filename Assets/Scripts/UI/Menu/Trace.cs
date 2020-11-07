using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trace {

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

    public static bool IsWellFormatedPassword(string password) {
        if (password.Length < 4)
            return false;
        string trace = password.Substring(0, 4);
        string passe = password.Substring(4, password.Length - 4);
        return IsWellFormatedTrace(trace) && IsWellFormatedPasse(passe);
    }

    public static string GetPasswordAdvice(string password, string truePassword) {
        if (password.Length < 4)
            return "Un password a toujours au moins 4 caractères.";
        string trace = password.Substring(0, 4);
        string passe = password.Substring(4, password.Length - 4);
        if(!IsWellFormatedTrace(trace)) {
            return GetTraceAdvice(trace);
        }
        if(!IsWellFormatedPasse(passe)) {
            return GetPasseAdvice(passe);
        }
        return GetWellFormatedPasswordAdvice(password, truePassword);
    }

    public static bool IsWellFormatedPasse(string passe) {
        bool noWhiteSpace = !passe.Any(c => Char.IsWhiteSpace(c));
        bool noUppercase = passe.ToLower() == passe;
        return noWhiteSpace && noUppercase;
    }

    public static string GetPasseAdvice(string passe) {
        bool noWhiteSpace = !passe.Any(c => Char.IsWhiteSpace(c));
        bool noUppercase = passe.ToLower() == passe;
        if(!noWhiteSpace) {
            return "Un Passe ne doit pas contenir d'espaces ou de caractères blancs.";
        }
        if(!noUppercase) {
            return "Un Passe ne doit pas contenir de lettres majuscules.";
        }
        return null;
    }

    public static bool IsWellFormatedTrace(string trace) {
        if (trace.Length != 4)
            return false;
        return IsBit(trace[0]) && IsBit(trace[1]) && IsHexadecimal(trace[2]) && IsHexadecimal(trace[3]);
    }

    public static string GetTraceAdvice(string trace) {
        if (trace.Length != 4)
            return "Une Trace contient toujours exactement 4 caractères.";
        if(!IsBit(trace[0])) {
            return "Le premier caractère de la Trace doit être soit '0', soit '1'.";
        }
        if(!IsBit(trace[1])) {
            return "Le deuxième caractère de la Trace doit être soit '0', soit '1'.";
        }
        string hexadecimalValues = $"[{String.Join(", ", GetHexadecimalPossibilities())}]";
        if(!IsHexadecimal(trace[2])) {
            return $"Le troisième caractère de la Trace doit être contenu dans {hexadecimalValues}.";
        }
        if(!IsHexadecimal(trace[3])) {
            return $"Le quatrième caractère de la Trace doit être contenu dans {hexadecimalValues}.";
        }
        return null;
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
            return "Ce n'est pas la bonne Trace.";
        }
        int levenshteinDist = GetDistanceDeLevenshtein(passe, truePasse);
        if (levenshteinDist <= 6) {
            return $"Presque ! Il vous faut faire {levenshteinDist} ajout, suppression ou remplacement dans votre Passe pour arriver au bon Passe.";
        }
        return "Ce n'est pas le bon Passe.";
    }

    public static int GetDistanceDeLevenshtein(string passe, string truePasse) {
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
        return dists[N, M];
    }
}
