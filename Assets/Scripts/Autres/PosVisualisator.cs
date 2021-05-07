using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PosVisualisator : MonoBehaviour {

    public KeyCode keyCode = KeyCode.Mouse2;
    public GameObject visualisatorPrefab;

    protected GameManager gm;
    protected Player.EtatPersonnage etatPlayer = Player.EtatPersonnage.AU_SOL;

    public void Start() {
        gm = GameManager.Instance;
    }

    void FixedUpdate() {
        if(Input.GetKeyDown(keyCode)) {
            CreateObjectAtPlayerPos();
        }

        //if(gm.player.GetEtat() == Player.EtatPersonnage.EN_CHUTE && etatPlayer != Player.EtatPersonnage.EN_CHUTE) {
        //    CreateObjectAtPlayerPos();
        //}
        //etatPlayer = gm.player.GetEtat();
    }

    public void CreateObjectAtPlayerPos()
    {
        GameObject go = Instantiate(visualisatorPrefab, gm.player.transform.position, gm.player.transform.rotation);
        go.transform.localScale = gm.player.transform.localScale;
    }

    public void CreateObjectAtPos(Vector3 pos) {
        GameObject go = Instantiate(visualisatorPrefab, pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * 0.1f;
    }

    public static void DrawCross(Vector3 pos, Color color, bool depthTest = false) {
        float sizeCross = 1f / 4f;
        Debug.DrawLine(pos + Vector3.up * sizeCross, pos - Vector3.up * sizeCross, color, 10000, depthTest);
        Debug.DrawLine(pos + Vector3.forward * sizeCross, pos - Vector3.forward * sizeCross, color, 10000, depthTest);
        Debug.DrawLine(pos + Vector3.right * sizeCross, pos - Vector3.right * sizeCross, color, 10000, depthTest);
    }

    public static void DrawCube(Vector3 center, Vector3 halfExtents, Color color, bool depthTest = false) {
        List<Tuple<Vector3, Vector3>> edges = MathTools.GetBoxEdges();
        for(int i = 0; i < edges.Count; i++) {
            Vector3 start = center + MathTools.VecMul(edges[i].Item1, halfExtents);
            Vector3 end = center + MathTools.VecMul(edges[i].Item2, halfExtents);
            Debug.DrawLine(start, end, color, 10000, depthTest: depthTest);
        }
    }

    public static void DrawCube(CubeInt cube, Color color, bool depthTest = false) {
        DrawCube(cube.mapCenter, cube.halfExtents, color, depthTest);
    }

    public static void DrawPath(List<Vector3> path, Color color) {
        if(path.Count > 0) {
            DrawCross(path.First(), Color.green);
            DrawCross(path.Last(), Color.red);
            for(int i = 1; i < path.Count - 1; i++) {
                DrawCross(path[i], color);
            }
        }
    }
}
