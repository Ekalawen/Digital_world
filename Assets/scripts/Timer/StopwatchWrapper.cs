using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using System.Diagnostics;

public class StopwatchWrapper {

    public static double Mesure(Action action, string methodName = "") {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        double nbMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        string finalMethodName = methodName != "" ? methodName : action.Method.Name;
        UnityEngine.Debug.Log($"{finalMethodName} : {nbMilliseconds}ms");
        return nbMilliseconds;
    }

}