using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public class UIHelper {

    public enum UIColor {
        // Don't add colors in the middle to avoid breaking prefabs ! :)
        RED,
        GREEN,
        BLUE,
        WHITE,
        BLACK,
        CYAN,
        DARK_CYAN,
        ELECTRIC_BLUE,
        ORANGE,
        YELLOW,
        MAGENTA,
        VIOLET,
        GREY,
        PURE_RED,
        PURE_GREEN,
        PURE_BLUE,
    }

    public static string RED = "#ff0000ff";
    public static string GREEN = "#00ff00ff";
    public static string BLUE = "#0000ffff";
    public static string WHITE = "#ffffffff";
    public static string BLACK = "#000000ff";
    public static string CYAN = "#00ffffff";
    public static string DARK_CYAN = "#008888ff";
    public static string ELECTRIC_BLUE = "#88ffffff";
    public static string ORANGE = "#ff8800ff";
    public static string YELLOW = "#ffff00ff";
    public static string MAGENTA = "#ff00ffff";
    public static string VIOLET = "#7f00ffff";
    public static string GREY = "#777777ff";
    public static string PURE_RED = "#ff0000ff";
    public static string PURE_GREEN = "#00ff00ff";
    public static string PURE_BLUE = "#0000ffff";


    public static List<Tuple<string, string>> GetColorMapping() {
        List<Tuple<string, string>> colorMapping = new List<Tuple<string, string>>() {
            new Tuple<string, string>("#RED", RED),
            new Tuple<string, string>("#GREEN", GREEN),
            new Tuple<string, string>("#BLUE", BLUE),
            new Tuple<string, string>("#CYAN", CYAN),
            new Tuple<string, string>("#DARK_CYAN", DARK_CYAN),
            new Tuple<string, string>("#ELECTRIC_BLUE", ELECTRIC_BLUE),
            new Tuple<string, string>("#WHITE", WHITE),
            new Tuple<string, string>("#BLACK", BLACK),
            new Tuple<string, string>("#ORANGE", ORANGE),
            new Tuple<string, string>("#YELLOW", YELLOW),
            new Tuple<string, string>("#VIOLET", VIOLET),
            new Tuple<string, string>("#GREY", GREY),
            new Tuple<string, string>("#PURE_RED", PURE_RED),
            new Tuple<string, string>("#PURE_GREEN", PURE_GREEN),
            new Tuple<string, string>("#PURE_GREEN", PURE_GREEN),
            new Tuple<string, string>("#PURE_BLUE", PURE_BLUE),
        };
        return colorMapping;
    }

    public static bool IsOverflowing(string content, Text text) {
        float preferedWidth = LayoutUtility.GetPreferredWidth(text.rectTransform);
        float maxWidth = text.GetComponent<RectTransform>().rect.width;
        return preferedWidth > maxWidth;
    }

    public static void FitTextHorizontaly(string content, Text text) {
        while (text.fontSize > 1 && IsOverflowing(content, text))
            text.fontSize -= 1;
    }

    public static bool IsOverflowing(string content, TMPro.TMP_Text text) {
        float preferedWidth = LayoutUtility.GetPreferredWidth(text.rectTransform);
        float maxWidth = text.GetComponent<RectTransform>().rect.width;
        return preferedWidth > maxWidth;
    }

    public static void FitTextHorizontaly(string content, TMPro.TMP_Text text) {
        while (text.fontSize > 1 && IsOverflowing(content, text))
            text.fontSize -= 1;
    }

    public static string SurroundWithColor(string text, Color color) {
        string htmlColor = $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        return SurroundWithColor(text, htmlColor);
    }

    public static string SurroundWithColorWithoutB(string text, Color color) {
        string htmlColor = $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        return SurroundWithColorWithoutB(text, htmlColor);
    }

    public static string SurroundWithB(string text) {
        return $"<b>{text}</b>";
    }

    public static string SurroundWithColor(string text, string htmlColor) {
        return $"<color={htmlColor}><b>{text}</b></color>";
    }

    public static string SurroundWithColorWithoutB(string text, string htmlColor) {
        return $"<color={htmlColor}>{text}</color>";
    }

    public static List<Tuple<string, string>> GetReplacementList(ReplacementStrings stringsReplacements) {
        List<Tuple<string, string>> replacements = new List<Tuple<string, string>>();
        foreach(ReplacementString stringReplacement in stringsReplacements.replacements) {
            string s = stringReplacement.localizedString.GetLocalizedString().Result;
            replacements.Add(new Tuple<string, string>(s, UIHelper.SurroundWithColor(s, UIColor2String(stringReplacement.color))));
        }
        return replacements;
    }

    public static string ApplyReplacementList(string originalText, ReplacementStrings replacementStrings) {
        List<Tuple<string, string>> replacements = GetReplacementList(replacementStrings);
        foreach(Tuple<string, string> replacement in replacements) {
            originalText = ApplyReplacement(originalText, replacement);
        }
        return originalText;
    }

    public static string ApplyReplacement(string origialText, Tuple<string, string> replacement) {
        return origialText.Replace(replacement.Item1, replacement.Item2);
    }

    public static string UIColor2String(UIColor uiColor) {
        switch (uiColor) {
            case UIColor.RED: return RED;
            case UIColor.GREEN: return GREEN;
            case UIColor.BLUE: return BLUE;
            case UIColor.WHITE: return WHITE;
            case UIColor.BLACK: return BLACK;
            case UIColor.CYAN: return CYAN;
            case UIColor.DARK_CYAN: return DARK_CYAN;
            case UIColor.ELECTRIC_BLUE: return ELECTRIC_BLUE;
            case UIColor.ORANGE: return ORANGE;
            case UIColor.YELLOW: return YELLOW;
            case UIColor.MAGENTA: return MAGENTA;
            case UIColor.VIOLET: return VIOLET;
            case UIColor.PURE_RED: return PURE_RED;
            case UIColor.PURE_GREEN: return PURE_GREEN;
            case UIColor.PURE_BLUE: return PURE_BLUE;
            default: return MAGENTA;
        }
    }
}
