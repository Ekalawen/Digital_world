using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class UILineRenderer : MonoBehaviour {

    public List<Vector2> positions;
    public float thickness = 3.0f;
    public GameObject linePrefab;
    public GameObject nodePrefab;

    protected List<GameObject> lines;
    protected List<GameObject> nodes;

    public void Initialize(List<Vector2> positions) {
        this.positions = positions;
        Initialize();
    }

    public void Initialize() {
        lines = new List<GameObject>();
        nodes = new List<GameObject>();

        if (positions.Count < 2) {
            return;
        }

        for (int i = 0; i < positions.Count - 1; ++i) {
            CreateLineBetween(positions[i], positions[i + 1]);
        }
        CreateNodes();
    }

    protected void CreateNodes() {
        if(!nodePrefab) {
            return;
        }
        positions.ForEach(p => CreateNodeAt(p));
    }

    protected void CreateLineBetween(Vector2 source, Vector2 target) {
        GameObject line = Instantiate(linePrefab, transform);
        RectTransform rect = line.GetComponent<RectTransform>();
        Vector2 direction = (target - source).normalized;
        float distance = (target - source).magnitude;
        rect.sizeDelta = new Vector2(distance, thickness);
        rect.anchoredPosition = (source + target) / 2.0f;
        rect.localEulerAngles = new Vector3(0, 0, Vector2.Angle(Vector2.right, direction));
    }

    protected void CreateNodeAt(Vector2 pos) {
        GameObject node = Instantiate(nodePrefab, transform);
        RectTransform rect = node.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        nodes.Add(node);
    }
}
