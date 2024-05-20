using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LumiereReward : MonoBehaviour
{

    protected GameManager gm;
    protected Lumiere lumiere;

    public void Initialize(Lumiere lumiere) {
        gm = GameManager.Instance;
        this.lumiere = lumiere;
    }

    public abstract void Reward();
}
