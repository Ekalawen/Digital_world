using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Scan {

    public float duree = 2.0f;
    public float speed = 15.0f;
    public float width = 2.0f;

    protected Player player;
    protected Timer timer;

    public Scan(float duree, float speed, float width) {
        this.duree = duree;
        this.speed = speed;
        this.width = width;
        player = GameManager.Instance.player;
        timer = new Timer(duree);
    }

    public bool IsIn(Vector3 position) {
        float elapsed = timer.GetElapsedTime();
        float centerBandDistance = elapsed * speed;
        float distanceToPosition = Vector3.Distance(position, player.transform.position);
        return Mathf.Abs(distanceToPosition - centerBandDistance) <= width;
    }

    public bool IsOver() {
        return timer.IsOver();
    }
}
