using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMethode3Script : MonoBehaviour {

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PUBLIQUES
    //////////////////////////////////////////////////////////////////////////////////////

    public float probaSource; // La probabilité d'être une source
    public int distanceSource; // La distance d'action de la source
    public float decroissanceSource; // La vitesse de décroissance de la source

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PRIVEES
    //////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
    public MenuBackgroundMethode3Script menu; // un pointeur vers le menu
    [HideInInspector]
    public int x; // Sa position dans l'écran, en terme de position par cube
    [HideInInspector]
    public int y;
    [HideInInspector]
    public bool isSource; // Permet de savoir que notre cube est une source
    [HideInInspector]
    public Color couleurSource; // La couleur de notre cube

    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Initialisation
        isSource = false;
    }

    void Update () {
        // Si c'est une source on fait décroître sa couleur
        if(isSource) {
            decroitreCouleur();
        }

        // Si on est pas une source, on a une petite chance de le devenir ! =)
        if(!isSource && Random.Range(0f, 1f) < probaSource) {
            becameSource();
        }

        // On met à jour la couleur
        setColor();
	}

    void decroitreCouleur() {
        float h, s, v;
        Color.RGBToHSV(couleurSource, out h, out s, out v);
        Debug.Log("avant v = " + v);
        v = v - decroissanceSource;
        Debug.Log("après v = " + v);
        if(v <= 0f) {
            isSource = false;
        } else {
            couleurSource = Color.HSVToRGB(h, s, v);
        }
    }

    void becameSource() {
        isSource = true;
		couleurSource = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
    }
    
    void setColor() {
        // On initialise la couleur
        Color couleur = Color.black;

        // On rajoute toutes les influence de toutes les sources suffisamment proches !
        for(int i = x - distanceSource; i <= x + distanceSource; i++) {
            for(int j = y - distanceSource; j <= y + distanceSource; j++) {
                if(menu.isIn(i, j)) {
                    PanelMethode3Script p = menu.getPanelXY(i, j);
                    int distance = distanceCarre(p);
                    if(p.isSource && distance <= distanceSource) {
                        float coefDistance = (float)distance / (float)distanceSource;
                        couleur.r += p.couleurSource.r * (1 - coefDistance);
                        couleur.g += p.couleurSource.g * (1 - coefDistance);
                        couleur.b += p.couleurSource.b * (1 - coefDistance);
                    }
                }
            }
        }

        // On set la couleur !
        GetComponent<Image>().color = couleur;
    }

    int distanceCarre(PanelMethode3Script p) {
        return (int) (Mathf.Abs(p.x - x) + Mathf.Abs(p.y - y));
    }
}
