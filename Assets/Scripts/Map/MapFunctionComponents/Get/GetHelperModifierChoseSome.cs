using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierChoseSome : GetHelperModifier {

    public enum ChoseMethod { QUANTITY, PROPORTION };
    public ChoseMethod method = ChoseMethod.QUANTITY;
    [ConditionalHide("method", ChoseMethod.QUANTITY)]
    public int quantity = 1;
    [ConditionalHide("method", ChoseMethod.PROPORTION)]
    public float proportion = 0.1f;

    public override List<Cube> ModifyCubes(List<Cube> cubes) {
        return ChoseSomeAccoringToMethod(cubes);
    }

    public override List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return ChoseSomeAccoringToMethod(positions);
    }

    public override bool IsInArea(Vector3 position) {
        // Not used;
        return false;
    }

    public List<T> ChoseSomeAccoringToMethod<T>(List<T> list) {
        if(method == ChoseMethod.QUANTITY) {
            if(quantity > list.Count) {
                return list;
            }
            return GaussianGenerator.SelecteSomeNumberOf(list, quantity);
        }
        // ChoseMethode.PROPORTION
        return GaussianGenerator.SelectSomeProportionOfSureMethod(list, proportion);
    }
}
