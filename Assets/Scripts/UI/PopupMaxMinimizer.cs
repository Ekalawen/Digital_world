using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupMaxMinimizer : MonoBehaviour {

    public enum State { MINIMIZED, MAXIMIZED };

    [Header("Parameters")]
    public float maximizationDuration = 0.5f;
    public float minimizationDuration = 0.5f;
    public float scrollBarMaximizedBottom = 32.8f;
    public float textMaximizedLeft = 3.0f;

    [Header("Links")]
    public RectTransform externalTerminal;
    public RectTransform scrollBar;
    public RectTransform text;
    public GameObject maximazingButton;
    public GameObject minimizingButton;

    protected State state = State.MINIMIZED;
    protected Vector2 initialExternalTerminalSize;
    protected float initialScrollBarBottom;
    protected float initialTextLeft;
    protected Fluctuator scrollBarFluctuator;
    protected Fluctuator textFluctuator;
    protected Canvas canvas;

    public void Start() {
        SetState(State.MINIMIZED);
        initialExternalTerminalSize = externalTerminal.sizeDelta;
        initialScrollBarBottom = scrollBar.offsetMin.y;
        initialTextLeft = text.offsetMin.x;
        scrollBarFluctuator = new Fluctuator(this, GetScrollBarBottom, SetScrollBarBottom);
        textFluctuator = new Fluctuator(this, GetTextLeft, SetTextLeft);
        canvas = GetComponentInParent<Canvas>();
    }

    public void Maximize() {
        SetState(State.MAXIMIZED);
        LeanTween.size(externalTerminal, canvas.GetComponent<RectTransform>().sizeDelta, maximizationDuration);
        scrollBarFluctuator.GoTo(scrollBarMaximizedBottom, maximizationDuration);
        textFluctuator.GoTo(textMaximizedLeft, maximizationDuration);
    }

    public void Minimize() {
        SetState(State.MINIMIZED);
        LeanTween.size(externalTerminal, initialExternalTerminalSize, minimizationDuration);
        scrollBarFluctuator.GoTo(initialScrollBarBottom, minimizationDuration);
        textFluctuator.GoTo(initialTextLeft, minimizationDuration);
    }

    public float GetScrollBarBottom() {
        return scrollBar.offsetMin.y;
    }

    public void SetScrollBarBottom(float value) {
        scrollBar.offsetMin = new Vector2(scrollBar.offsetMin.x, value);
    }

    public float GetTextLeft() {
        return text.offsetMin.x;
    }

    public void SetTextLeft(float value) {
        text.offsetMin = new Vector2(value, text.offsetMin.y);
    }

    protected void SetState(State newState) {
        state = newState;
        maximazingButton.SetActive(newState == State.MINIMIZED);
        minimizingButton.SetActive(newState == State.MAXIMIZED);
    }
}
