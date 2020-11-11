using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct SavedTheme {
    public string key;
    public List<ColorSource.ThemeSource> themes;
}
[System.Serializable]
public struct SavedColor {
    public string key;
    public Color color;
}

public class ColorManager : MonoBehaviour {

    public GameObject colorSourcePrefab;
    public bool bGenerateInCubes = false;

	// On peut choisir le thème du cube =)
	// 0.0 = automne, 0.1 = jaune, 0.3 = vert, O.5 = bleu, 0.65 = violet, 0.8 = rose, 0.9 = rouge
	public List<ColorSource.ThemeSource> themes; // Le thème, cad la couleur de la map !
	public List<SavedTheme> savedThemes; // Utile si on a besoin d'autres thèmes à d'autres moments !

    // Il faudra changer ça, les sources ne sont pas forcément des cubes !?
	public float frequenceSources; // La frequence qu'un emplacement soit une source
	public Vector2 porteeSourceRange; // La range de distance de coloration d'une source
    public bool bUseOwnLuminosity = false; // De base on utilise la luminosité des PlayerPrefs !
    public float cubeLuminosityMax; // Le maximum de luminosité d'un cube (pour éviter d'éblouir le joueur)
    public List<SavedColor> savedColors; // Utile si on a besoin de certaines couleurs à certains moments dans le jeu !

    protected MapManager map;
    [HideInInspector]
    public GameObject colorSourceFolder;

    [HideInInspector] public List<ColorSource> sources;

    public virtual void Initialize() {
        map = FindObjectOfType<MapManager>();
        colorSourceFolder = new GameObject("ColorSources");
        cubeLuminosityMax = bUseOwnLuminosity ? cubeLuminosityMax : PlayerPrefs.GetFloat(MenuOptions.LUMINOSITY_KEY);

        // Si c'est random, alors on relance ! :)
        for (int i = 0; i < themes.Count; i++) {
            if (themes[i] == ColorSource.ThemeSource.RANDOM) {
                System.Array enumValues = System.Enum.GetValues(typeof(ColorSource.ThemeSource));
                themes[i] = (ColorSource.ThemeSource)enumValues.GetValue(Random.Range(0, (int)ColorSource.ThemeSource.RANDOM));
                while (themes[i] == ColorSource.ThemeSource.NOIR) { // On veut pas du noir ! x)
                    enumValues = System.Enum.GetValues(typeof(ColorSource.ThemeSource));
                    themes[i] = (ColorSource.ThemeSource)enumValues.GetValue(Random.Range(0, (int)ColorSource.ThemeSource.RANDOM));
                }
            }
            Debug.Log(themes[i]);
        }

        if (!bGenerateInCubes)
            GenerateColorSources(map.tailleMap);
        else
            GenerateColorSourcesInCubes(map.GetAllCubes());
        CheckCubeSaturation();

        Debug.Log("Nombre color sources = " + GetAllColorSources().Count);
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
            GameObject go = GameObject.Instantiate(colorSourcePrefab, pos, Quaternion.identity, colorSourceFolder.transform);
            ColorSource source = go.GetComponent<ColorSource>();
            source.Initialize(GetColor(themes), Random.Range(porteeSourceRange[0], porteeSourceRange[1]));
            sources.Add(go.GetComponent<ColorSource>());
        }
    }

    public void GenerateColorSourcesInCubes(List<Cube> cubes) {
        // On calcul le nombre de sources
        List<Vector3> possiblesPos = new List<Vector3>();
        foreach (Cube cube in cubes) possiblesPos.Add(cube.transform.position);
        int N = possiblesPos.Count;
        float P = frequenceSources;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSources = (int)GaussianGenerator.Next(mean, variance, 0, N);

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

	public static Color GetColor(List<ColorSource.ThemeSource> currentThemes) {
		Color c;

        // On choisit notre thème parmis nos thème
        ColorSource.ThemeSource themeChoisi = currentThemes[Random.Range(0, currentThemes.Count)];

		// Puis on l'applique !
		switch (themeChoisi) {
			case ColorSource.ThemeSource.JAUNE:
				c = Color.HSVToRGB(Random.Range(0.1f, 0.2f), 0.8f, 0.5f); break;
			case ColorSource.ThemeSource.VERT:
				c = Color.HSVToRGB(Random.Range(0.27f, 0.37f), 1f, 0.5f); break;
			case ColorSource.ThemeSource.BLEU_GLACE:
				c = Color.HSVToRGB(Random.Range(0.5f, 0.6f), 1f, 0.7f); break;
			case ColorSource.ThemeSource.BLEU_NUIT:
				c = Color.HSVToRGB(Random.Range(0.6f, 0.7f), 1f, 0.4f); break;
			case ColorSource.ThemeSource.VIOLET:
				c = Color.HSVToRGB(Random.Range(0.7f, 0.8f), 1f, 0.7f); break;
			case ColorSource.ThemeSource.ROUGE:
				c = Color.HSVToRGB(Random.Range(0.98f, 1.02f), 1f, 0.7f); break;
			case ColorSource.ThemeSource.MULTICOLOR:
				c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 0.7f); break;
			case ColorSource.ThemeSource.BLANC:
				c = Color.HSVToRGB(0f, 0f, 0.6f); break;
			case ColorSource.ThemeSource.NOIR:
				c = Color.HSVToRGB(0f, 0f, 0.001f); break;
			case ColorSource.ThemeSource.RANDOM:
                c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f); break;
			default:
				c = Color.black;
				Debug.Log("Je ne connais pas ce thème !");
                break;
		}
		return c;
	}

    public void CheckCubeSaturation() {
        List<Cube> cubes = map.GetAllCubes();
        CheckCubeSaturationInCubes(cubes);
    }

    public void CheckCubeSaturationInCubes(List<Cube> cubes) {
        MathTools.Shuffle(cubes);

        foreach(Cube cube in cubes) {
            while(cube.GetLuminosity() > cubeLuminosityMax + 0.0001f) { // Car la luminosité n'est jamais à vraiment 0
                RemoveClosestSource(cube.transform.position);
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

    public static ColorSource.ThemeSource GetRandomTheme() {
        System.Array enumValues = System.Enum.GetValues(typeof(ColorSource.ThemeSource));
        return (ColorSource.ThemeSource)enumValues.GetValue(Random.Range(0, enumValues.Length));
    }

    public static Color InterpolateColors(Color c1, Color c2, float avancement = 0.5f) {
        return (1 - avancement) * c1 + avancement * c2;
    }

    public List<ColorSource> GetAllColorSources() {
        return sources;
    }

    public void ChangeTheme(List<ColorSource.ThemeSource> newThemes) {
        themes = newThemes;
        foreach(ColorSource source in sources) {
            source.ChangeColor(GetColor(themes));
        }
    }

    public List<ColorSource.ThemeSource> GetSavedTheme(string key) {
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

    public static bool AreSameThemes(List<ColorSource.ThemeSource> t1, List<ColorSource.ThemeSource> t2) {
        return MathTools.AreListEqual(t1, t2);
    }

    public Color GetColorForPosition(Vector3 pos) {
        Color color = Color.black;
        foreach(ColorSource source in sources) {
            float dist = Vector3.Distance(pos, source.transform.position);
            if(dist < source.range) {
                color += source.GetColorForPosition(pos);
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
}
