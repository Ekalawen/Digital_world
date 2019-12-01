using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMethode2 : MonoBehaviour {

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PUBLIQUES
    //////////////////////////////////////////////////////////////////////////////////////

    public float frequence; // La fréquence à laquelle le panel de changer de couleur !
    public float seuil; // la différence de couleur que l'on peut prendre !

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PRIVEES
    //////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
    public MenuBackgroundMethode2 menu; // un pointeur vers le menu
    [HideInInspector]
    public int x; // Sa position dans l'écran, en terme de position par cube
    [HideInInspector]
    public int y;

    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Initialisation
    }

    void Update () {
        // À chaque frame on a une petite chance de changer de couleur
        if(Random.Range(0f, 1f) < frequence)
        {
            prendreCouleurVoisins();
        }
	}
            
    void prendreCouleurVoisins() {
        List<Color> couleursVoisines = getCouleursVoisines();
        int ind = Random.Range(0, couleursVoisines.Count);
        // MÉTHODE 1 : on prend la même couleur
        //gameObject.GetComponent<Image>().color = couleursVoisines[ind];

        // MÉTHODE 2 : on prend une couleur similaire
        Color couleurCible = couleursVoisines[ind];
        float h, s, v;
        Color.RGBToHSV(couleurCible, out h, out s, out v);
        couleurCible = Color.HSVToRGB(Random.Range(h - seuil, h + seuil),
                                          s,
                                          v);
        gameObject.GetComponent<Image>().color = couleurCible;
    }

    List<Color> getCouleursVoisines() {
        List<Color> couleurs = new List<Color>();
        if(menu.isIn(x+1, y)) {
            couleurs.Add(menu.getPanelXY(x+1, y).gameObject.GetComponent<Image>().color);
        }
        if(menu.isIn(x-1, y)) {
            couleurs.Add(menu.getPanelXY(x-1, y).gameObject.GetComponent<Image>().color);
        }
        if(menu.isIn(x, y+1)) {
            couleurs.Add(menu.getPanelXY(x, y+1).gameObject.GetComponent<Image>().color);
        }
        if(menu.isIn(x, y-1)) {
            couleurs.Add(menu.getPanelXY(x, y-1).gameObject.GetComponent<Image>().color);
        }
        return couleurs;
    }

    // La distance colorimétrique entre deux couleurs, en norme 2
    public static float distanceCouleur(Color c1, Color c2)
    {
        float res = 0;
        res += Mathf.Pow(c1.r - c2.r, 2);
        res += Mathf.Pow(c1.g - c2.g, 2);
        res += Mathf.Pow(c1.b - c2.b, 2);
        res = Mathf.Pow(res, 1 / 2);
        return res;
    }
}
