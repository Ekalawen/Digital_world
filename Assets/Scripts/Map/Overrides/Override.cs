using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Override: MonoBehaviour {

    protected GameManager gm;
    protected MapManager map;

    public void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
    }
}
