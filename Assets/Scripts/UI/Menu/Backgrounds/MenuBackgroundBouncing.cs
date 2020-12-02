using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBackgroundBouncing : MonoBehaviour {

	public GameObject panelPrefab;
	public RectTransform rectToFill;
	public int panelSize;

	protected int nbPanelsX;
	protected int nbPanelsY;
	protected List<GameObject> panels;
    protected PanelBouncing[,] panelsPos;
    protected bool isPaused = false;


	public void Initialize () {
        nbPanelsX = (int)(rectToFill.rect.width / panelSize) + 1; // +1 pour être sur que ça dépasse de tous les cotés !
        nbPanelsY = (int)(rectToFill.rect.height / panelSize) + 1;
        panels = new List<GameObject>();
        panelsPos = new PanelBouncing[nbPanelsX, nbPanelsY];

        CreateAllPanels();
    }

    public void Update() {
        if(Input.GetMouseButton(0)) {
            AddSourceWhereIsMouse();
        }
    }

    protected void AddSourceWhereIsMouse() {
        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        int correspondingX = (int)(mousePosition.x * (nbPanelsX - 1));
        int correspondingY = (int)(mousePosition.y * (nbPanelsY - 1));
        if(0 <= correspondingX && correspondingX < nbPanelsX && 0 <= correspondingY && correspondingY < nbPanelsY)
            panelsPos[correspondingX, correspondingY].BecameOrUpdateSource();
    }

    protected void CreateAllPanels() {
        for (int i = 0; i < GetNbPanels(); i++) {
            GameObject go = Instantiate(panelPrefab) as GameObject;
            PanelBouncing panel = go.GetComponent<PanelBouncing>();

            int x = i / nbPanelsY;
            int y = i % nbPanelsY;
            panel.Initialize(x, y, this);
            panelsPos[x, y] = panel;

            panel.GetComponent<Image>().color = Color.black;

            panel.transform.SetParent(this.transform);
            panel.transform.SetAsFirstSibling();

            panel.SetPosition(new Vector2(x, y), panelSize, rectToFill);

            panels.Add(panel.gameObject);
        }
    }

    public bool IsIn(int x, int y) {
		return x >= 0 && y >= 0 && x < nbPanelsX && y < nbPanelsY;
	}

	public PanelBouncing GetPanelByPosition(int x, int y ) {
		return panelsPos[x, y];
	}

    public void SetParameters(float probaSource,
        int distanceSource, 
        float decroissanceSource,
        List<ColorSource.ThemeSource> themes) {
        for(int i = 0; i < nbPanelsX; i++) {
            for(int j = 0; j < nbPanelsY; j++) {
                panelsPos[i, j].SetSource(UnityEngine.Random.Range(0.0f, 1.0f) < probaSource);
                panelsPos[i, j].probaSource = probaSource;
                panelsPos[i, j].distanceSource = distanceSource;
                panelsPos[i, j].decroissanceSource = decroissanceSource;
                panelsPos[i, j].themes = themes;
            }
        }
    }

    public void StartLoading() {
        List<PanelBouncing> panels = new List<PanelBouncing>();
        foreach (PanelBouncing panel in panelsPos)
            panels.Add(panel);

        LoadingWheel wheel = new GameObject("LoadingWheel").AddComponent<LoadingWheel>();
        wheel.Initialize(panels, this);
    }

    public int GetNbPanels() {
        return nbPanelsX * nbPanelsY;
    }

    public void Pause() {
        foreach(PanelBouncing panel in panelsPos) {
            isPaused = true;
            panel.Pause();
        }
    }

    public void Unpause() {
        foreach(PanelBouncing panel in panelsPos) {
            isPaused = false;
            panel.Unpause();
        }
    }

    public bool IsPaused() {
        return isPaused;
    }
}
