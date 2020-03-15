using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManagerBlackAndWhite : ColorManager {

    public List<ColorSource.ThemeSource> primaryTheme;
    public List<ColorSource.ThemeSource> secondaryTheme;
    public Color primaryColor;
    public Color secondaryColor;

    protected bool isCurrentlyPrimaryTheme = true;

    public override void Initialize() {
        base.Initialize();
    }

    public void SwapTheme() {
        isCurrentlyPrimaryTheme = !isCurrentlyPrimaryTheme;
    }

    public List<ColorSource.ThemeSource> GetCurrentTheme() {
        return isCurrentlyPrimaryTheme ? primaryTheme : secondaryTheme;
    }

    public List<ColorSource.ThemeSource> GetNotCurrentTheme() {
        return !isCurrentlyPrimaryTheme ? primaryTheme : secondaryTheme;
    }

    public Color GetCurrentColor() {
        //return isCurrentlyPrimaryTheme ? primaryColor : secondaryColor;
        return primaryColor;
    }

    public Color GetNotCurrentColor() {
        //return !isCurrentlyPrimaryTheme ? primaryColor : secondaryColor;
        return secondaryColor;
    }
}
