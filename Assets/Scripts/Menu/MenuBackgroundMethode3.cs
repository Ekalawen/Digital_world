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
public class MenuBackgroundMethode3 : MonoBehaviour {

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
	public PanelMethode3[,] positions; // les positions


	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisations
		nbX = (int) (rect.rect.width / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbY = (int) (rect.rect.height / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbCubes = (int) (nbX * nbY); // On prend toutes les places possibles
		cubes = new List<GameObject>();
		positions = new PanelMethode3[nbX, nbY];

		// Puis on ajoute tous les cubes ! <3
		for(int i = 0; i < nbCubes; i++) {
			// On instancie notre cube
			GameObject monCube = Instantiate(cubePrefabs) as GameObject;
            monCube.GetComponent<PanelMethode3>().menu = this;

			// On lui donne cette position
			int x = i / nbY;
			int y = i % nbY;
            monCube.GetComponent<PanelMethode3>().x = x;
            monCube.GetComponent<PanelMethode3>().y = y;
			positions[x, y] = monCube.GetComponent<PanelMethode3>();

			// On lui donne la couleur noire !
			monCube.GetComponent<Image>().color = Color.black;

			// On set son parent
			monCube.transform.SetParent(this.transform);
			monCube.transform.SetAsFirstSibling();

			// On set sa taille
			RectTransform r = monCube.GetComponent<RectTransform>();
			r.localPosition = new Vector3(x * size - rect.rect.width / 2 + size / 2, y * size - rect.rect.height / 2 + size / 2, 0);
			r.localRotation = Quaternion.identity;
			r.localScale = new Vector3(1, 1, 1);
			r.sizeDelta = new Vector2(size, size);

			// Et on l'ajoute à notre liste
			cubes.Add(monCube);
		}
	}
	
	public bool isIn(int x, int y) {
		return x >= 0 && y >= 0 && x < nbX && y < nbY;
	}

	public PanelMethode3 getPanelXY(int x, int y ) {
		return positions[x, y];
	}
}
