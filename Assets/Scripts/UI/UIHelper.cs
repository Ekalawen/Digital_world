using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UIHelper {

    public static string RED = "#ff0000ff";
    public static string GREEN = "#00ff00ff";
    public static string BLUE = "#0000ffff";
    public static string WHITE = "#ffffffff";
    public static string BLACK = "#000000ff";
    public static string CYAN = "#00ffffff";
    public static string DARK_CYAN = "#008888ff";
    public static string ELECTRIC_BLUE = "#88ffffff";
    public static string ORANGE = "#ff8888ff";
    public static string YELLOW = "#ffff00ff";
    public static string PURE_RED = "#ff0000ff";
    public static string PURE_GREEN = "#00ff00ff";
    public static string PURE_BLUE = "#0000ffff";


    public static List<Tuple<string, string>> GetColorMapping() {
        List<Tuple<string, string>> colorMapping = new List<Tuple<string, string>>()
        {
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

    public static string SurroundWithColor(string text, string htmlColor) {
        return $"<color={htmlColor}><b>{text}</b></color>";
    }

    public static string SurroundWithColorWithoutB(string text, string htmlColor) {
        return $"<color={htmlColor}>{text}</color>";
    }
}
