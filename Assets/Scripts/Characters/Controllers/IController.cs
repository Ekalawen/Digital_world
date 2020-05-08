using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IController : MonoBehaviour {

    public float vitesse = 5.0f;
    public float tempsInactifDebutJeu = 0.0f; // Le temps pendant lequel la sonde n'agira pas en début de partie

    protected GameManager gm;
	protected CharacterController controller;

    public virtual void Start() {
        gm = GameManager.Instance;
		controller = this.GetComponent<CharacterController> ();
        if (controller == null)
            Debug.LogError("Il est nécessaire d'avoir un CharacterController avec un " + name + " !");
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

    protected Vector3 Move(Vector3 target, bool useCustomVitesse = false, float customVitesse = 0.0f) {
        float vitesseToUse = useCustomVitesse ? customVitesse : vitesse;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 finalMouvement = direction * vitesseToUse * Time.deltaTime;

        // Si c'est trop long, on ajuste
        if (Vector3.Magnitude(finalMouvement) > Vector3.Distance(transform.position, target)) {
            finalMouvement = target - transform.position;
        }

        controller.Move(finalMouvement);

        return finalMouvement;
    }
}
