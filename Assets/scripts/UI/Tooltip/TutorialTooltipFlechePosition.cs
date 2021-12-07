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
        RECT_TRANSFORM,
    };

    public RectTransform background;
    public FlecheTrackingType flecheTrackingType;
    [ConditionalHide("!flecheTrackingType", FlecheTrackingType.RECT_TRANSFORM)]
    public bool useSpecificIndice = false;
    [ConditionalHide("useSpecificIndice")]
    public int specificIndice = 0;
    [ConditionalHide("flecheTrackingType", FlecheTrackingType.RECT_TRANSFORM)]
    public RectTransform rectTransformToTrack;
    public float coefAvancement = 0.5f;

    protected RectTransform screen;
    protected SelectorManager selectorManager;
    protected List<Vector3> interestPoints;
    protected Canvas canvas;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        canvas = GetComponentInParent<Canvas>();
        screen = selectorManager.screen;
        ComputeInterestPoints();
    }

    protected void ComputeInterestPoints() {
        if(flecheTrackingType == FlecheTrackingType.LEVEL) {
            List<SelectorLevel> levels = selectorManager.GetLevels();
            if(useSpecificIndice) {
                levels = levels.FindAll(l => selectorManager.GetLevelIndice(l) == specificIndice);
            }
            interestPoints = levels.Select(l => l.objectLevel.transform.position).ToList();
        } else if(flecheTrackingType == FlecheTrackingType.CADENAS) {
            List<SelectorPath> paths = selectorManager.GetPaths();
            if(useSpecificIndice) {
                paths = paths.FindAll(p => selectorManager.GetPathIndice(p) == specificIndice);
            }
            interestPoints = paths.Select(p => p.cadena.transform.position).ToList();
        }
    }

    public void Update() {
        if (flecheTrackingType != FlecheTrackingType.RECT_TRANSFORM) {
            UpdateFlechePositionForWorldMode();
        } else {
            UpdateFlechePositionForCanvasMode();
        }
    }

    protected void UpdateFlechePositionForCanvasMode() {
        Vector3 interestPoint = canvas.transform.InverseTransformPoint(rectTransformToTrack.transform.position);
        Vector3 backgroundCenter = canvas.transform.InverseTransformPoint(background.transform.position);
        Vector3 middlePoint = Vector3.Lerp(backgroundCenter, interestPoint, coefAvancement);
        transform.position = canvas.transform.TransformPoint(middlePoint);

        float angle = Vector3.SignedAngle(Vector3.right, interestPoint - middlePoint, Vector3.forward);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, angle);
    }

    protected void UpdateFlechePositionForWorldMode() {
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
