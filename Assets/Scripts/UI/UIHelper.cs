using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIHelper {

    public static bool IsOverflowing(string content, Text text) {
        float preferedWidth = LayoutUtility.GetPreferredWidth(text.rectTransform);
        float maxWidth = text.GetComponent<RectTransform>().rect.width;
        return preferedWidth > maxWidth;
    }

    public static void FitTextHorizontaly(string content, Text text) {
        while (text.fontSize > 1 && IsOverflowing(content, text))
            text.fontSize -= 1;
    }
}
