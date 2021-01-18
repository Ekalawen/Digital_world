using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelBouncing : MonoBehaviour {

    public float probaSource; // La probabilité d'être une source
    public int distanceSource; // La distance d'action de la source
    public float decroissanceSource; // La vitesse de décroissance de la source
    public List<ColorManager.Theme> themes; // Les thèmes pour choisir la couleur des sources

    protected MenuBackgroundBouncing menu;
    protected int x; // La position en indice
    protected int y;
    protected Vector2 realPosition; // La véritable position en pixel à l'écran
    protected bool isSource;
    protected Color colorIfSource;
    protected bool isPaused = false;

    public void Initialize(int x, int y, MenuBackgroundBouncing menuBackgroundBouncing) {
        this.x = x;
        this.y = y;
        this.menu = menuBackgroundBouncing;

        isSource = false;

        if (themes == null)
            themes = new List<ColorManager.Theme> { ColorManager.Theme.RANDOM };
    }

    void Update () {
        if (isPaused)
            return;

        if(isSource) {
            DecroitreCouleur();
        }

        if(!isSource && UnityEngine.Random.Range(0f, 1f) < probaSource) {
            BecameSource();
        }

        SetColor();
	}

    void DecroitreCouleur() {
        float h, s, v;
        Color.RGBToHSV(colorIfSource, out h, out s, out v);
        v = v - decroissanceSource;
        if(v <= 0f) {
            isSource = false;
        } else {
            colorIfSource = Color.HSVToRGB(h, s, v);
        }
    }

    public void BecameSource() {
        isSource = true;
        colorIfSource = ColorManager.GetColor(themes);
    }

    public void BecameOrUpdateSource() {
        if (!isSource) {
            BecameSource();
        } else {
            UpdateSource();
        }
    }

    protected void UpdateSource() {
        float h, s, v;
        Color.RGBToHSV(colorIfSource, out h, out s, out v);
        v = 1.0f;
        colorIfSource = Color.HSVToRGB(h, s, v);
    }

    public void SetSource(bool value) {
        this.isSource = value;
    }

    public bool IsSource() {
        return isSource;
    }

    void SetColor() {
        Color couleur = Color.black;

        couleur = GetInfluenceOfOthersSources(couleur);

        GetComponent<Image>().color = couleur;
    }

    protected Color GetInfluenceOfOthersSources(Color couleur) {
        for (int i = x - distanceSource; i <= x + distanceSource; i++) {
            for (int j = y - distanceSource; j <= y + distanceSource; j++) {
                if (menu.IsIn(i, j)) {
                    PanelBouncing p = menu.GetPanelByPosition(i, j);
                    int distance = DistanceCarre(p);
                    if (p.isSource && distance <= distanceSource) {
                        float coefDistance = (float)distance / (float)distanceSource;
                        couleur.r += p.colorIfSource.r * (1 - coefDistance);
                        couleur.g += p.colorIfSource.g * (1 - coefDistance);
                        couleur.b += p.colorIfSource.b * (1 - coefDistance);
                    }
                }
            }
        }

        return couleur;
    }

    protected int DistanceCarre(PanelBouncing p) {
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

    public Vector2 GetRealPosition() {
        return realPosition;
    }

    public void Pause() {
        isPaused = true;
    }

    public void Unpause() {
        isPaused = false;
    }
}
