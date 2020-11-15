using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelPositionLeader {

    protected PanelBouncing panel;
    protected Vector2 position;
    protected float coefficiantDeRapprochement = 0.3f;
    protected MenuBackgroundBouncing background;

    public PanelPositionLeader(PanelBouncing panel,
        Vector2 position, 
        float coefficiantDeRapprochement,
        MenuBackgroundBouncing background) {
        this.panel = panel;
        this.position = position;
        this.coefficiantDeRapprochement = coefficiantDeRapprochement;
        this.background = background;
    }

    public virtual void Update() {
        Vector2 currentPos = panel.realPosition;
        Vector2 finalPos = (coefficiantDeRapprochement) * position + (1.0f - coefficiantDeRapprochement) * currentPos;
        Debug.LogFormat("currentPos = {0} finalPos = {1}", currentPos, finalPos);
        panel.SetPosition(position, background.panelSize, background.rectToFill);
    }
}
