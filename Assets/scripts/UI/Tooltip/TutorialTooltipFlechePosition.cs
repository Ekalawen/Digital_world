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

    protected RectTransform screen;
    protected SelectorManager selectorManager;
    protected List<Vector3> interestPoints;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        screen = selectorManager.screen;
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
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 backgroundCenter = background.GetComponent<RectTransform>().anchoredPosition;
        Vector2 bestPoint = interestPointsProjected.OrderBy(p => Vector2.Distance(backgroundCenter, p)).First();
        rect.anchoredPosition = backgroundCenter * (1 - coefAvancement) + bestPoint * coefAvancement;
        float angle = Vector2.SignedAngle(Vector2.right, bestPoint - backgroundCenter);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, angle);
    }

    protected List<Vector2> ComputeInterestPointsProjected() {
        Camera camera = selectorManager.overlayCamera;
        return interestPoints.Select(worldPoint => {
            Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);
            Vector2 rectanglePoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPoint, camera, out rectanglePoint);
            return rectanglePoint;
        }).ToList();
    }
}
