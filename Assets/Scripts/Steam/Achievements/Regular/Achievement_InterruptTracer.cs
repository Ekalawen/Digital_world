using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_InterruptTracer : Achievement {

    [Header("Parameters")]
    public int nbTimesForATracer = 1;

    protected Dictionary<TracerBlast, int> nbTimesByTracer;

    protected override void InitializeSpecific() {
        nbTimesByTracer = new Dictionary<TracerBlast, int>();
        gm.ennemiManager.onInterruptTracer.AddListener(OnInterruptTracer);
    }

    public void OnInterruptTracer(TracerBlast tracer) {
        if(nbTimesByTracer.ContainsKey(tracer)) {
            nbTimesByTracer[tracer] += 1;
        } else {
            nbTimesByTracer[tracer] = 1;
        }
        if(nbTimesByTracer[tracer] >= nbTimesForATracer) {
            Unlock();
        }
    }
}
