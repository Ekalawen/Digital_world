using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Lumin;

public class BlackoutEvent : RandomEvent {

    public float ambientIntensity = 0.1f;
    public float reflectionIntensity = 0.1f;
    public float coefficiantAttenuationLumieres = 0.3f;
    public float pourcentageBlinkStart = 0.1f;
    public int nbBlinksStarts = 11;

    protected Color oldAmbientLight;
    protected float oldAmbientIntensity;
    protected float oldReflectionIntensity;

    public override void StartEvent() {
        StartCoroutine(BlinkStart());
    }

    protected IEnumerator BlinkStart() {
        if (nbBlinksStarts % 2 == 0) // Il ne faut pas que l'on éteigne 2 fois d'affilé les lumières à la fin !
            nbBlinksStarts++;
        float dureeBlink = dureeCourante * pourcentageBlinkStart;
        List<float> timings = new List<float>();
        for(int i = 0; i < nbBlinksStarts; i++) {
            timings.Add(Random.Range(0.0f, dureeBlink));
        }
        timings.Sort();
        bool shouldSwitchOff = true;
        for(int i = 0; i < nbBlinksStarts - 1; i++) {
            float duree = timings[i + 1] - timings[i];
            if (shouldSwitchOff)
                SwitchOff(duree);
            else
                SwitchOn();
            shouldSwitchOff = !shouldSwitchOff;
            yield return new WaitForSeconds(duree);
        }
        SwitchOff(dureeCourante - dureeBlink);
    }

    public override void EndEvent() {
        SwitchOn();
    }

    public override void StartEventConsoleMessage() {
        gm.console.BlackoutMessage();
    }

    protected void SwitchOn() {
        RenderSettings.ambientLight = oldAmbientLight;
        RenderSettings.ambientIntensity = oldAmbientIntensity;
        RenderSettings.reflectionIntensity = oldReflectionIntensity;
    }

    protected void SwitchOff(float duree) {
        oldAmbientLight = RenderSettings.ambientLight;
        oldAmbientIntensity = RenderSettings.ambientIntensity;
        oldReflectionIntensity = RenderSettings.reflectionIntensity;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.reflectionIntensity = reflectionIntensity;

        foreach(Lumiere lumiere in gm.map.GetLumieres()) {
            StartCoroutine(SwitchOffLight(lumiere, duree));
        }
    }

    protected IEnumerator SwitchOffLight(Lumiere lumiere, float duree) {
        Light light = lumiere.GetComponentInChildren<Light>();
        light.intensity *= coefficiantAttenuationLumieres;
        yield return new WaitForSeconds(duree);
        if (light != null)
            light.intensity *= (1 / coefficiantAttenuationLumieres);
    }
}
