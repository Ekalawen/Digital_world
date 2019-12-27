using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerOverride : CharacterController {

    public bool newIsGrounded;

    void OnCollisionStay(Collision collision) {
        Debug.Log("CharacterControllerOverride.OnCollissionStay()");
        ContactPoint[] cps = collision.contacts;
        Vector3 up = GameManager.Instance.gravityManager.Up();
        foreach(ContactPoint cp in cps) {
            Vector3 n = cp.normal;
            float angle = Vector3.Angle(n, up);
            if (Mathf.Abs(angle) <= 45f) {
                newIsGrounded = true;
                return;
            }
        }
        newIsGrounded = false;
    }

}
