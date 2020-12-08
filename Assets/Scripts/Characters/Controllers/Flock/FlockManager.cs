using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public Vector3 CalculateMove(IController flockController) {
        Vector3 totalMove = Vector3.zero;
        for (int i = 0; i < behaviors.Count; i++) {
            FlockBehavior behavior = behaviors[i];
            float weight = weights[i];
            Vector3 move = behavior.CalculateMove(flockController);
            totalMove += move * weight;
        }
        totalMove.Normalize();
        return totalMove;
    }

    public List<IController> GetAgentsInRadius(Vector3 center, float radius) {
        return agents.FindAll(a => Vector3.Distance(center, a.transform.position) <= radius);
    }

    public List<IController> GetOtherAgentsInRadius(IController flockController, float radius) {
        List<IController> nearByAgents = GetAgentsInRadius(flockController.transform.position, radius);
        nearByAgents.Remove(flockController);
        return nearByAgents;
    }
}
