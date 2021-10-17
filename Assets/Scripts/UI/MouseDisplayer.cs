using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseDisplayer : MonoBehaviour {

    public enum State { IDLE, CLICKED };

    static MouseDisplayer _instance;
    public static MouseDisplayer Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<MouseDisplayer>()); } }

    public Image image;
    public float pixelSize = 48;
    [ColorUsage(true, true)]
    public Color clickedColor;
    public float clickedSpeed = 10.0f;
    public float clickedRotation = 15.0f;
    public float clickedRotationDuration = 0.1f;
    public bool isInGame = false;
    [ConditionalHide("!isInGame")]
    public Camera cam;
    public bool useOverlay = false;

    protected State state = State.IDLE;
    protected InputManager inputManager;
    protected Color idleColor;
    protected float idleSpeed;
    protected float planeDistance;
    protected Fluctuator rotationFluctuator;

    public void Awake() {
        if (!_instance) {
            _instance = this;
        }
    }

    public void Start() {
        if (!isInGame) {
            ShowCursor();
        } else {
            cam = GameManager.Instance.player.camera;
            HideCursor();
        }
        SetSize();
        inputManager = InputManager.Instance;
        idleColor = image.material.GetColor("_EdgesColor");
        idleSpeed = image.material.GetVector("NoiseSpeed").y;
        planeDistance = GetComponentInParent<Canvas>().planeDistance;
        rotationFluctuator = new Fluctuator(this, GetRotation, SetRotation, useUnscaleTime: true);
    }

    void Update() {
        UpdatePosition();
        UpdateState();
    }

    protected void UpdateState() {
        if(state == State.IDLE) {
            if(inputManager.GetMouseLeftClickDown()) {
                SetState(State.CLICKED);
            }
        } else if (state == State.CLICKED) {
            if(inputManager.GetMouseLeftClickUp()) {
                SetState(State.IDLE);
            }
        }
    }

    protected void SetState(State newState) {
        state = newState;
        float speed = newState == State.IDLE ? idleSpeed : clickedSpeed;
        Color color = newState == State.IDLE ? idleColor : clickedColor;
        image.material.SetColor("_EdgesColor", color);
        image.material.SetVector("NoiseSpeed", new Vector2(0, speed));
        float angle = newState == State.IDLE ? 0 : clickedRotation;
        rotationFluctuator.GoTo(angle, clickedRotationDuration);
    }

    public void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowCursor() {
        image.enabled = true;
        UnlockCursor();
        Cursor.visible = false;
    }

    public void HideCursor() {
        image.enabled = false;
        LockCursor();
        Cursor.visible = false;
    }

    public void ShowTrueCursor() {
        UnlockCursor();
        Cursor.visible = true;
    }

    public void HideTrueCursor() {
        LockCursor();
        Cursor.visible = false;
    }

    protected void SetSize() {
        image.rectTransform.sizeDelta = Vector2.one * pixelSize;
    }

    protected void UpdatePosition() {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (!useOverlay) {
            Vector3 localPoint;
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = planeDistance;
            localPoint = cam.ScreenToWorldPoint(screenPoint);
            rectTransform.position = localPoint;
        } else {
            RectTransform screen = transform.parent.GetComponent<RectTransform>();
            Vector2 localPoint2D;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screen, Input.mousePosition, null, out localPoint2D);
            rectTransform.localPosition = localPoint2D;
        }
    }

    protected float GetRotation() {
        return GetComponent<RectTransform>().eulerAngles.z;
    }

    protected void SetRotation(float rotation) {
        Vector3 angles = GetComponent<RectTransform>().eulerAngles;
        angles.z = rotation;
        GetComponent<RectTransform>().eulerAngles = angles;
    }

}
