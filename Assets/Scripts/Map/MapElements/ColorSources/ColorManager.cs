using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct SavedTheme {
    public string key;
    public List<ColorManager.Theme> themes;
}
[System.Serializable]
public struct SavedColor {
    public string key;
    public Color color;
}

public class ColorManager : MonoBehaviour {

    public enum MainColor {
        RED,
        GREEN,
        BLUE,
    }

	public enum Theme {
        ROUGE,  // Ne pas changer l'ordre de l'énumération, sinon ça change les prefabs !
        ORANGE,
        JAUNE,
        VERT_CLAIR,
        VERT,
        VERT_NEON,
        CYAN,
        BLEU_GLACE,
        BLEU,
        VIOLET,
        ROSE,
        BLANC,
        NOIR,
        MULTICOLOR,
        MULTICOLOR_NON_BRIGHT,
        RANDOM,
        RANDOM_NON_BRIGHT,
    };

    public GameObject colorSourcePrefab;
    public bool bGenerateInCubes = false;

	// On peut choisir le thème du cube =)
	// 0.0 = automne, 0.1 = jaune, 0.3 = vert, O.5 = bleu, 0.65 = violet, 0.8 = rose, 0.9 = rouge
	public List<ColorManager.Theme> themes;
	public List<SavedTheme> savedThemes;

    // Il faudra changer ça, les sources ne sont pas forcément des cubes !?
	public float frequenceSources; // La frequence qu'un emplacement soit une source
	public Vector2 porteeSourceRange;
    public float cubeLuminanceMax; // Le maximum de luminosité d'un cube (pour éviter d'éblouir le joueur)
    public List<SavedColor> savedColors; // Utile si on a besoin de certaines couleurs à certains moments dans le jeu !

    protected MapManager map;
    [HideInInspector]
    public GameObject colorSourceFolder;

    protected Octree<ColorSource> sourcesOctree = new Octree<ColorSource>(cellSize: 8);

    public virtual void Initialize() {
        map = FindObjectOfType<MapManager>();
        colorSourceFolder = new GameObject("ColorSources");

        themes = ReplaceNonDeterministicColors(themes);

        if (!bGenerateInCubes)
            GenerateColorSources(map.tailleMap);
        else
            GenerateColorSourcesInCubes(map.GetAllCubes());
        Debug.Log("Nombre color sources = " + GetAllColorSources().Count);
        CheckCubeSaturation();

        PrintColorStats();
    }

    protected void PrintColorStats() {
        Debug.Log("Nombre color sources = " + GetAllColorSources().Count);
        string s = "";
        foreach(Theme theme in themes) {
            s = $"{s}{theme}, ";
        }
        Debug.Log(s.Substring(0, s.Length - 2));
    }

    public static List<Theme> ReplaceNonDeterministicColors(List<Theme> themes) {
        List<Theme> newThemes = new List<Theme>();
        for (int i = 0; i < themes.Count; i++) {
            if (themes[i] == Theme.RANDOM) {
                newThemes.Add(GetRandomTheme());
            } else if (themes[i] == Theme.RANDOM_NON_BRIGHT) {
                newThemes.Add(GetRandomNonBrightTheme());
            } else {
                newThemes.Add(themes[i]);
            }
        }
        return newThemes;
    }

    public static Theme GetRandomTheme() {
        List<Theme> themesPossibles = new List<Theme>() {
            Theme.ROUGE,
            Theme.ORANGE,
            Theme.JAUNE,
            Theme.VERT_CLAIR,
            Theme.VERT,
            Theme.VERT_NEON,
            Theme.CYAN,
            Theme.BLEU_GLACE,
            Theme.BLEU,
            Theme.VIOLET,
            Theme.ROSE,
            Theme.BLANC,
            Theme.MULTICOLOR,
            Theme.MULTICOLOR_NON_BRIGHT,
        };

        return themesPossibles[Random.Range(0, themesPossibles.Count)];
    }

