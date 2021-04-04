using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderIRRandom : SwappyCubesHolderIR {

    public int nbDisableIndices = 1;
    public int nbEnableIndices = 1;

    public override void Initialize(SwappyCubesHolderManager manager) {
        base.Initialize(manager);
        InitIntervals();
    }

    public void InitIntervals() {
        if(nbDisableIndices + nbEnableIndices > manager.nbIntervals) {
            Debug.LogError($"Il ne peut pas y avoir plus d'indices que d'intervals dans un {name} ! :)");
        }
        if(nbDisableIndices != nbEnableIndices) {
            Debug.LogError($"Il faut autant d'enable que de disable dans un {name} ! :)");
        }
        intervalToDisable = new List<int>();
        intervalToEnable = new List<int>();
        List<int> values = Enumerable.Range(0, manager.nbIntervals).ToList();
        for(int i = 0; i < manager.nbIntervals - nbDisableIndices - nbEnableIndices; i++) {
            values.RemoveAt(UnityEngine.Random.Range(0, values.Count));
        }
        for(int i = 0; i < values.Count; i++) {
            if(i % 2 == 0) {
                intervalToDisable.Add(values[i]);
            } else {
                intervalToEnable.Add(values[i]);
            }
        }
    }
}
