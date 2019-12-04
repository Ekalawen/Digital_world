using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

    public GameObject colorSourcePrefab;

	// On peut choisir le thème du cube =)
	// 0.0 = automne, 0.1 = jaune, 0.3 = vert, O.5 = bleu, 0.65 = violet, 0.8 = rose, 0.9 = rouge
	public List<ColorSource.ThemeSource> themes; // Le thème, cad la couleur de la map !

    // Il faudra changer ça, les sources ne sont pas forcément des cubes !?
	public float frequenceSources; // La frequence qu'un emplacement soit une source
	public Vector2 porteeSourceRange; // La range de distance de coloration d'une source

    protected MapManager map;

    [HideInInspector] public List<ColorSource> sources;

    public void Initialize() {
        map = FindObjectOfType<MapManager>();
        GenerateColorSources();
    }

    protected void GenerateColorSources() {
        // On calcul le nombre de sources
        int N = map.tailleMap * map.tailleMap * map.tailleMap;
        float P = frequenceSources;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSources = (int)GaussianGenerator.Next(mean, variance, 0, N);

        // Puis on les réparties ! C'est pas grâve si plusieurs sources sont au même endroit !
        for(int i = 0; i < nbSources; i++) {
            Vector3Int pos = new Vector3Int(Random.Range(0, map.tailleMap), Random.Range(0, map.tailleMap), Random.Range(0, map.tailleMap));
            GameObject go = GameObject.Instantiate(colorSourcePrefab, pos, Quaternion.identity);
            ColorSource source = go.GetComponent<ColorSource>();
            source.Initialize(GetColor(), Random.Range(porteeSourceRange[0], porteeSourceRange[1]));
            sources.Add(go.GetComponent<ColorSource>());
        }
    }

	private Color GetColor() {
		Color c;

        // On choisit notre thème parmis nos thème
        ColorSource.ThemeSource themeChoisi = themes[Random.Range(0, themes.Count)];

        // Si c'est random, alors on relance ! :)
        if(themeChoisi == ColorSource.ThemeSource.RANDOM) {
            System.Array enumValues = System.Enum.GetValues(typeof(ColorSource.ThemeSource));
            themeChoisi = (ColorSource.ThemeSource)enumValues.GetValue(Random.Range(0, (int)ColorSource.ThemeSource.RANDOM));
        }

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
			default:
				c = Color.black;
				Debug.Log("Je ne connais pas ce thème !");
                break;
		}
		return c;
	}
}
