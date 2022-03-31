using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using System;
using UnityEngine.Assertions;

public abstract class GenerateRandomFillingModifier : MonoBehaviour {

    protected GenerateRandomFilling generateRandomFilling;

    public virtual void InitializeSpecific(GenerateRandomFilling generateRandomFilling) {
        this.generateRandomFilling = generateRandomFilling;
    }

    public abstract FullBlock ModifyFullBlock(FullBlock fullBlock);
}
