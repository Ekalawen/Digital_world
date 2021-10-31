using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IController : MonoBehaviour {

    public float vitesse = 5.0f;
    public float tempsInactifDebutJeu = 3.0f; // Le temps pendant lequel la sonde n'agira pas en début de partie

    protected GameManager gm;
	protected CharacterController controller;
	protected new Rigidbody rigidbody;

    public virtual void Start() {
        gm = GameManager.Instance;
		controller = this.GetComponent<CharacterController> ();
        rigidbody = GetComponent<Rigidbody>();
        if (controller == null && rigidbody == null)
            Debug.LogError("Il est nécessaire d'avoir un CharacterController OU un rigidbody avec un " + name + " !");
    }

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(gm.IsTimeFreezed())
            return;

        // Si c'est encore trop tôt dans le jeu pour agir, on ne fait rien
        if (Time.timeSinceLevelLoad < tempsInactifDebutJeu)
            return;

        UpdateSpecific();
	}

    protected abstract void UpdateSpecific();

    public abstract bool IsInactive();
    public abstract bool IsMoving();

    protected virtual Vector3 MoveToTarget(Vector3 target, bool useCustomVitesse = false, float customVitesse = 0.0f) {
        float vitesseToUse = useCustomVitesse ? customVitesse : vitesse;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 finalMouvement = direction * vitesseToUse * Time.deltaTime;

        if (Vector3.Magnitude(finalMouvement) > Vector3.Distance(transform.position, target)) {
            finalMouvement = target - transform.position;
        }

        Move(finalMouvement);

        return finalMouvement;
    }

    protected Vector3 MoveWithMove(Vector3 move) {
        move *= vitesse * Time.deltaTime;
        Move(move);
        return move;
    }

    protected Vector3 Move(Vector3 mouvement) {
        if (controller != null) {
            if (controller.enabled) {
                controller.Move(mouvement);
            }
        } else {
            RaycastHit hit;
            if (rigidbody.SweepTest(mouvement.normalized, out hit, mouvement.magnitude)) {
                transform.position += mouvement.normalized * hit.distance;
                //transform.Translate(mouvement.normalized * hit.distance);
            } else {
                transform.position += mouvement;
                //transform.Translate(mouvement);
            }
        }
        return mouvement;
    }
}
