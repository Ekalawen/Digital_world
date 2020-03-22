using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePanelPositionLeader : PanelPositionLeader {

    protected Vector2 center;
    protected float rayon;
    protected int n; // Le nombre de circlePanel
    protected int ind; // L'indice de celui-ci !
    protected float startPhi;

    public CirclePanelPositionLeader(
        PanelBouncing panel,
        Vector2 position, 
        float coefficiantDeRapprochement, 
        MenuBackgroundBouncing background,
        Vector2 center,
        float rayon,
        int n,
        int ind)
        : base(panel, position, coefficiantDeRapprochement, background) {
        this.center = center;
        this.rayon = rayon;
        this.n = n;
        this.ind = ind;
        this.startPhi = 2 * Mathf.PI / n * ind;
    }

    public override void Update() {
        Vector2 vecteurUnitaire = new Vector2(Mathf.Cos(startPhi), Mathf.Sin(startPhi));
        position = center + rayon * vecteurUnitaire;
        Debug.LogFormat("center = {0} rayon = {1} startPhi = {4} vecteurUnitaire = {2} position = {3}", center, rayon, vecteurUnitaire, position, startPhi);
        base.Update();
    }
}
