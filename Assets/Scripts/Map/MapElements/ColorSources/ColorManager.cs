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
    public float cubeLuminosityMax; // Le maximum de luminosité d'un cube (pour éviter d'éblouir le joueur)
    public List<SavedColor> savedColors; // Utile si on a besoin de certaines couleurs à certains moments dans le jeu !

    protected MapManager map;
    [HideInInspector]
    public GameObject colorSourceFolder;

    [HideInInspector] public List<ColorSource> sources;

    public virtual void Initialize() {
        map = FindObjectOfType<MapManager>();
        colorSourceFolder = new GameObject("ColorSources");

        ReplaceNonDeterministicColors();

        if (!bGenerateInCubes)
            GenerateColorSources(map.tailleMap);
        else
            GenerateColorSourcesInCubes(map.GetAllCubes());
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

    protected void ReplaceNonDeterministicColors() {
        for (int i = 0; i < themes.Count; i++) {
            if (themes[i] == Theme.RANDOM) {
                themes[i] = GetRandomTheme();
            } else if (themes[i] == Theme.RANDOM_NON_BRIGHT) {
                themes[i] = GetRandomNonBrightTheme();
            }
        }
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
        sources.Add(go.GetComponent<ColorSource>());
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
				Debug.LogWarning("Je ne connais pas ce thème !");
                break;
		}
		return c;
	}

    public void CheckCubeSaturation() {
        List<Cube> cubes = map.GetAllCubes();
        CheckCubeSaturationInCubes(cubes);
        EnsureNoCubeIsBlack(cubes);
    }

    public void CheckCubeSaturationInCubes(List<Cube> cubes) {
        MathTools.Shuffle(cubes);

        foreach(Cube cube in cubes) {
            while(cube.GetLuminosity() > cubeLuminosityMax + 0.0001f) { // Car la luminosité n'est jamais à vraiment 0
                RemoveClosestSource(cube.transform.position);
            }
        }
    }

    public void EnsureNoCubeIsBlack(List<Cube> cubes) {
        MathTools.Shuffle(cubes);

        foreach(Cube cube in cubes) {
            if(cube.GetLuminosity() < 0.001f) {
                GenerateColorSourceAt(cube.transform.position);
            }
        }
    }

    public List<ColorSource> GetClosestsColorSources(Vector3 pos, int nb)
    {
        sources.Sort(delegate (ColorSource A, ColorSource B) {
            float distAToStart = Vector3.Distance(A.transform.position, pos);
            float distBToStart = Vector3.Distance(B.transform.position, pos);
            return distBToStart.CompareTo(distAToStart);
        });
        return sources.Take(nb).ToList();
    }

    public List<ColorSource> GetColorSourcesInRange(Vector3 pos, float range) {
        return sources.Where(source => Vector3.Distance(source.transform.position, pos) <= range).ToList();
    }

    protected void RemoveClosestSource(Vector3 pos) {
        float distMin = Vector3.Distance(pos, sources[0].transform.position);
        ColorSource closest = sources[0];
        foreach(ColorSource source in sources) {
            float dist = Vector3.Distance(pos, source.transform.position);
            if(dist < distMin) {
                distMin = dist;
                closest = source;
            }
        }
        sources.Remove(closest);
        closest.Delete();
    }

    //public static ColorManager.Theme GetRandomTheme() {
    //    System.Array enumValues = System.Enum.GetValues(typeof(ColorManager.Theme));
    //    return (ColorManager.Theme)enumValues.GetValue(Random.Range(0, enumValues.Length));
    //}

    //public static ColorManager.Theme GetRandomThemeNotNoir() {
    //    ColorManager.Theme theme = GetRandomTheme();
    //    while(theme == ColorManager.Theme.NOIR)
    //        theme = GetRandomTheme();
    //    return theme;
    //}

    public static Color InterpolateColors(Color c1, Color c2, float avancement = 0.5f) {
        return (1 - avancement) * c1 + avancement * c2;
    }

    public List<ColorSource> GetAllColorSources() {
        return sources;
    }

    public void ChangeTheme(List<ColorManager.Theme> newThemes) {
        themes = newThemes;
        foreach(ColorSource source in sources) {
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
        foreach(ColorSource source in sources) {
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

    public static float GetLuminosity(Color color) {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        return V;
    }

    public void MakeAllColorSourcesBounceToColor(Color targetColor, float fadeIn, float fadeOut) {
        StartCoroutine(CMakeAllColorSourcesBounceToColor(targetColor, fadeIn, fadeOut));
    }

    public IEnumerator CMakeAllColorSourcesBounceToColor(Color targetColor, float fadeIn, float fadeOut) {
        List<Color> initialColors = this.sources.Select(s => s.color).ToList();
        List<ColorSource> sources = this.sources;
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
}
