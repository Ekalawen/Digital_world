using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ENUM
	//////////////////////////////////////////////////////////////////////////////////////

	public enum CubeType {Source, Recepteur};
	public enum ThemeCube {JAUNE, VERT, BLEU_GLACE, BLEU_NUIT, VIOLET, ROUGE, MULTICOLOR, BLANC, NOIR, RANDOM};

	//////////////////////////////////////////////////////////////////////////////////////
	// STATIC
	//////////////////////////////////////////////////////////////////////////////////////

	public static float probaSource;
	public static float distSourceMax;
	// ON peut choisir le thème du cube =)
	// 0.0 = automne, 0.1 = jaune, 0.3 = vert, O.5 = bleu, 0.65 = violet, 0.8 = rose, 0.9 = rouge
	public static List<ThemeCube> theme;

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public CubeType type;
	public Color couleur;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Awake () {
		// De base, tous les cubes sont des récepteurs avec une probabilité 1-p !
		if (Random.Range (0f, 1f) <= probaSource) {
			type = CubeType.Source;
			// Si le cube est une source, on va changer sa couleur !
			Color c = getColor();
			couleur = c;
			GetComponent<MeshRenderer> ().material.color = c;
		} else {
			type = CubeType.Recepteur;
			couleur = Color.black;
			GetComponent<MeshRenderer> ().material.color = Color.black;
		}
	}

	private Color getColor() {
		Color c;

		// On choisit notre thème parmis nos thème
		CubeScript.ThemeCube themeChoisi = theme[Random.Range(0, theme.Count)];

		// Puis on l'applique !
		switch (themeChoisi)
		{
			case ThemeCube.JAUNE:
				c = Color.HSVToRGB(Random.Range(0.1f, 0.2f), 0.8f, 0.5f);
			break;
			case ThemeCube.VERT:
				c = Color.HSVToRGB(Random.Range(0.27f, 0.37f), 1f, 0.5f);
			break;
			case ThemeCube.BLEU_GLACE:
				c = Color.HSVToRGB(Random.Range(0.5f, 0.6f), 1f, 0.7f);
			break;
			case ThemeCube.BLEU_NUIT:
				c = Color.HSVToRGB(Random.Range(0.6f, 0.7f), 1f, 0.4f);
			break;
			case ThemeCube.VIOLET:
				c = Color.HSVToRGB(Random.Range(0.7f, 0.8f), 1f, 0.7f);
			break;
			case ThemeCube.ROUGE:
				c = Color.HSVToRGB(Random.Range(0.98f, 1.02f), 1f, 0.7f);
			break;
			case ThemeCube.MULTICOLOR:
				c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 0.7f);
			break;
			case ThemeCube.BLANC:
				c = Color.HSVToRGB(0f, 0f, 0.6f);
			break;
			case ThemeCube.NOIR:
				c = Color.HSVToRGB(0f, 0f, 0.001f);
			break;
			default:
				c = Color.black;
				Debug.Log("Je ne connais pas ce thème !");
			break;
		}
		return c;
	}
	
	void Start () {
		// La couleur des récepteurs se met à jour en fonction des sources qui sont suffisamment proches d'eux !
		// On récupère toutes les sources à moins de distSourceMax
		Collider[] colliders = Physics.OverlapSphere(this.transform.position, distSourceMax);
		foreach (Collider collider in colliders) {
			if (collider.tag == "Cube") {
				CubeScript c = collider.gameObject.GetComponent<CubeScript> () as CubeScript;				
				if (c.type == CubeType.Source) {
					float distance = Vector3.Distance (c.transform.position, this.transform.position);
					if (distance < distSourceMax) {
						float affaiblissement = 1f - distance / distSourceMax;
						GetComponent<MeshRenderer> ().material.color += (c.couleur * affaiblissement);
					}
				}
			}
		}
		// Puis on soustrait sa propre couleur pour ne pas la compter deux fois
		GetComponent<MeshRenderer>().material.color -= couleur;
	}
}
