using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class StringHelper {

    public static string GetKeyFor(string keySuffix) {
        string levelNameKey = SceneManager.GetActiveScene().name;
        return levelNameKey + keySuffix;
    }

    public static string ToCreditsFormat(int creditsCount) {
        //System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
        //return $"{creditsCount:N3}";
        string res = "";
        int divider = 1;
        while(divider * 1000 <= creditsCount) {
            divider *= 1000;
        }
        int creditsRest = creditsCount;
        while (divider > 1) {
            int quotient = creditsRest / divider;
            if (creditsRest != creditsCount) {
                res += $"{quotient:D3}.";
            } else {
                res += $"{quotient}.";
            }
            creditsRest = creditsRest % divider;
            divider /= 1000;
        }
        if (creditsCount >= 1000) {
            res += $"{creditsRest:D3}";
        } else {
            res += $"{creditsRest}";
        }
        return res;
    }

    public static string ToCreditsShortFormat(long value) {
        List<Tuple<long, string>> mapping = GetCreditsSuffixMapping();
        foreach(Tuple<long, string> map in mapping) {
            long unit = map.Item1;
            string symbol = map.Item2;
            if(value > unit) {
                long quotient = value / unit;
                string quotientString = quotient >= 10 ? quotient.ToString() : quotient.ToString(".1f");
                return $"{quotientString}{symbol}";
            }
        }
        return value.ToString();
    }

    private static List<Tuple<long, string>> mapping = null;
    private static List<Tuple<long, string>> GetCreditsSuffixMapping() {
        if (mapping == null) {
            mapping = new List<Tuple<long, string>>();
            mapping.Add(new Tuple<long, string>(1_000_000_000_000_000_000, "Qi"));
            mapping.Add(new Tuple<long, string>(1_000_000_000_000_000, "Q"));
            mapping.Add(new Tuple<long, string>(1_000_000_000_000, "T"));
            mapping.Add(new Tuple<long, string>(1_000_000_000, "B"));
            mapping.Add(new Tuple<long, string>(1_000_000, "M"));
            mapping.Add(new Tuple<long, string>(1_000, "K"));
        }
        return mapping;
    }
}

