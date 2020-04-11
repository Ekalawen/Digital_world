using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeZoneButton : IZone {

    public float rayon = 1.5f;
    public float durationToActivate = 3.0f;
    public bool shouldPopCubeInCenter = true;
    public UnityEvent events; // C'est déjà une liste !

    protected Coroutine coroutine = null;
    protected string currentMessage = "";
    protected string precedantMessage = "";

    protected override void Start() {
        base.Start();
        Resize(transform.position, Vector3.one * rayon);
        PopCubeInCenter();
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(COnEnter(other));
        }
    }

    protected IEnumerator COnEnter(Collider other) {
        gm.soundManager.PlayTimeZoneButtonInClip(transform.position);

        Timer timer = new Timer(durationToActivate);
        while(!timer.IsOver()) {
            currentMessage = TimerManager.TimerToString(timer.GetRemainingTime());
            gm.console.AjouterMessageImportant(currentMessage,
                Console.TypeText.ALLY_TEXT,
                0.1f,
                bAfficherInConsole: false,
                precedantMessage);
            precedantMessage = currentMessage;
            yield return null;
        }

        currentMessage = "Hacké !";
        gm.console.AjouterMessageImportant(currentMessage,
            Console.TypeText.BASIC_TEXT,
            1.0f,
            bAfficherInConsole: true,
            precedantMessage);
        precedantMessage = currentMessage;

        CallAllEvents();
    }

    public void CallAllEvents() {
        events.Invoke();
    }

    protected override void OnExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (coroutine != null)
                StopCoroutine(coroutine);

            //currentMessage = "Déconnecté !";
            //gm.console.AjouterMessageImportant(currentMessage,
            //    Console.TypeText.ENNEMI_TEXT,
            //    1.0f,
            //    bAfficherInConsole: true,
            //    precedantMessage);
            //precedantMessage = currentMessage;
            
            gm.soundManager.PlayTimeZoneButtonOutClip(transform.position);
        }
    }

    protected void PopCubeInCenter() {
        if (!shouldPopCubeInCenter)
            return;
        Cube cube = gm.map.AddCube(transform.position, Cube.CubeType.NORMAL);
        //Material material = GetComponent<MeshRenderer>().material;
        //cube.GetComponent<MeshRenderer>().material = material;
    }

    public void AddEvent(UnityAction call) {
        events.AddListener(call);
    }
}
