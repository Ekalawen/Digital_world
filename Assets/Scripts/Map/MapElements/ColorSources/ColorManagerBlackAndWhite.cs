using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManagerBlackAndWhite : ColorManager {

    public List<ColorManager.Theme> primaryTheme;
    public List<ColorManager.Theme> secondaryTheme;
    public Color primaryColor;
    public Color secondaryColor;

    protected bool isCurrentlyPrimaryTheme = true;

    public override void Initialize() {
        base.Initialize();
    }

    public void SwapTheme() {
        isCurrentlyPrimaryTheme = !isCurrentlyPrimaryTheme;
    }

    public List<ColorManager.Theme> GetCurrentTheme() {
        return isCurrentlyPrimaryTheme ? primaryTheme : secondaryTheme;
    }

    public List<ColorManager.Theme> GetNotCurrentTheme() {
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