    public static Theme GetRandomNonMulticolorTheme() {
        List<Theme> themesPossibles = new List<Theme>() {
            Theme.ROUGE,
            Theme.ORANGE,
            Theme.JAUNE,
            Theme.VERT_CLAIR,
            Theme.VERT,
            Theme.VERT_NEON,
            Theme.CYAN,
            Theme.BLEU_GLACE,
            Theme.BLEU,
            Theme.VIOLET,
            Theme.ROSE,
            Theme.BLANC,
        };

        return themesPossibles[Random.Range(0, themesPossibles.Count)];
    }

    public static Theme GetRandomNonBrightTheme() {
        List<Theme> themesPossibles = new List<Theme>() {
            Theme.ROUGE,
            Theme.ORANGE,
            Theme.VERT_CLAIR,
            Theme.VERT,
            Theme.VERT_NEON,
            Theme.CYAN,
            Theme.BLEU_GLACE,
            Theme.BLEU,
            Theme.VIOLET,
            Theme.ROSE,
            Theme.MULTICOLOR_NON_BRIGHT,
            Theme.MULTICOLOR_NON_BRIGHT, // La deuxième fois pour compenser l'abscence du Multicolor tout court ;)
        };

        return themesPossibles[Random.Range(0, themesPossibles.Count)];
    }
    public static Theme GetRandomNonBrightNonMulticolorTheme() {
        List<Theme> themesPossibles = new List<Theme>() {
            Theme.ROUGE,
            Theme.ORANGE,
            Theme.VERT_CLAIR,
            Theme.VERT,
            Theme.VERT_NEON,
            Theme.CYAN,
            Theme.BLEU_GLACE,
            Theme.BLEU,
            Theme.VIOLET,
            Theme.ROSE,
        };

        return themesPossibles[Random.Range(0, themesPossibles.Count)];
    }

    public void GenerateColorSources(Vector3Int size) {
        // On calcul le nombre de sources
        int N = size.x * size.y * size.z;
        float P = frequenceSources;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSources = (int)GaussianGenerator.Next(mean, variance, 0, N);

        // Puis on les réparties ! C'est pas grâve si plusieurs sources sont au même endroit !
        for(int i = 0; i < nbSources; i++) {
            Vector3Int pos = new Vector3Int(Random.Range(0, size.x), Random.Range(0, size.y), Random.Range(0, size.z));
            GenerateColorSourceAt(pos);
        }
    }

    public void GenerateColorSourcesInCubes(List<Cube> cubes, int nbSourcesMin = 0) {
        // On calcul le nombre de sources
        List<Vector3> possiblesPos = new List<Vector3>();
        foreach (Cube cube in cubes) possiblesPos.Add(cube.transform.position);
        int N = possiblesPos.Count;
        float P = frequenceSources;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSources = (int)GaussianGenerator.Next(mean, variance, 0, N);
        nbSources = Mathf.Max(nbSources, nbSourcesMin);

        // Puis on les réparties ! C'est pas grâve si plusieurs sources sont au même endroit !
        for(int i = 0; i < nbSources; i++) {
            int indice = Random.Range(0, N);
            Vector3 pos = possiblesPos[indice];
            possiblesPos.RemoveAt(indice);
            N--;
            GenerateColorSourceAt(pos);
        }
    }

    public void GenerateColorSourceAt(Vector3 position) {
        GameObject go = Instantiate(colorSourcePrefab, position, Quaternion.identity, colorSourceFolder.transform);
        ColorSource source = go.GetComponent<ColorSource>();
        source.Initialize(GetColor(themes), Random.Range(porteeSourceRange[0], porteeSourceRange[1]));
        sourcesOctree.Add(go.GetComponent<ColorSource>());
    }
    public static Color GetColor(ColorManager.Theme theme) {
        return GetColor(new List<ColorManager.Theme>() { theme });
    }

