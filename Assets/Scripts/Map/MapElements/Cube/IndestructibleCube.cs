using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndestructibleCube : NonBlackCube {
    // La non destructibilité est en fait géré par les déstructeurs de cubes.
    // Ils checkent si le cube est de type Indestructible ou non avant de prendre leur décision.
    // Ils checkent aussi l'attribut "bIsDestructible" ... x)
}
