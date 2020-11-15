using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour {

    public enum Direction { HAUT, BAS, GAUCHE, DROITE, AVANT, ARRIERE };

    [Header("Gravity initial")]
    public Direction initialGravityDirection = Direction.BAS;
    public float initialGravityIntensity = 5; // 5 est la gravity par défautl !
    public float dureeGravityTransition = 0.4f; // Le temps pendant lequel on fait tourner la caméra quand on change de gravité !

    [Header("ScreenShake on SetGravity")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 5;
    public float screenShakeFadeInPercentage = 0.5f;

    [HideInInspector]
    public Direction gravityDirection;
    [HideInInspector]
    public float gravityIntensity;
    protected GameManager gm;

    protected Timer timer;

    public static Direction GetRandomDirection() {
        System.Array enumValues = System.Enum.GetValues(typeof(Direction));
        return (Direction)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
    }

    public static Direction GetRandomDirection(Direction differentFrom) {
        Direction dir = GetRandomDirection();
        while(dir == differentFrom)
            dir = GetRandomDirection();
        return dir;
    }

    public void Initialize() {
        gm = GameManager.Instance;
        timer = new Timer(5);
        gravityDirection = initialGravityDirection;
        gravityIntensity = initialGravityIntensity;
    }

    public Vector3 ApplyGravity(Vector3 initialMovement) {
        return initialMovement + DirToVec(gravityDirection) * gravityIntensity;
    }

    public Vector3 CounterGravity(Vector3 initialMovement) {
        return initialMovement - DirToVec(gravityDirection) * gravityIntensity;
    }

    public Vector3 MoveOppositeDirectionOfGravity(Vector3 initialMovement, float intensityMovement) {
        return initialMovement - DirToVec(gravityDirection) * intensityMovement;
    }

    public void SetGravity(Direction newGravityDirection, float newGravityIntensity) {
        float angle = Vector3.Angle(DirToVec(gravityDirection), DirToVec(newGravityDirection));
        Vector3 axe = Vector3.Cross(DirToVec(gravityDirection), DirToVec(newGravityDirection));
        if (axe.magnitude == 0) {
            axe = gm.player.camera.transform.forward;
        }

        gravityDirection = newGravityDirection;
        gravityIntensity = newGravityIntensity;

        gm.player.bSetUpRotation = false;
        ScreenShakeOnGravityChange();
        StartCoroutine(RotateCamera(axe, angle));
    }

    protected void ScreenShakeOnGravityChange() {
        float timeShake = dureeGravityTransition;
        float timeFadeIn = timeShake * screenShakeFadeInPercentage;
        float timeFadeOut = timeShake * (1 - screenShakeFadeInPercentage);
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, timeFadeIn, timeFadeOut);
    }

    public void LookAt(Vector3 newDirection) {
        Vector3 currentDirection = gm.player.camera.transform.forward;
        float angle = Vector3.Angle(currentDirection, newDirection);
        Vector3 axe = Vector3.Cross(currentDirection, newDirection);
        if (axe.magnitude == 0) {
            return;
        }

        StartCoroutine(RotateCamera(axe, angle));
    }

    protected IEnumerator RotateCamera(Vector3 axe, float angle) {
        Timer t = new Timer(dureeGravityTransition);
        Camera camera = gm.player.camera;
        float currentAvancement = 0;
        float totalPortion = 0.0f;
        while (!t.IsOver()) {
            float portion = t.GetAvancement() - currentAvancement;
            totalPortion += portion;
            currentAvancement = t.GetAvancement();
            float value = angle * portion;
            camera.transform.RotateAround(camera.transform.position, axe, value);
            yield return null;
        }
        float finalValue = angle * (1.0f - totalPortion);
        camera.transform.RotateAround(camera.transform.position, axe, finalValue);
        gm.player.bSetUpRotation = true;
    }

    public Vector3 Up() {
        return -Down();
    }
    public Vector3 Down() {
        return DirToVec(gravityDirection);
    }
    public Vector3 Right() {
        Vector3 up = Up();
        float dot = Vector3.Dot(up, Vector3.right);
        if(Mathf.Abs(dot) != 1) {
            return Vector3.Cross(up, Vector3.right);
        } else {
            return Vector3.Cross(up, Vector3.forward);
        }
    }
    public Vector3 Left() {
        return -Right();
    }
    public Vector3 Forward() {
        return Vector3.Cross(Right(), Up());
    }
    public Vector3 Backward() {
        return -Forward();
    }

    public static Vector3 DirToVec(Direction dir) {
        List<Vector3> list = new List<Vector3>();
        list.Add(Vector3.up);
        list.Add(Vector3.down);
        list.Add(Vector3.left);
        list.Add(Vector3.right);
        list.Add(Vector3.forward);
        list.Add(Vector3.back);
        return list[(int)dir];
    }

    public static Direction OppositeDir(Direction dir) {
        switch(dir) {
            case Direction.HAUT:
                return Direction.BAS;
            case Direction.BAS:
                return Direction.HAUT;
            case Direction.GAUCHE:
                return Direction.DROITE;
            case Direction.DROITE:
                return Direction.GAUCHE;
            case Direction.AVANT:
                return Direction.ARRIERE;
            case Direction.ARRIERE:
                return Direction.AVANT;
        }
        throw new System.Exception("Erreur dans OppositeDir()");
    }

    public float GetHigh(Vector3 pos) {
        switch(gravityDirection) {
            case Direction.HAUT:
                return gm.map.tailleMap.y - pos.y;
            case Direction.BAS:
                return pos.y;
            case Direction.GAUCHE:
                return pos.x;
            case Direction.DROITE:
                return gm.map.tailleMap.x - pos.x;
            case Direction.AVANT:
                return pos.z;
            case Direction.ARRIERE:
                return gm.map.tailleMap.z - pos.z;
        }
        throw new System.Exception("Erreur dans GetHigh()");
    }

    public bool HasGravity() {
        return gravityIntensity > 0.0f;
    }
}
