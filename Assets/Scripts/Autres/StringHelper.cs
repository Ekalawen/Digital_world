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
}

