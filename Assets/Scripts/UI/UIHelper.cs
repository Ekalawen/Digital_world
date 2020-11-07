﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIHelper {

    public static string RED = "#800000ff";
    public static string GREEN = "#008000ff";
    public static string BLUE = "#0000ffff";
    public static string CYAN = "#00ffffff";
    public static string PURE_RED = "#ff0000ff";
    public static string PURE_GREEN = "#00ff00ff";
    public static string PURE_BLUE = "#0000ffff";

    public static bool IsOverflowing(string content, Text text) {
        float preferedWidth = LayoutUtility.GetPreferredWidth(text.rectTransform);
        float maxWidth = text.GetComponent<RectTransform>().rect.width;
        return preferedWidth > maxWidth;
    }

    public static void FitTextHorizontaly(string content, Text text) {
        while (text.fontSize > 1 && IsOverflowing(content, text))
            text.fontSize -= 1;
    }

    public static string SurroundWithColor(string text, Color color) {
        string htmlColor = $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        return SurroundWithColor(text, htmlColor);
    }

    public static string SurroundWithColor(string text, string htmlColor) {
        return $"<color={htmlColor}>{text}</color>";
    }
}
