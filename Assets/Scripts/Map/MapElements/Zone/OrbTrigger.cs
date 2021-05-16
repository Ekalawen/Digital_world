using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbTrigger : IZone {

    public float rayon = 1.5f;
    public float durationToActivate = 3.0f;
    public bool shouldPopCubeInCenter = false;
    public bool autoDestroyOnActivate = false;
    public UnityEvent events; // C'est déjà une liste !

    protected Coroutine coroutineOnEnter = null;
    protected string currentMessage = "";
    protected string precedantMessage = "";

    protected override void Start() {
        base.Start();
        Resize(transform.position, Vector3.one * rayon);
        PopCubeInCenter();
    }

    public void Initialize(float rayon, float durationToActivate) {
        Initialize();
        this.rayon = rayon;
        this.durationToActivate = durationToActivate;
    }

    public override void Resize(Vector3 center, Vector3 halfExtents) {
        base.Resize(center, halfExtents);
        GetComponent<Renderer>().material.SetFloat("_ObjectScale", halfExtents.x * 2);
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (coroutineOnEnter != null) {
                StopCoroutine(coroutineOnEnter);
            }
            coroutineOnEnter = StartCoroutine(COnEnter(other));
        }
    }

    protected IEnumerator COnEnter(Collider other) {
        gm.soundManager.PlayTimeZoneButtonInClip(transform.position);

        Timer timer = new Timer(durationToActivate);
        while(!timer.IsOver()) {
            if (gm.eventManager.IsGameOver()) {
                break;
            }
            currentMessage = TimerManager.TimerToString(timer.GetRemainingTime());
            gm.console.AjouterMessageImportant(currentMessage,
                Console.TypeText.ALLY_TEXT,
                0.1f,
                bAfficherInConsole: false,
                precedantMessage);
            precedantMessage = currentMessage;
            yield return null;
        }

        AsyncOperationHandle<string> hackedHandle = gm.console.strings.lumiereProtectionHacked.GetLocalizedString();
        yield return hackedHandle;
        gm.console.AjouterMessageImportant(hackedHandle.Result,
            Console.TypeText.BASIC_TEXT,
            1.0f,
            bAfficherInConsole: true,
            precedantMessage);
        precedantMessage = currentMessage;

        CallAllEvents();
    }

    public void CallAllEvents() {
        events.Invoke();
        if (autoDestroyOnActivate) {
            Destroy(transform.parent.gameObject); // Attention ça c'est du au fait qu'on a un composant parent inutile pour les OrbTriggers !
        }
    }

    protected override void OnExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (coroutineOnEnter != null) {
                StopCoroutine(coroutineOnEnter);
            }

            gm.soundManager.PlayTimeZoneButtonOutClip(transform.position);
        }
    }

    protected void PopCubeInCenter() {
        if (!shouldPopCubeInCenter)
            return;
        Cube cube = gm.map.AddCube(transform.position, Cube.CubeType.NORMAL);
    }

    public void AddEvent(UnityAction call) {
        events.AddListener(call);
    }
}
