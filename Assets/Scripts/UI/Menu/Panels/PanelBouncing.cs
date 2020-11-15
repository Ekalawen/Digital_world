using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelBouncing : MonoBehaviour {

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PUBLIQUES
    //////////////////////////////////////////////////////////////////////////////////////

    public float probaSource; // La probabilité d'être une source
    public int distanceSource; // La distance d'action de la source
    public float decroissanceSource; // La vitesse de décroissance de la source
    public List<ColorSource.ThemeSource> themes; // Les thèmes pour choisir la couleur des sources

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PRIVEES
    //////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
    public MenuBackgroundBouncing menu; // un pointeur vers le menu
    [HideInInspector]
    public int x; // Sa position dans l'écran, en terme de position par cube
    [HideInInspector]
    public int y;
    [HideInInspector]
    public bool isSource; // Permet de savoir que notre cube est une source
    [HideInInspector]
    public Color couleurSource; // La couleur de notre cube
    [HideInInspector]
    public Vector2 realPosition; // La véritable position en pixel à l'écran ! x)

    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    void Start() {
        // Initialisation
        isSource = false;

        if (themes == null)
            themes = new List<ColorSource.ThemeSource> { ColorSource.ThemeSource.RANDOM };
    }

    void Update () {
        // Si c'est une source on fait décroître sa couleur
        if(isSource) {
            DecroitreCouleur();
        }

        // Si on est pas une source, on a une petite chance de le devenir ! =)
        if(!isSource && Random.Range(0f, 1f) < probaSource) {
            BecameSource();
        }

        // On met à jour la couleur
        SetColor();
	}

    void DecroitreCouleur() {
        float h, s, v;
        Color.RGBToHSV(couleurSource, out h, out s, out v);
        v = v - decroissanceSource;
        if(v <= 0f) {
            isSource = false;
        } else {
            couleurSource = Color.HSVToRGB(h, s, v);
        }
    }

    void BecameSource() {
        isSource = true;
        couleurSource = ColorManager.GetColor(themes);
		//couleurSource = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
    }
    
    void SetColor() {
        // On initialise la couleur
        Color couleur = Color.black;

        // On rajoute toutes les influence de toutes les sources suffisamment proches !
        for(int i = x - distanceSource; i <= x + distanceSource; i++) {
            for(int j = y - distanceSource; j <= y + distanceSource; j++) {
                if(menu.IsIn(i, j)) {
                    PanelBouncing p = menu.GetPanelByPosition(i, j);
                    int distance = DistanceCarre(p);
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

    int DistanceCarre(PanelBouncing p) {
        return (int) (Mathf.Abs(p.x - x) + Mathf.Abs(p.y - y));
    }

    public void SetPosition(Vector2 pos, float size, RectTransform parentRect) {
        RectTransform r = this.GetComponent<RectTransform>();
        float newX = pos.x * size - parentRect.rect.width / 2 + size / 2;
        float newY = pos.y * size - parentRect.rect.height / 2 + size / 2;
        r.localPosition = new Vector3(newX, newY, 0);
        r.localRotation = Quaternion.identity;
        r.localScale = new Vector3(1, 1, 1);
        r.sizeDelta = new Vector2(size, size);
        realPosition = new Vector2(newX, newY);
    }
}
