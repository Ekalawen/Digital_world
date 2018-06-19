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
public class MenuBackgroundScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject cube; // Les petits cubes à faire apparaître à l'écran <3 (ce sont en fait des panels :/)
	public RectTransform rect; // la taille de la zone à remplir
	public int size; // La taille des petits cubes
	public float proportionCubes; // La quantité de petits cubes !

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	private int nbX; // Le nombre de petits cubes en large
	private int nbY; // Le nombre de petits cubes en hauteur
	private int nbCubes; // Le nombre total de cubes
	private List<GameObject> cubes; // Tous les petits cubes ! <3
	private int[,] positions; // Les positions de tous les cubes. 1 il y a quelqu'un, 0 il n'y a personne

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// Initialisations
		nbX = (int) (rect.rect.width / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbY = (int) (rect.rect.height / size) + 2; // +2 pour être vraiment sur que ça dépasse de tous les cotés !
		nbCubes = (int) (nbX * nbY * proportionCubes);
		positions = new int[nbX, nbY];
		cubes = new List<GameObject>();

		// Puis on ajoute tous les cubes ! <3
		for(int i = 0; i < nbCubes; i++) {
			// On instancie notre cube
			GameObject monCube = Instantiate(cube) as GameObject;

			// On cherche une position innocupée
			int x;
			int y;
			while(positions[x = Random.Range(0, nbX-1), y = Random.Range(0, nbY-1)] == 1); 

			// On lui donne cette position

			// On lui donne également une couleur aléatoire !
			monCube.GetComponent<Image>().color = Color.HSVToRGB(Random.Range(0.27f, 0.37f), 1f, Random.Range(0f, 1f));

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
			positions[x, y] = 1;
		}
	}
	
	void Update () {
		
	}
}
