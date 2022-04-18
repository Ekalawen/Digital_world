using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GainPouvoirOnEnterBlock : MonoBehaviour {

    public GameObject pouvoirPrefab;
    public PouvoirGiverItem.PouvoirBinding pouvoirBinding;

    protected Block block;
    protected bool hasGivenPouvoir = false;

    public void Start() {
        block = GetComponent<Block>();
        block.onEnterBlock.AddListener(OnEnterBlock);
    }

    protected void OnEnterBlock(Block block) {
        if (hasGivenPouvoir) {
            return;
        }
        GivePouvoir();
    }

    protected void GivePouvoir() {
        hasGivenPouvoir = true;
        PouvoirGiverItem.GivePouvoir(GameManager.Instance, pouvoirPrefab, pouvoirBinding);
    }
}
