using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionEvent : RandomEvent {

    [Header("Previsualisation")]
    public float timeBeforeIntersection = 1.5f;
    public Texture previsualisationTexture;

    [Header("Effect")]
    public float timeIntersection = 1.0f;
    public Cube.CubeType cubeTypeWhileEffect = Cube.CubeType.SPECIAL;
    public float cubeWhileEffectDissolveTime = 0.1f;

    [Header("End Effect")]
    public float timeBeforeSwapCubeTypeAgain = 1.0f;
    public Cube.CubeType cubeTypeAfterEffect = Cube.CubeType.SPECIAL;
    public float cubeAfterEffectDissolveTime = 0.1f;

    protected static List<Cube> cubesAlreadyUsedInEvents = new List<Cube>();

    protected List<Coroutine> coroutines = new List<Coroutine>();

    protected override void StartEvent() {
        Vector3 center = GetIntersectionCenter();
        List<Vector3> extremites = GetExtremites(center);
        foreach(Vector3 extremite in extremites) {
            Coroutine coroutine = StartCoroutine(CCreateLine(center, extremite));
            coroutines.Add(coroutine);
        }
    }

    protected override void EndEvent() {
        coroutines.Clear();
    }

    protected Vector3 GetIntersectionCenter() {
        return MathTools.Round(gm.player.transform.position);
    }

    protected override void StartEventConsoleMessage() {
    }

    protected IEnumerator CCreateLine(Vector3 center, Vector3 extremite) {
        Cube extremiteCube = gm.map.GetCubeAt(extremite);
        if (extremiteCube != null && IsNotAlreadySetupForAnOtherIntersectionEvent(extremiteCube)) {
            Texture oldTexture = PrevisualisationOfLine(extremiteCube);
            yield return new WaitForSeconds(timeBeforeIntersection);
            Coroutine coroutineCreate = StartCoroutine(CCreateBridge(center, extremite));
            coroutines.Add(coroutineCreate);
            yield return new WaitForSeconds(timeIntersection);
            UndoPrevisualisationOfIntersections(extremiteCube, oldTexture);
            yield return new WaitForSeconds(timeBeforeSwapCubeTypeAgain);
            Coroutine coroutineSwap = StartCoroutine(SwapCubeTypesForExtremite(center, extremite));
            coroutines.Add(coroutineSwap);
        }
    }

    protected bool IsNotAlreadySetupForAnOtherIntersectionEvent(Cube extremiteCube) {
        return !cubesAlreadyUsedInEvents.Contains(extremiteCube);
    }

    protected Texture PrevisualisationOfLine(Cube extremiteCube) {
        Texture texture = extremiteCube.GetMaterial().GetTexture("_ColorTexture");
        extremiteCube.SetTexture(previsualisationTexture);
        extremiteCube.SetColor(gm.colorManager.GetNotBlackColorForPosition(extremiteCube.transform.position));
        cubesAlreadyUsedInEvents.Add(extremiteCube);
        return texture;
    }

    protected IEnumerator CCreateBridge(Vector3 center, Vector3 extremite) {
        Vector3 direction = (center - extremite).normalized;
        int nbCubesToCreate = (int)(center - extremite).magnitude;
        float intervalle = timeIntersection / (float)nbCubesToCreate;
        Vector3 currentPosition = extremite + direction;

        GameObject soundHolder = new GameObject();
        soundHolder.transform.position = currentPosition;
        gm.soundManager.PlayIntersectionEventClip(currentPosition, timeIntersection, soundHolder.transform);
        for(int i = 0; i < nbCubesToCreate; i++) {
            yield return new WaitForSeconds(intervalle);
            if (gm.map.CubeFarEnoughtFromLumieres(currentPosition)) {
                Cube cube = gm.map.AddCube(currentPosition, cubeTypeWhileEffect);
                if (cube != null) {
                    cube.RegisterCubeToColorSources();
                    cube.StartDissolveEffect(cubeWhileEffectDissolveTime, gm.postProcessManager.dissolveInGamePlayerProximityCoef);
                }
            }
            soundHolder.transform.position = currentPosition;
            currentPosition += direction;
        }
        foreach(Transform child in soundHolder.transform) {
            child.parent = soundHolder.transform.parent;
        }
        Destroy(soundHolder);
    }

    protected void UndoPrevisualisationOfIntersections(Cube extremite, Texture oldTexture) {
        if (extremite != null) {
            extremite.SetTexture(oldTexture);
            cubesAlreadyUsedInEvents.Remove(extremite);
        }
    }

    protected IEnumerator SwapCubeTypesForExtremite(Vector3 center, Vector3 extremite) {
        Vector3 direction = (center - extremite).normalized;
        int nbCubesCreated = (int)(center - extremite).magnitude;
        float intervalle = timeBeforeSwapCubeTypeAgain / (float)nbCubesCreated;
        Vector3 currentPosition = extremite + direction;

        GameObject soundHolder = new GameObject();
        soundHolder.transform.position = currentPosition;
        gm.soundManager.PlayIntersectionEventClip(currentPosition, timeBeforeSwapCubeTypeAgain, soundHolder.transform);
        for (int i = 0; i < nbCubesCreated; i++) {
            yield return new WaitForSeconds(intervalle);
            Cube cube = gm.map.GetCubeAt(currentPosition);
            if (cube != null) {
                cube = gm.map.SwapCubeType(cube, cubeTypeAfterEffect);
                cube.StartDissolveEffect(cubeAfterEffectDissolveTime, gm.postProcessManager.dissolveInGamePlayerProximityCoef);
            }
            soundHolder.transform.position = currentPosition;
            currentPosition += direction;
        }
        foreach(Transform child in soundHolder.transform) {
            child.parent = soundHolder.transform.parent;
        }
        Destroy(soundHolder);
    }

    protected List<Vector3> GetExtremites(Vector3 center) {
        List<Vector3> extremites = new List<Vector3>();
        foreach(Vector3 direction in MapManager.GetAllDirections()) {
            Cube extremite = gm.map.GetClosestCubeInDirection(center, direction);
            if(extremite != null)
                extremites.Add(extremite.transform.position);
        }
        return extremites;
    }

    public override void StopEvent() {
        foreach(Coroutine coroutine in coroutines) {
            if(coroutine != null)
                StopCoroutine(coroutine);
        }
        coroutines.Clear();
    }
}
