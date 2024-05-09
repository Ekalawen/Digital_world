using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BlockLumiereConditionnalItem : BlockLumiere {

    public BlockLumiereConditionnalCube.ConditionType conditionType = BlockLumiereConditionnalCube.ConditionType.Presence;
    public List<Item> necessaryItems;

    protected Block block;

    public override void Initialize() {
        base.Initialize();
        block = GetComponentInParent<Block>();
    }

    public override bool CanBePicked() {
        if (conditionType == BlockLumiereConditionnalCube.ConditionType.Presence) {
            return necessaryItems.All(i => i && block.IsItemAt(i.transform.position));
        }
        return necessaryItems.All(i => !i || !block.IsItemAt(i.transform.position));
    }
}
