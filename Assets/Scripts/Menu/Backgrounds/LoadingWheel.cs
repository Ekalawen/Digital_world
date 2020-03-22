using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWheel : MonoBehaviour {

    public float coefficiantDeRapprochement = 0.3f;
    public float rayon = 3.0f;

    protected List<PanelBouncing> panels;
    protected List<PanelPositionLeader> leaders;

    public void Initialize(List<PanelBouncing> panels, MenuBackgroundBouncing background) {
        this.panels = panels;
        leaders = new List<PanelPositionLeader>();
        Vector2 center = new Vector2(background.rect.rect.width / 2, background.rect.rect.height / 2);
        int i = 0;
        foreach(PanelBouncing panel in panels) {
            CirclePanelPositionLeader leader = new CirclePanelPositionLeader(
                panel: panel,
                position: new Vector2(),
                coefficiantDeRapprochement: coefficiantDeRapprochement, 
                background: background, 
                center: center, 
                rayon: rayon, 
                n: panels.Count, 
                ind: i);
            leaders.Add(leader);
            i++;
        }
    }

    public void Update() {
        foreach(PanelPositionLeader leader in leaders) {
            leader.Update();
        }
    }
}
