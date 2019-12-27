using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : ZoneCubique {

    public GravityManager.Direction direction;
    public bool bUseCurrentGravity = false;
    public bool bUseRandomDirection = false;
    public float intensity = 5.0f;

    protected GameManager gm;
    protected GravityManager.Direction oldDirection;
    protected float oldIntensity;

    public void Start() {
        gm = GameManager.Instance;
        oldDirection = gm.gravityManager.initialGravityDirection;
        oldIntensity = gm.gravityManager.initialGravityIntensity;
        if(bUseRandomDirection) {
            direction = GravityManager.GetRandomDirection(gm.gravityManager.initialGravityDirection);
        }
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            // Si le joueur veut renter du coté du "down" on le repousse !
            Player player = other.gameObject.GetComponent<Player>();
            if(IsBelow(player) && intensity > 0.0f) {
                Vector3 dirPoussee = GravityManager.DirToVec(direction);
                player.RemoveAllPoussees();
                player.AddPoussee(new Poussee(dirPoussee, 0.2f, 3.0f));
                gm.soundManager.PlayHitClip(player.audioSource, priority: true);
                Debug.Log("IS BELOW !!");
                return;
            }

            oldDirection = gm.gravityManager.gravityDirection;
            oldIntensity = gm.gravityManager.gravityIntensity;
            GravityManager.Direction newDir = bUseCurrentGravity ? oldDirection : direction;
            gm.gravityManager.SetGravity(newDir, intensity);
            Debug.Log("oldDirection = " + oldDirection);
            Debug.Log("newDir = " + newDir);
        }
    }

    protected override void OnExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            Debug.Log("ExitDir = " + oldDirection);
            gm.gravityManager.SetGravity(oldDirection, oldIntensity);
        }
    }

    protected bool IsBelow(Player player) {
        Vector3 pos = player.transform.position;
        Vector3 axe = -GravityManager.DirToVec(direction);
        Vector3 projectPlayer = Vector3.Project(pos, axe);
        float posPlayer = projectPlayer.magnitude * Mathf.Sign(Vector3.Dot(projectPlayer, axe));
        Vector3 projectBox = Vector3.Project(transform.position, axe);
        float posBox = projectBox.magnitude * Mathf.Sign(Vector3.Dot(projectBox, axe));
        posBox -= Vector3.Project(transform.localScale / 2.3f, axe).magnitude;
        return posPlayer < posBox;
    }
}
