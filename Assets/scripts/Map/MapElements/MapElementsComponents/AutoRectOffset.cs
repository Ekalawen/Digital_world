using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRectOffset : MonoBehaviour {

    public Vector2 direction = Vector2.up;
    public float magnitude = 10.0f;
    public float frequence = 0.3f;
    public AnimationCurve curve = null;

    void Start() {
        StartCoroutine(CStartOffseting());
    }

    protected IEnumerator CStartOffseting() {
        Timer timer = new Timer(frequence);
        float vitesse = magnitude / frequence;
        bool forward = true;
        direction = direction.normalized;
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 initialPosition = rect.position;
        while(true) {
            if(timer.IsOver()) {
                timer.Reset();
                forward = !forward;
            }
            float avancement = timer.GetAvancement();
            avancement = curve.Evaluate(forward ? avancement : (1 - avancement));
            Vector2 position = initialPosition + direction * magnitude * avancement;
            rect.position = position;
            yield return null;
        }
    }
}