	public static Color GetColor(List<ColorManager.Theme> currentThemes) {
		Color c;
        ColorManager.Theme themeChoisi = currentThemes[Random.Range(0, currentThemes.Count)];
		switch (themeChoisi) {
			case ColorManager.Theme.ROUGE:
				c = Color.HSVToRGB(Random.Range(0.99f, 1.01f), 1f, 0.7f); break;
			case ColorManager.Theme.ORANGE:
				c = Color.HSVToRGB(Random.Range(0.07f, 0.09f), 1f, 0.6f); break;
			case ColorManager.Theme.JAUNE:
                c = Color.HSVToRGB(Random.Range(0.16f, 0.18f), 1f, 0.7f); break;
            case ColorManager.Theme.VERT_CLAIR:
				c = Color.HSVToRGB(Random.Range(0.20f, 0.26f), 1f, 0.5f); break;
			case ColorManager.Theme.VERT:
				c = Color.HSVToRGB(Random.Range(0.30f, 0.36f), 1f, 0.5f); break;
			case ColorManager.Theme.VERT_NEON:
				c = Color.HSVToRGB(Random.Range(0.40f, 0.46f), 1f, 0.5f); break;
			case ColorManager.Theme.CYAN:
				c = Color.HSVToRGB(Random.Range(0.47f, 0.53f), 1f, 0.7f); break;
			case ColorManager.Theme.BLEU_GLACE:
				c = Color.HSVToRGB(Random.Range(0.54f, 0.60f), 1f, 0.7f); break;
			case ColorManager.Theme.BLEU:
				c = Color.HSVToRGB(Random.Range(0.63f, 0.69f), 1f, 0.4f); break;
			case ColorManager.Theme.VIOLET:
				c = Color.HSVToRGB(Random.Range(0.72f, 0.78f), 1f, 0.7f); break;
			case ColorManager.Theme.ROSE:
				c = Color.HSVToRGB(Random.Range(0.84f, 0.88f), 1f, 0.7f); break;
			case ColorManager.Theme.BLANC:
				c = Color.HSVToRGB(0f, 0f, 0.6f); break;
			case ColorManager.Theme.NOIR:
				c = Color.HSVToRGB(0f, 0f, 0.001f); break;
			case ColorManager.Theme.MULTICOLOR:
				c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 0.7f); break;
			case ColorManager.Theme.MULTICOLOR_NON_BRIGHT:
                c = GetColor(new List<Theme>() { GetRandomNonBrightTheme() }); break;
			case ColorManager.Theme.RANDOM:
                c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f); break;
			default:
				c = Color.black;
				Debug.LogWarning($"Je ne connais pas ce thème {themeChoisi} !");
                break;
		}
		return c;
	}

    public List<ColorSource> GetAllColorSourcesInSphere(Vector3 center, float range) {
        return sourcesOctree.GetInSphere(center, range);
    }

    public void CheckCubeSaturation() {
        List<Cube> cubes = map.GetAllCubes();
        List<Vector3> positions = cubes.Select(c => c.transform.position).ToList();
        List<Vector3> emptyPositions = map.GetAllEmptyPositions();
        positions.AddRange(emptyPositions);
        CheckCubeSaturationInCubes(positions);
        EnsureNoCubeIsBlack(cubes);
        EnsureNoPositionIsBlack(emptyPositions);
    }

    public void CheckCubeSaturationInCubes(List<Vector3> positions) {
        MathTools.Shuffle(positions);

        foreach(Vector3 position in positions) {
            float luminance = GetLuminance(GetColorForPosition(position));
            while(luminance > cubeLuminanceMax) {
                float savedLuminance = luminance;
                RemoveClosestSource(position);
                luminance = GetLuminance(GetColorForPosition(position));

                if(savedLuminance == luminance) { // Car des fois supprimer une lumière ne change plus rien à la couleur !
                    break;
                }
            }
        }
    }

    public void EnsureNoCubeIsBlack(List<Cube> cubes) {
        MathTools.Shuffle(cubes);

        foreach(Cube cube in cubes) {
            if (!cube.GetComponent<NonBlackCube>()) {
                if (cube.GetLuminance() < 0.01f) {
                    GenerateColorSourceAt(cube.transform.position);
                }
            } else {
                if (cube.GetColor() == ColorSource.LimiteColorSaturation(Color.black, NonBlackCube.minColorSaturationAndValue)) {
                    GenerateColorSourceAt(cube.transform.position);
                }
            }
        }
    }

    protected void EnsureNoPositionIsBlack(List<Vector3> positions) {
        MathTools.Shuffle(positions);

        foreach(Vector3 position in positions) {
            float luminance = GetLuminance(GetColorForPosition(position));
            if (luminance < 0.01f) {
                GenerateColorSourceAt(position);
            }
        }
    }

    public List<ColorSource> GetClosestsColorSources(Vector3 pos, int nb)
    {
        List<ColorSource> sortedSources = sourcesOctree.GetAll();
        sortedSources.Sort(delegate (ColorSource A, ColorSource B) {
            float distAToStart = Vector3.Distance(A.transform.position, pos);
            float distBToStart = Vector3.Distance(B.transform.position, pos);
            return distBToStart.CompareTo(distAToStart);
        });
        return sortedSources.Take(nb).ToList();
    }

    public List<ColorSource> GetColorSourcesInRange(Vector3 pos, float range) {
        return sourcesOctree.GetAll().Where(source => Vector3.Distance(source.transform.position, pos) <= range).ToList();
    }

    protected void RemoveClosestSource(Vector3 pos) {
        List<ColorSource> colorSources = sourcesOctree.GetAll();
        float distMin = Vector3.Distance(pos, colorSources.First().transform.position);
        ColorSource closest = colorSources.First();
        foreach(ColorSource source in colorSources) {
            float dist = Vector3.Distance(pos, source.transform.position);
            if(dist < distMin) {
                distMin = dist;
                closest = source;
            }
        }
        sourcesOctree.Remove(closest);
        closest.Delete();
    }

    public static Color InterpolateColors(Color c1, Color c2, float avancement = 0.5f) {
        return (1 - avancement) * c1 + avancement * c2;
    }

    public List<ColorSource> GetAllColorSources() {
        return sourcesOctree.GetAll();
    }

    public void ChangeTheme(List<ColorManager.Theme> newThemes) {
        themes = newThemes;
        foreach(ColorSource source in sourcesOctree.GetAll()) {
            source.ChangeColor(GetColor(themes));
        }
    }

    public List<ColorManager.Theme> GetSavedTheme(string key) {
        foreach(SavedTheme theme in savedThemes) {
            if (theme.key == key)
                return theme.themes;
        }
        Debug.LogError("Le theme " + key + " n'a pas été trouvé dans les savedThemes !");
        return null;
    }

    public Color GetSavedColor(string key) {
        foreach(SavedColor color in savedColors) {
            if (color.key == key)
                return color.color;
        }
        Debug.LogError("La couleur " + key + " n'a pas été trouvé dans les savedColors !");
        return Color.magenta;
    }

    public static bool AreSameThemes(List<ColorManager.Theme> t1, List<ColorManager.Theme> t2) {
        return MathTools.AreListEqual(t1, t2);
    }

    public Color GetColorForPosition(Vector3 pos) {
        Color color = Color.black;
        foreach(ColorSource source in sourcesOctree.GetAll()) {
            if (source != null) {
                float dist = Vector3.Distance(pos, source.transform.position);
                if (dist < source.range) {
                    color += source.GetColorForPosition(pos);
                }
            }
        }
        return color;
    }

    public Color GetNotBlackColorForPosition(Vector3 pos) {
        Color color = GetColorForPosition(pos);
        color = ColorSource.LimiteColorSaturation(color);
        return color;
    }

    public static float GetLuminance(Color rgb) {
        return RGBtoCIELAB(rgb)[0] / 100f;
    }

    public static float GetContrastRatioDangerous(Color color1, Color color2) {
        // 1 is the minimum
        // There is no maximum, but 4 or 5 is a lot
        float color1Luminance = GetContrastRatioLuminanceDangerous(color1);
        float color2Luminance = GetContrastRatioLuminanceDangerous(color2);
        float maxLuminance = Mathf.Max(color1Luminance, color2Luminance);
        float minLuminance = Mathf.Min(color1Luminance, color2Luminance);
        return (maxLuminance + 0.05f) / (minLuminance + 0.05f);
    }

    public static float GetContrastRatioLuminanceDangerous(Color color) {
        float R = GetContrastRatioLuminanceChannelDangerous(color.r);
        float G = GetContrastRatioLuminanceChannelDangerous(color.g);
        float B = GetContrastRatioLuminanceChannelDangerous(color.b);
        return 0.2126f * R + 0.7152f * G + 0.0722f * B;
    }

    protected static float GetContrastRatioLuminanceChannelDangerous(float value) {
        if(value <= 0.03928f) {
            return value / 12.92f;
        } else {
            return Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
        }
    }

    public static Vector3 RGBtoXYZ(Color rgb) {
        float R = RGBtoXYZChannel(rgb.r);
        float G = RGBtoXYZChannel(rgb.g);
        float B = RGBtoXYZChannel(rgb.b);
        R *= 100;
        G *= 100;
        B *= 100;
        float X = 0.4124f * R + 0.3576f * G + 0.1805f * B;
        float Y = 0.2126f * R + 0.7152f * G + 0.0722f * B;
        float Z = 0.0193f * R + 0.1192f * G + 0.9505f * B;
        return new Vector3(X, Y, Z);
    }

    protected static float RGBtoXYZChannel(float value) {
        if(value > 0.04045f) {
            return Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
        } else {
            return value / 12.92f;
        }
    }


    public static Color XYZtoRGB(Vector3 xyz) {
        float X = xyz.x / 100;
        float Y = xyz.y / 100;
        float Z = xyz.z / 100;
        float R = 3.2406f * X + -1.5372f * Y + -0.4986f * Z;
        float G = -0.9689f * X + 1.8758f * Y + 0.0415f * Z;
        float B = 0.0557f * X + -0.2040f * Y + 1.0570f * Z;
        return new Color(R, G, B);
    }

    protected static float XYZtoRGBChannel(float value) {
        if(value > 0.0031308f) {
            return 1.055f * (Mathf.Pow(value, 1.0f / 2.4f)) - 0.055f;
        } else {
            return 12.92f * value;
        }
    }

    public static Vector3 XYZtoCIELAB(Vector3 xyz) {
        // On prend 'E' donc Equal Energy
        float X = xyz.x / 100f;
        float Y = xyz.y / 100f;
        float Z = xyz.z / 100f;
        X = XYZtoCIELABChannel(X);
        Y = XYZtoCIELABChannel(Y);
        Z = XYZtoCIELABChannel(Z);
        float L = 116f * Y - 16f;
        float A = 500f * (X - Y);
        float B = 200f * (Y - Z);
        return new Vector3(L, A, B);
    }

    protected static float XYZtoCIELABChannel(float value) {
        if(value > 0.008856f) {
            return Mathf.Pow(value, 1.0f / 3.0f);
        } else {
            return 7.787f * value + 16f / 116f;
        }
    }

    public static Vector3 CIELABtoXYZ(Vector3 cielab) {
        float L = cielab[0];
        float A = cielab[1];
        float B = cielab[2];
        float Y = (L + 16f) / 116f;
        float X = A / 500f + Y;
        float Z = Y - B / 200f;
        X = CIELABtoXYZChannel(X);
        Y = CIELABtoXYZChannel(Y);
        Z = CIELABtoXYZChannel(Z);
        X *= 100f;
        Y *= 100f;
        Z *= 100f;
        return new Vector3(X, Y, Z);
    }

    protected static float CIELABtoXYZChannel(float value) {
        if(value > 0.008856f) {
            return Mathf.Pow(value, 3f);
        } else {
            return (value - (16f / 116f)) / 7.787f;
        }
    }

    public static Vector3 RGBtoCIELAB(Color rgb) {
        return XYZtoCIELAB(RGBtoXYZ(rgb));
    }

    public static Color CIELABtoRGB(Vector3 cielab) {
        return XYZtoRGB(CIELABtoXYZ(cielab));
    }

    public static float ColorDistanceCIELAB(Color rgb1, Color rgb2) {
        Vector3 cielab1 = RGBtoCIELAB(rgb1);
        Vector3 cielab2 = RGBtoCIELAB(rgb2);
        return Vector3.Distance(cielab1, cielab2) / 100.5168f; // où 100,5168f est la distance du blanc au noir pour avoir une distance dans [0, 1] :)
    }

    public void MakeAllColorSourcesBounceToColor(Color targetColor, float fadeIn, float fadeOut) {
        StartCoroutine(CMakeAllColorSourcesBounceToColor(targetColor, fadeIn, fadeOut));
    }

    public IEnumerator CMakeAllColorSourcesBounceToColor(Color targetColor, float fadeIn, float fadeOut) {
        List<Color> initialColors = sourcesOctree.GetAll().Select(s => s.color).ToList();
        List<ColorSource> sources = sourcesOctree.GetAll();
        foreach(ColorSource source in sources) {
            source.GoToColor(targetColor, fadeIn);
        }
        yield return new WaitForSeconds(fadeIn);
        yield return null;
        for(int i = 0; i < sources.Count; i++) {
            ColorSource source = sources[i];
            if (source != null) {
                source.GoToColor(initialColors[i], fadeOut);
            }
        }
    }

    public void MakeLightIntensityBounce(float intensityVariation, float fadeIn, float fadeOut) {
        StartCoroutine(CMakeLightIntensityBounce(intensityVariation, fadeIn, fadeOut));
    }

    public IEnumerator CMakeLightIntensityBounce(float intensityVariation, float fadeIn, float fadeOut) {
        Color startColor = RenderSettings.ambientLight;
        intensityVariation = Mathf.Pow(2, intensityVariation);
        Timer fadeInTimer = new Timer(fadeIn);
        while(!fadeInTimer.IsOver()) {
            float coef = MathCurves.Linear(1, 1 + intensityVariation, fadeInTimer.GetAvancement());
            RenderSettings.ambientLight = startColor * coef;
            yield return null;
        }
        Timer fadeOutTimer = new Timer(fadeOut);
        while(!fadeOutTimer.IsOver()) {
            float coef = MathCurves.Linear(1, 1 + intensityVariation, 1 - fadeOutTimer.GetAvancement());
            RenderSettings.ambientLight = startColor * coef;
            yield return null;
        }
        RenderSettings.ambientLight = startColor;
    }

    public static Color InterpolateHSV(Color colorSource, Color colorCible, float avancement) {
        float HS, SS, VS;
        Color.RGBToHSV(colorSource, out HS, out SS, out VS);
        float HC, SC, VC;
        Color.RGBToHSV(colorCible, out HC, out SC, out VC);
        float HR;
        if (Mathf.Abs(HC - HS) <= Mathf.Abs(HS + (1 - HC))) {
            HR = MathCurves.Linear(HS, HC, avancement);
        } else {
            if(HS < HC) {
                HS += 1;
            } else {
                HC += 1;
            }
            HR = MathCurves.Linear(HS, HC, avancement);
            HR = (HR > 1) ? HR - 1 : HR;
        }
        float SR = MathCurves.Linear(SS, SC, avancement);
        float VR = MathCurves.Linear(VS, VC, avancement);
        return Color.HSVToRGB(HR, SR, VR);
    }

    public static List<Theme> ReplaceSpecificColorFromThemes(List<Theme> themes, Theme source, Theme cible) {
        if(source == cible) {
            Debug.LogError($"La source ({source}) ne doit pas être égale à la cible ({cible}) dans ReplaceSpecificColorFromThemes ;)");
            return themes;
        }
        while (themes.Contains(source)) {
            themes.Remove(source);
            themes.Add(cible);
        }
        return themes;
    }

    public static List<Theme> RemoveSpecificColorFromThemes(List<Theme> themes, Theme theme) {
        while (themes.Contains(theme)) {
            themes.Remove(theme);
            if (themes.Count <= 0) {
                themes.Add(GetRandomNonBrightTheme());
            }
        }
        return themes;
    }

    public static Color GetMainColor(MainColor color) {
        switch (color) {
            case MainColor.RED:
                return Color.red;
            case MainColor.GREEN:
                return Color.green;
            case MainColor.BLUE:
                return Color.blue;
            default:
                throw new System.Exception($"MainColor unkown {color} :o");
        }
    }

    public static Color GetMainRed() {
        return GetMainColor(MainColor.RED);
    }
    public static Color GetMainGreen() {
        return GetMainColor(MainColor.GREEN);
    }
    public static Color GetMainBlue() {
        return GetMainColor(MainColor.BLUE);
    }

    // This is suppose to handle hdr colors ! x)
    public static Color RotateHueTo(Color rgbColor, float hue) {
        float h, s, v;
        Color.RGBToHSV(rgbColor, out h, out s, out v);
        return Color.HSVToRGB(hue, s, v, hdr: true);
    }
}
