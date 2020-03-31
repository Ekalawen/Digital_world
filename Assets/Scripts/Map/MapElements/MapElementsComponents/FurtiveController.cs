using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurtiveController : MonoBehaviour {

    public enum EtatFurtif { HIDDEN, VISIBLE };

	public float hiddenVitesse; // vitesse de déplacement de base
	public float visibleBaseVitesse; // vitesse de déplacement de base

    protected GameManager gm;
	protected Player player;
    protected EtatFurtif etat;
    protected Vector3 posObjectifHidden;
    protected Vector3 posObjectifVisible;
	public CharacterController controller;
	//[HideInInspector]
 //   public List<Poussee> poussees;

    public void Start() {
        gm = GameManager.Instance;
        player = gm.player;
        posObjectifHidden = transform.position;
        posObjectifVisible = transform.position;
		controller = this.GetComponent<CharacterController> ();
        if (controller == null)
            Debug.LogError("Il est nécessaire d'avoir un CharacterController avec un FurtiveController !");
        //poussees = new List<Poussee>();
    }

	public virtual void Update () {
        // Si le temps est freeze, on ne fait rien
        if(gm.IsTimeFreezed()) {
            return;
        }

        UpdateSpecific();

        //ApplyPoussees();
	}

    protected void UpdateSpecific() {
        UpdateEtat();

        // Tant que le joueur ne nous voit pas, on erre
        // Si il nous voit, on fuit de son axe de vision aussi vite que possible !
        switch (etat) {
            case EtatFurtif.HIDDEN:
                // On cherche un point où aller
                if(Vector3.Distance(posObjectifHidden, transform.position) <= 0.1f
                || IsInPlayerSight(posObjectifHidden, player)) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifHidden = GetOtherObjectifHidden();
                }

                // Puis on y va
                Vector3 direction = (posObjectifHidden - transform.position).normalized;
                Vector3 finalMouvement = direction * hiddenVitesse * Time.deltaTime;
                controller.Move(finalMouvement);
                break;

            case EtatFurtif.VISIBLE:
                // On cherche à aller le plus loin possible et le plus vite possible hors du champs de vision du joueur
                float angle = Vector3.Angle(player.camera.transform.forward, (transform.position - player.transform.position));
                float maxAngle = 50;
                angle = Mathf.Min(maxAngle, angle);
                float coefRapprochementAxe = 1.0f / Mathf.Max(angle / maxAngle, 0.2f);

                if(Vector3.Distance(posObjectifVisible, transform.position) <= 0.1f
                || IsInPlayerSight(posObjectifVisible, player)) {
                    // On est arrivé, il faut trouver un autre point !
                    posObjectifVisible = GetOtherObjectifVisible();
                }

                // Puis on y va
                Vector3 directionVisible = (posObjectifVisible - transform.position).normalized;
                Vector3 finalMouvementVisible = directionVisible * visibleBaseVitesse * coefRapprochementAxe * Time.deltaTime;
                controller.Move(finalMouvementVisible);
                break;
        }
    }

    protected EtatFurtif UpdateEtat() {
        if(IsInPlayerSight(transform.position, player)) {
            if (etat != EtatFurtif.VISIBLE) {
                //    gm.soundManager.PlayGetLumiereClip(transform.position);
                posObjectifVisible = transform.position;
            }
            etat = EtatFurtif.VISIBLE;
            return EtatFurtif.VISIBLE;
        } else {
            if (etat != EtatFurtif.HIDDEN) {
                //    gm.soundManager.PlayGetLumiereClip(transform.position);
                posObjectifHidden = transform.position;
            }
            etat = EtatFurtif.HIDDEN;
            return EtatFurtif.HIDDEN;
        }
    }

    public static bool IsInPlayerSight(Vector3 pos, Player player) {
        Camera camera = player.camera;
        Vector3 playerToPos = pos - player.transform.position;

        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        for (int i = 0; i < 4; i++) {
            Vector3 directionCorner1 = camera.transform.TransformVector(frustumCorners[i]).normalized;
            Vector3 directionCorner2 = camera.transform.TransformVector(frustumCorners[(i + 1) % 4]).normalized;
            Vector3 cross = Vector3.Cross(directionCorner1, directionCorner2).normalized;
            //Debug.DrawRay(camera.transform.position, directionCorner1, Color.blue);

            if(Vector3.Dot(cross, playerToPos) >= 0) {
                return false;
            }
        }

        // Donc on est dans le frustrum, maintenant on vérifie que l'on est vraiment visible et pas caché par un obstacle !
        RaycastHit hit;
        Ray ray = new Ray (pos, player.transform.position - pos);
        return Physics.Raycast(ray, out hit, player.camera.farClipPlane) && hit.collider.name == "Joueur";
    }

    protected Vector3 GetOtherObjectifHidden() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();
        while(allEmptyLocations.Count > 1) {
            int ind = Random.Range(0, allEmptyLocations.Count);
            if (!IsInPlayerSight(allEmptyLocations[ind], player)) {
                RaycastHit hit;
                Vector3 direction = allEmptyLocations[ind] - transform.position;
                Ray ray = new Ray (transform.position, direction);
                if(!Physics.Raycast(ray, out hit, direction.magnitude))
                    return allEmptyLocations[ind];
            }
            allEmptyLocations.RemoveAt(ind);
        }
        return allEmptyLocations[0];
    }

    protected Vector3 GetOtherObjectifVisible() {
        List<Vector3> allEmptyLocations = gm.map.GetAllEmptyPositions();

        Vector3 bestPos = transform.position;
        float minDist = float.PositiveInfinity;

        foreach(Vector3 pos in allEmptyLocations) {
            Vector3 direction = pos - transform.position;
            float dist = direction.magnitude;
            if (dist < minDist) {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, direction);
                if (!IsInPlayerSight(pos, player) && !Physics.Raycast(ray, out hit, dist)) {
                    minDist = dist;
                    bestPos = pos;
                }
            }
        }

        if(minDist < float.PositiveInfinity) {
            return bestPos;
        }

        float maxDist = Vector3.Distance(allEmptyLocations[0], player.transform.position);
        bestPos = allEmptyLocations[0];
        for(int i = 1; i < allEmptyLocations.Count; i++) {
            float dist = Vector3.Distance(allEmptyLocations[i], player.transform.position);
            if(dist > maxDist) {
                maxDist = dist;
                bestPos = allEmptyLocations[i];
            }
        }
        return bestPos;
    }
}
