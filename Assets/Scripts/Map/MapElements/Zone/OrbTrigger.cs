using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbTrigger : IZone {

    [Header("Gameplay")]
    public float rayon = 4.5f;
    public float additionnalRayonOnEnter = 0.5f;
    public float durationToActivate = 3.0f;

    [Header("Resizing durations")]
    public float dureeConstruction = 1.0f;
    public float dureeDestruction = 1.0f;
    public float dureeAdjustRayonOnIncrease = 0.3f;
    public float dureeAdjustRayonOnDecrease = 0.3f;

    [Header("Events")]
    public UnityEvent events; // C'est déjà une liste !

    [Header("Lightning Link")]
    public GameObject lightningLinkPrefab;
    public float dureeDestructionLightningLink = 0.3f;

    [Header("GeoData")]
    public GeoData geoData;

    [Header("OtherParams")]
    public bool shouldPopCubeInCenter = false;
    public bool autoDestroyOnActivate = false;
    public bool hasToBeDestroyedBeforeEndGame = false;
    public bool shouldBeCapturedByPlayer = true;

    protected Coroutine coroutineOnEnter = null;
    protected string currentMessage = "";
    protected string precedantMessage = "";
    protected Coroutine coroutineResizing = null;
    protected bool isDestroying = false;
    protected Lightning lightning;
    protected GeoPoint geoPoint;
    [HideInInspector]
    public UnityEvent<OrbTrigger> onHack;
    [HideInInspector]
    public UnityEvent<OrbTrigger> onExit;
    protected float coefTimeToDisplayOnScreen = 1.0f;
    protected Timer timerEnter;
    protected List<OrbTriggerEnterCondition> conditions;

    public void Initialize(float rayon, float durationToActivate) {
        base.Initialize();
        gm.itemManager.AddOrbTrigger(this);
        InitializeConditions();
        onHack.AddListener(gm.itemManager.onOrbTriggerHacked.Invoke);
        onExit.AddListener(gm.itemManager.onOrbTriggerExit.Invoke);
        Resize(transform.position, Vector3.zero);
        ResizeOverTime(rayon, dureeConstruction);
        RegisterHasToBeDestroyBeforeEndGame();
        PopCubeInCenter();
        this.rayon = rayon;
        this.durationToActivate = durationToActivate;
        gm.player.onTimeHackStart.AddListener(OnTimeHackStart);
        gm.player.onTimeHackStop.AddListener(OnTimeHackStop);
        gm.eventManager.onGameOver.AddListener(DestroyLightning);
    }

    protected void InitializeConditions() {
        conditions = gameObject.GetComponents<OrbTriggerEnterCondition>().ToList();
        foreach (OrbTriggerEnterCondition condition in conditions) {
            condition.Initialize(this);
        }
    }

    public override void Resize(Vector3 center, Vector3 halfExtents) {
        base.Resize(center, halfExtents);
        GetComponent<Renderer>().material.SetFloat("_ObjectScale", halfExtents.x * 2);
    }

    public void ResizeOverTime(float rayon, float time) {
        if(isDestroying) {
            return;
        }
        if(coroutineResizing != null) {
            StopCoroutine(coroutineResizing);
        }
        coroutineResizing = StartCoroutine(CResizeOverTime(rayon, time));
    }

    protected IEnumerator CResizeOverTime(float rayon, float duree) {
        float initialRayon = transform.localScale.x / 2;
        float dureeWithTimeHack = duree / gm.player.GetTimeHackCurrentSlowmotionFactor();
        Timer timer = new Timer(dureeWithTimeHack);
        while(!timer.IsOver()) {
            Resize(transform.position, Vector3.one * MathCurves.Linear(initialRayon, rayon, timer.GetAvancement()));
            yield return null;
        }
        Resize(transform.position, Vector3.one * rayon);
    }

    protected bool IsTimeHackOn() {
        return gm.player.IsTimeHackOn();
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player" && AllConditionsAreFullfilled()) {
            if (coroutineOnEnter != null) {
                StopCoroutine(coroutineOnEnter);
            }
            coroutineOnEnter = StartCoroutine(COnEnter(other));
            ResizeOverTime(rayon + additionnalRayonOnEnter, dureeAdjustRayonOnIncrease);
            InstantiateLightningLink();
            AddGeoPoint();
        }
    }

    protected bool AllConditionsAreFullfilled() {
        foreach(OrbTriggerEnterCondition condition in conditions) {
            if(!condition.IsFullfilled()) {
                condition.DisplayNotFullfilledMessage();
                return false;
            }
        }
        return true;
    }

    public void ReduceAndDestroy() {
        ResizeOverTime(0, dureeDestruction);
        SetIsDestroying();
        gm.itemManager.RemoveOrbTrigger(this);
        Destroy(gameObject, dureeDestruction);
    }

    protected IEnumerator COnEnter(Collider other) {
        gm.soundManager.PlayTimeZoneButtonInClip(transform.position);

        timerEnter = new Timer(durationToActivate / gm.player.GetTimeHackCurrentSlowmotionFactor());
        while(!timerEnter.IsOver())
        {
            if (gm.eventManager.IsGameOver())
            {
                break;
            }
            float timeToDisplayOnScreen = GetTimeToDisplayOnScreen(timerEnter);
            currentMessage = TimerManager.TimerToString(timeToDisplayOnScreen);
            gm.console.AjouterMessageImportant(currentMessage,
                Console.TypeText.BLUE_TEXT,
                0.1f,
                bAfficherInConsole: false,
                precedantMessage);
            precedantMessage = currentMessage;
            UpdateLightningLink();
            yield return null;
        }

        if (!gm.eventManager.IsGameOver()) {
            AsyncOperationHandle<string> hackedHandle = gm.console.strings.lumiereProtectionHacked.GetLocalizedString();
            yield return hackedHandle;
            gm.console.AjouterMessageImportant(hackedHandle.Result,
                Console.TypeText.GREEN_TEXT,
                1.0f / gm.player.GetTimeHackCurrentSlowmotionFactor(),
                bAfficherInConsole: true,
                precedantMessage);
            precedantMessage = currentMessage;

            CallAllEvents();
        }
        coroutineOnEnter = null;
    }

    protected float GetTimeToDisplayOnScreen(Timer timer) {
        return timer.GetRemainingTime() * gm.player.GetTimeHackCurrentSlowmotionFactor();
    }

    protected Vector3 InFrontOfPlayerPosition() {
        Vector3 playerPos = gm.player.transform.position;
        return playerPos + (gm.player.camera.transform.forward + gm.gravityManager.Down() + (transform.position - playerPos).normalized) * 0.3f;
    }

    protected void UpdateLightningLink() {
        if(lightning != null) {
            lightning.SetPosition(InFrontOfPlayerPosition(), transform.position, parentSize: transform.localScale.x);
        }
    }

    protected void InstantiateLightningLink() {
        Vector3 inFrontOfPlayerPosition = InFrontOfPlayerPosition();
        lightning = Instantiate(lightningLinkPrefab, inFrontOfPlayerPosition, Quaternion.identity, parent: transform).GetComponent<Lightning>();
        lightning.Initialize(inFrontOfPlayerPosition, transform.position, Lightning.PivotType.EXTREMITY);
    }

    public Lightning GetLightning() {
        return lightning;
    }

    protected void AddGeoPoint() {
        geoPoint = gm.player.geoSphere.AddGeoPoint(geoData);
    }

    protected void RemoveGeoPoint() {
        if(geoPoint != null) {
            geoPoint.Stop();
            geoPoint = null;
        }
    }


    public void CallAllEvents() {
        foreach(OrbTriggerEnterCondition condition in conditions) {
            condition.OnTrigger();
        }
        events.Invoke();
        onHack.Invoke(this);
        gm.soundManager.PlayOrbTriggerActivationClip(transform.position);
        if (autoDestroyOnActivate) {
            ReduceAndDestroy();
        }
        if(hasToBeDestroyedBeforeEndGame) {
            gm.eventManager.RemoveElementToBeDoneBeforeEndGame(this);
        }
    }

    protected override void OnExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (coroutineOnEnter != null) {
                StopCoroutine(coroutineOnEnter);
                coroutineOnEnter = null;
            }
            RemoveGeoPoint();

            gm.soundManager.PlayTimeZoneButtonOutClip(transform.position);
            ResizeOverTime(rayon, dureeAdjustRayonOnDecrease);
            DestroyLightning();
            onExit.Invoke(this);
        }
    }

    protected void DestroyLightning() {
        if (lightning != null) {
            lightning.StopRefreshing();
            Destroy(lightning.gameObject, lightning.refreshRate);
            lightning = null;
        }
    }

    protected void PopCubeInCenter() {
        if (!shouldPopCubeInCenter)
            return;
        Cube cube = gm.map.AddCube(transform.position, Cube.CubeType.NORMAL);
        if (cube != null) {
            cube.RegisterCubeToColorSources();
        }
    }

    public void AddEvent(UnityAction call) {
        events.AddListener(call);
    }

    public void SetIsDestroying() {
        isDestroying = true;
    }

    public bool IsDestroying() {
        return isDestroying;
    }

    protected void RegisterHasToBeDestroyBeforeEndGame() {
        if(hasToBeDestroyedBeforeEndGame) {
            gm.eventManager.AddElementToBeDoneBeforeEndGame(this);
        }
    }

    private void OnTimeHackStart(PouvoirTimeHack timeHack) {
        if(coroutineOnEnter == null) {
            return;
        }
        float targetAvancement = timerEnter.GetAvancement();
        timerEnter = new Timer(durationToActivate / timeHack.slowmotionFactor);
        timerEnter.SetAvancement(targetAvancement);
    }

    protected void OnTimeHackStop(PouvoirTimeHack timeHack) {
        if(coroutineOnEnter == null) {
            return;
        }
        float targetAvancement = timerEnter.GetAvancement();
        timerEnter = new Timer(durationToActivate / 1);
        timerEnter.SetAvancement(targetAvancement);
    }
}
