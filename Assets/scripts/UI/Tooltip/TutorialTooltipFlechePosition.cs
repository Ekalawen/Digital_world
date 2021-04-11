using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TutorialTooltipFlechePosition : MonoBehaviour {

    public enum FlecheTrackingType {
        LEVEL,
        CADENAS,
    };

    public RectTransform background;
    public FlecheTrackingType flecheTrackingType;
    public float coefAvancement = 0.5f;

    protected SelectorManager selectorManager;
    protected List<Vector3> interestPoints;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        ComputeInterestPoints();
    }

    protected void ComputeInterestPoints() {
        if(flecheTrackingType == FlecheTrackingType.LEVEL) {
            interestPoints = selectorManager.GetLevels().Select(l => l.objectLevel.transform.position).ToList();
        } else {
            interestPoints = selectorManager.GetPaths().Select(p => p.cadena.transform.position).ToList();
        }
    }

    public void Update() {
        List<Vector2> interestPointsProjected = ComputeInterestPointsProjected();
        Vector2 backgroundCenter = background.transform.position;
        Vector2 bestPoint = interestPointsProjected.OrderBy(p => Vector2.Distance(backgroundCenter, p)).First();
        transform.position = backgroundCenter * (1 - coefAvancement) + bestPoint * coefAvancement;
        float angle = Vector2.SignedAngle(Vector2.right, bestPoint - backgroundCenter);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected List<Vector2> ComputeInterestPointsProjected() {
        Camera camera = Camera.main;
        return interestPoints.Select(p => (Vector2)camera.WorldToViewportPoint(p) * new Vector2(Screen.width, Screen.height)).ToList();
    }
}
