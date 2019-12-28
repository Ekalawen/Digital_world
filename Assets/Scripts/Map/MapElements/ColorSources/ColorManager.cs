using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

    public GameObject colorSourcePrefab;
    public bool bGenerateInCubes = false;

	// On peut choisir le thème du cube =)
	// 0.0 = automne, 0.1 = jaune, 0.3 = vert, O.5 = bleu, 0.65 = violet, 0.8 = rose, 0.9 = rouge
	public List<ColorSource.ThemeSource> themes; // Le thème, cad la couleur de la map !

    // Il faudra changer ça, les sources ne sont pas forcément des cubes !?
	public float frequenceSources; // La frequence qu'un emplacement soit une source
	public Vector2 porteeSourceRange; // La range de distance de coloration d'une source
    public float cubeLuminosityMax; // Le maximum de luminosité d'un cube (pour éviter d'éblouir le joueur)

    protected MapManager map;
    [HideInInspector]
    public GameObject colorSourceFolder;

    [HideInInspector] public List<ColorSource> sources;

    public void Initialize() {
        map = FindObjectOfType<MapManager>();
        colorSourceFolder = new GameObject("ColorSources");
        cubeLuminosityMax = PlayerPrefs.GetFloat(MenuOptions.LUMINOSITY_KEY);

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
            GenerateColorSourcesInCubes();
        CheckCubeSaturation();
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

    public void GenerateColorSourcesInCubes() {
        // On calcul le nombre de sources
        List<Cube> cubes = map.GetAllCubes();
        List<Vector3> possiblesPos = new List<Vector3>();
        foreach (Cube cube in cubes) possiblesPos.Add(cube.transform.position);
        int N = possiblesPos.Count;
        float P = frequenceSources;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSources = (int)GaussianGenerator.Next(mean, variance, 0, N);
        Debug.Log("nbSources = " + nbSources);

        // Puis on les réparties ! C'est pas grâve si plusieurs sources sont au même endroit !
        for(int i = 0; i < nbSources; i++) {
            int indice = Random.Range(0, N);
            Vector3 pos = possiblesPos[indice];
            possiblesPos.RemoveAt(indice);
            N--;
            GameObject go = GameObject.Instantiate(colorSourcePrefab, pos, Quaternion.identity, colorSourceFolder.transform);
            ColorSource source = go.GetComponent<ColorSource>();
            source.Initialize(GetColor(themes), Random.Range(porteeSourceRange[0], porteeSourceRange[1]));
            sources.Add(go.GetComponent<ColorSource>());
        }
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
        MathTools.Shuffle(cubes);

        foreach(Cube cube in cubes) {
            while(cube.GetLuminosity() > cubeLuminosityMax + 0.0001f) { // Car la luminosité n'est jamais à vraiment 0
                RemoveClosestSource(cube.transform.position);
            }
        }
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
}
