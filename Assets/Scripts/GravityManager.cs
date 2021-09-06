using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour {

    public enum Direction { HAUT, BAS, GAUCHE, DROITE, AVANT, ARRIERE };

    [Header("Gravity Initial")]
    public Direction initialGravityDirection = Direction.BAS;
    public float initialGravityIntensity = 5; // 5 est la gravity par défautl !

    [Header("Gravity Transitions")]
    public float dureeGravityTransition = 0.4f; // Le temps pendant lequel on fait tourner la caméra quand on change de gravité !
    public AnimationCurve gravityTransitionCurve;

    [Header("ScreenShake on SetGravity")]
    public float screenShakeMagnitude = 10;
    public float screenShakeRoughness = 5;
    public float screenShakeFadeInPercentage = 0.5f;

    [HideInInspector]
    public Direction gravityDirection;
    [HideInInspector]
    public float gravityIntensity;
    protected GameManager gm;
    protected Fluctuator playerCameraOffsetFluctuator;

    protected Timer cooldownChangeGravity;

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
        cooldownChangeGravity = new Timer(dureeGravityTransition, setOver: true);
        gravityDirection = initialGravityDirection;
        gravityIntensity = initialGravityIntensity;
        playerCameraOffsetFluctuator = new Fluctuator(this, gm.player.GetCameraShakerHeight, gm.player.SetCameraShakerHeight);
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
        if(!cooldownChangeGravity.IsOver()) {
            return;
        }
        cooldownChangeGravity.Reset();

        float angle = Vector3.Angle(DirToVec(gravityDirection), DirToVec(newGravityDirection));
        Vector3 axe = Vector3.Cross(DirToVec(gravityDirection), DirToVec(newGravityDirection));
        if (axe.magnitude == 0) {
            axe = Vector3.ProjectOnPlane(gm.player.camera.transform.forward, Up());
        }

        gravityDirection = newGravityDirection;
        gravityIntensity = newGravityIntensity;

        gm.player.bSetUpRotation = false;
        //ScreenShakeOnGravityChange();
        gm.soundManager.PlayGravityChangeClip();
        StartCoroutine(RotateCamera(axe, angle, gravityTransitionCurve));
        playerCameraOffsetFluctuator.GoTo(gm.player.GetCameraShakerInitialHeight(), dureeGravityTransition);
    }

    public void SetGravityZeroSwap() {
        if (gravityIntensity != 0.0f) {
            SetGravity(gravityDirection, 0.0f);
            gm.console.CapturePouvoirGiverVoler();
        } else {
            SetGravity(gravityDirection, initialGravityIntensity);
        }
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

    protected IEnumerator RotateCamera(Vector3 axe, float angle, AnimationCurve curve = null) {
        Timer t = new Timer(dureeGravityTransition);
        Camera camera = gm.player.camera;
        float currentAvancement = curve != null ? curve.Evaluate(0) : 0;
        float totalPortion = 0.0f;
        while (!t.IsOver()) {
            float portion = curve != null ? curve.Evaluate(t.GetAvancement()) - curve.Evaluate(currentAvancement) : t.GetAvancement() - currentAvancement;
            totalPortion += portion;
            currentAvancement = t.GetAvancement();
            float value = angle * portion;
            camera.transform.RotateAround(camera.transform.position, axe, value);
            yield return null;
        }
        float finalValue = angle * (curve != null ? curve.Evaluate(1.0f - totalPortion) : 1.0f - totalPortion);
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

    public static Direction VecToDir(Vector3 vec) {
        Dictionary<Vector3, Direction> map = new Dictionary<Vector3, Direction>();
        map.Add(Vector3.right, Direction.DROITE);
        map.Add(Vector3.left, Direction.GAUCHE);
        map.Add(Vector3.up, Direction.HAUT);
        map.Add(Vector3.down, Direction.BAS);
        map.Add(Vector3.forward, Direction.AVANT);
        map.Add(Vector3.back, Direction.ARRIERE);
        if (map.ContainsKey(vec)) {
            return map[vec];
        } else {
            throw new Exception("GravityManager.VecToDir prend uniquement une direction orthogonale normalisé en entrée ! :p");
        }
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

    public int GetHeightIndice() {
        switch(gravityDirection) {
            case Direction.HAUT:
                return 1;
            case Direction.BAS:
                return 1;
            case Direction.GAUCHE:
                return 0;
            case Direction.DROITE:
                return 0;
            case Direction.AVANT:
                return 2;
            case Direction.ARRIERE:
                return 2;
        }
        throw new System.Exception("Erreur dans GetHeightIndice()");
    }

    public float GetHeightSign() {
        // C'est "inversé" car on cherche le sens du "haut", pas de la gravité !
        switch(gravityDirection) {
            case Direction.HAUT:
                return -1;
            case Direction.BAS:
                return 1;
            case Direction.GAUCHE:
                return 1;
            case Direction.DROITE:
                return -1;
            case Direction.AVANT:
                return -1;
            case Direction.ARRIERE:
                return 1;
        }
        throw new System.Exception("Erreur dans GetHeightSign()");
    }

    public float GetHeightInMap(Vector3 pos) {
        float sign = GetHeightSign();
        float mapHeight = sign < 0 ? - GetHeightAbsolute(gm.map.tailleMap) : 0;
        float posHeight = GetHeightAbsolute(pos);
        return mapHeight + posHeight;
        //switch(gravityDirection) {
        //    case Direction.HAUT:
        //        return gm.map.tailleMap.y - pos.y;
        //    case Direction.BAS:
        //        return pos.y;
        //    case Direction.GAUCHE:
        //        return pos.x;
        //    case Direction.DROITE:
        //        return gm.map.tailleMap.x - pos.x;
        //    case Direction.AVANT:
        //        return gm.map.tailleMap.z - pos.z;
        //    case Direction.ARRIERE:
        //        return pos.z;
        //}
        //throw new System.Exception("Erreur dans GetHigh()");
    }

    public float GetHeightAbsolute(Vector3 pos) {
        return pos[GetHeightIndice()] * GetHeightSign();
    }

    public bool HasGravity() {
        return gravityIntensity > 0.0f;
    }
}
