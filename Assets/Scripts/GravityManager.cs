﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour {

    public enum Direction { HAUT, BAS, GAUCHE, DROITE, AVANT, ARRIERE };

    public Direction initialGravityDirection = Direction.BAS;
    public float initialGravityIntensity = 5; // 5 est la gravity par défautl !
    public float dureeGravityTransition = 0.4f; // Le temps pendant lequel on fait tourner la caméra quand on change de gravité !

    [HideInInspector]
    public Direction gravityDirection;
    [HideInInspector]
    public float gravityIntensity;
    protected GameManager gm;

    protected Timer timer;

    public void Update() {
        //if (timer.IsOver()) {
        //    SetGravity(GetRandomDirection(), gravityIntensity);
        //    timer.Reset();
        //}
    }

    public static Direction GetRandomDirection() {
        System.Array enumValues = System.Enum.GetValues(typeof(Direction));
        return (Direction)enumValues.GetValue(Random.Range(0, enumValues.Length));
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
        SetGravity(initialGravityDirection, initialGravityIntensity);
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
        if (axe.magnitude == 0)
            axe = gm.player.camera.transform.forward;

        gravityDirection = newGravityDirection;
        gravityIntensity = newGravityIntensity;

        gm.player.bSetUpRotation = false;
        StartCoroutine(RotateCamera(axe, angle));
    }

    protected IEnumerator RotateCamera(Vector3 axe, float angle) {
        Timer t = new Timer(dureeGravityTransition);
        Camera camera = gm.player.camera;
        float currentAvancement = 0;
        while (!t.IsOver()) {
            float portion = t.GetAvancement() - currentAvancement;
            currentAvancement = t.GetAvancement();
            float value = angle * portion;
            camera.transform.RotateAround(camera.transform.position, axe, value);
            yield return null;
        }
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
                return gm.map.tailleMap - pos.y;
            case Direction.BAS:
                return pos.y;
            // FONCTION A VERIFIER A PARTIR D'ICI !!
            case Direction.GAUCHE:
                return pos.x;
            case Direction.DROITE:
                return gm.map.tailleMap - pos.x;
            case Direction.AVANT:
                return pos.z;
            case Direction.ARRIERE:
                return gm.map.tailleMap - pos.z;
        }
        throw new System.Exception("Erreur dans GetHigh()");
    }

    public bool HasGravity() {
        return gravityIntensity > 0.0f;
    }
}
