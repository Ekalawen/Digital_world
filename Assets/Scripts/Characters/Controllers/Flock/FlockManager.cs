using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour {

    public List<FlockBehavior> behaviors;
    public List<float> weights;

    protected GameManager gm;
    protected List<IController> agents;

    public void Initialize() {
        gm = GameManager.Instance;
        agents = new List<IController>();
        if(behaviors.Count != weights.Count) {
            Debug.LogWarning("Le FlockManager doit avoir autant de behaviors que de weights ! ;)");
        }
        foreach(FlockBehavior behavior in behaviors) {
            behavior.Initialize();
        }
    }

    public void Register(IController flockController) {
        agents.Add(flockController);
    }

    public void Unregister(IController flockController) {
        agents.Remove(flockController);
    }

    public Vector3 GetFlockMove(IController flockController) {
        Vector3 totalMove = Vector3.zero;
        for (int i = 0; i < behaviors.Count; i++) {
            FlockBehavior behavior = behaviors[i];
            float weight = weights[i];
            totalMove += behavior.GetMove(flockController) * weight;
        }
        totalMove /= behaviors.Count;
        return totalMove;
    }
}
