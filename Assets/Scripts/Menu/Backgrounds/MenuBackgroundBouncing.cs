using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Le but de ce script est d'animer le fond d'écran du menu ! :D
// Il va créer pleins de petits panels indépendants et de couleurs différentes.
// Au fur et à mesure du temps ceux-ci voudront bouger. Lorsqu'ils voudront bouger ils choisiront une direction.
// Si la place est libre ils iront.
// Si il y a quelqu'un il recevra alors l'ordre de bouger à son tour pour libérer la place ! :D
// Et ça serait trop bien que chaque cube aille vers le barycentre de sa propre couleur ! :D
public class MenuBackgroundBouncing : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject cubePrefabs; // Les petits cubes à faire apparaître à l'écran <3 (ce sont en fait des panels :/)
	public RectTransform rect; // la taille de la zone à remplir
	public int size; // La taille des petits cubes

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
	public int nbX; // Le nombre de petits cubes en large
    [HideInInspector]
	public int nbY; // Le nombre de petits cubes en hauteur
    [HideInInspector]
	public int nbCubes; // Le nombre total de cubes
    [HideInInspector]
	public List<GameObject> cubes; // Tous les petits cubes ! <3
    [HideInInspector]
	public PanelBouncing[,] positions; // les positions


	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisations
		nbX = (int) (rect.rect.width / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbY = (int) (rect.rect.height / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbCubes = (int) (nbX * nbY); // On prend toutes les places possibles
		cubes = new List<GameObject>();
		positions = new PanelBouncing[nbX, nbY];

		// Puis on ajoute tous les cubes ! <3
		for(int i = 0; i < nbCubes; i++) {
			// On instancie notre cube
			GameObject go = Instantiate(cubePrefabs) as GameObject;
            PanelBouncing panel = go.GetComponent<PanelBouncing>();
            panel.menu = this;

			// On lui donne cette position
			int x = i / nbY;
			int y = i % nbY;
            panel.x = x;
            panel.y = y;
			positions[x, y] = panel;

			// On lui donne la couleur noire !
			panel.GetComponent<Image>().color = Color.black;

			// On set son parent
			panel.transform.SetParent(this.transform);
			panel.transform.SetAsFirstSibling();

            // On set sa taille
            panel.SetPosition(new Vector2(x, y), size, rect);

			// Et on l'ajoute à notre liste
			cubes.Add(panel.gameObject);
		}
	}

    public bool isIn(int x, int y) {
		return x >= 0 && y >= 0 && x < nbX && y < nbY;
	}

	public PanelBouncing getPanelXY(int x, int y ) {
		return positions[x, y];
	}

    public void SetParameters(float probaSource,
        int distanceSource, 
        float decroissanceSource,
        List<ColorSource.ThemeSource> themes) {
        for(int i = 0; i < nbX; i++) {
            for(int j = 0; j < nbY; j++) {
                positions[i, j].isSource = (UnityEngine.Random.Range(0.0f, 1.0f) < probaSource);
                positions[i, j].probaSource = probaSource;
                positions[i, j].distanceSource = distanceSource;
                positions[i, j].decroissanceSource = decroissanceSource;
                positions[i, j].themes = themes;
            }
        }
    }

    public void StartLoading() {
        List<PanelBouncing> panels = new List<PanelBouncing>();
        foreach (PanelBouncing panel in positions)
            panels.Add(panel);

        LoadingWheel wheel = new GameObject("LoadingWheel").AddComponent<LoadingWheel>();
        wheel.Initialize(panels, this);
    }
}
