using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SingleCoroutine {

    // How to use :
    // {
    //     SingleCoroutine single =  new SingleCoroutine(this);
    //     single.Start(CMyCoroutine(myParam1, myParam2));
    //     single.Start(CMyCoroutine(myParam1, myParam2));  // This will stop the previous Coroutine to ensure unicity ! :)
    // }

    protected MonoBehaviour holder;
    protected Coroutine coroutine = null;

    public SingleCoroutine(MonoBehaviour holder) {
        this.holder = holder;
    }

    public Coroutine Start(IEnumerator generator) {
        Stop();
        coroutine = holder.StartCoroutine(generator);
        return coroutine;
    }

    public Coroutine Coroutine() {
        return coroutine;
    }

    public void Stop() {
        if (coroutine != null) {
            holder.StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}

