using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PUBLIQUES
    //////////////////////////////////////////////////////////////////////////////////////

    public float frequenceMouvement; // La fréquence à laquelle le panel décide de bouger !
    public int nbSemblables; // Le nombre de voisins semblables dont on veut s'approcher

    //////////////////////////////////////////////////////////////////////////////////////
    // ATTRIBUTS PRIVEES
    //////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
    public MenuBackgroundScript menu; // un pointeur vers le menu
    [HideInInspector]
    public int x; // Sa position dans l'écran, en terme de position par cube
    [HideInInspector]
    public int y;
    [HideInInspector]
    public List<PanelScript> semblables; // Les Panels semblables dont il cherchera à se rapprocher
    [HideInInspector]
    public bool isMoving; // Permet de savoir que notre cube est en train de bouger, donc il ne peut pas être bougé à son tour
    [HideInInspector]
    public bool fixe; // Permet de savoir si notre cube va encore bouger ou pas ! =)

    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Initialisation
        isMoving = false;
        fixe = false;
    }

    void Update () {
        // À chaque frame on a une petite chance de bouger
        if(Random.Range(0f, 1f) < frequenceMouvement)
        {
            tryStartMove();
        }
	}

    void tryStartMove()
    {
        // Si on est déjà en place, alors on ne bouge plus =)
        if(fixe)
            return;

        // On bouge
        isMoving = true;

        // On récupère le barycentre de nos semblables
        int xbar = 0, ybar = 0, n = 0;
        foreach(PanelScript semblable in semblables)
        {
            if(!semblable.fixe) {
                xbar += semblable.x;
                ybar += semblable.y;
                n ++;
            }
        }
        xbar = (int)xbar / n;
        ybar = (int)ybar / n;

        // On récupère le mouvement pour nous en rapprocher
        int dx = xbar - x;
        int dy = ybar - y;
        int xnext, ynext;
        if(Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            xnext = x + (int)Mathf.Sign(dx);
            ynext = y;
        } else
        {
            xnext = x;
            ynext = y + (int)Mathf.Sign(dy);
        }

        // On appel la fonction qui fait bouger
        bouger(xnext, ynext);

        // On a finit de bouger
        isMoving = false;
    }

    bool bouger(int xdest, int ydest) {
        // Si il n'y a personne, on se déplace !
        if(menu.positions[xdest, ydest] == 0)
        {
            menu.positions[xdest, ydest] = 1;
            menu.positions[x, y] = 0;
            x = xdest;
            y = ydest;
			RectTransform r = this.gameObject.GetComponent<RectTransform>();
			r.localPosition = new Vector3(x * menu.size - menu.rect.rect.width / 2 + menu.size / 2, y * menu.size - menu.rect.rect.height / 2 + menu.size / 2, 0);
            fixerPosition();
            return true; // On a réussi à bouger ! :D
        } else {
            // On force quelqu'un à bouger ! =)
            PanelScript victime = menu.getPanelXY(xdest, ydest);
            if(!victime.fixe && !victime.isMoving && victime.forceMove(x, y)) {
                // Si elle a réussi à bouger, alors on peut bouger à notre tour ! =)
                menu.positions[xdest, ydest] = 1;
                menu.positions[x, y] = 0;
                x = xdest;
                y = ydest;
                RectTransform r = this.gameObject.GetComponent<RectTransform>();
                r.localPosition = new Vector3(x * menu.size - menu.rect.rect.width / 2 + menu.size / 2, y * menu.size - menu.rect.rect.height / 2 + menu.size / 2, 0);
                fixerPosition();
                return true; // On a réussi à bouger ! :D
            } else {
                // Sinon on renvoi que c'est un échec :/
                return false;
            }
        }
    }

    // Si un cube à besoin de passer mais qu'un autre cube le block, on va faire bouger celui-ci ! :D
    // Mais dans une direction aléatoire cette fois-ci =)
    bool forceMove(int xsource, int ysource) {
        // On bouge
        isMoving = true;

        // On récupère toutes les destinations possibles
        List<int> destinationsPossiblesX = new List<int>();
        List<int> destinationsPossiblesY = new List<int>();
        if(!(x-1 == xsource && y == ysource) && menu.isIn(x-1, y)) {
            destinationsPossiblesX.Add(x-1);
            destinationsPossiblesY.Add(y);
        }
        if(!(x+1 == xsource && y == ysource) && menu.isIn(x+1, y)) {
            destinationsPossiblesX.Add(x+1);
            destinationsPossiblesY.Add(y);
        }
        if(!(x == xsource && y-1 == ysource) && menu.isIn(x, y-1)) {
            destinationsPossiblesX.Add(x);
            destinationsPossiblesY.Add(y-1);
        }
        if(!(x == xsource && y+1 == ysource) && menu.isIn(x, y+1)) {
            destinationsPossiblesX.Add(x);
            destinationsPossiblesY.Add(y+1);
        }

        // Tant qu'il nous reste des destinations possibles on tente de bouger !
        bool aBouge = false;
        while(destinationsPossiblesX.Count > 0) {
            // On en choisit une au hasard
            int ind = Random.Range(0, destinationsPossiblesY.Count);
            int xdest = destinationsPossiblesX[ind];
            int ydest = destinationsPossiblesY[ind];

            // Puis on bouge
            if(bouger(xdest, ydest)) {
                // Si on y arrive c'est cool on le dit
                aBouge = true;
                break;
            } else {
                // Sinon on tente un autre
                destinationsPossiblesX.RemoveAt(ind);
                destinationsPossiblesY.RemoveAt(ind);
            }
        }

        // On a finit de bouger
        isMoving = false;
        return aBouge;
    }

    public void tryAddSemblable(PanelScript autre)
    {
        Color notreCouleur = gameObject.GetComponent<Image>().color;
        float distance = distanceCouleur(notreCouleur, autre.gameObject.GetComponent<Image>().color);

        if(semblables.Count < nbSemblables) {
            semblables.Add(autre);
        } else {
            for (int i = 0; i < nbSemblables; i++) {
                float distanceActuelle = distanceCouleur(notreCouleur, semblables[i].gameObject.GetComponent<Image>().color);
                if (distance < distanceActuelle)
                {
                    semblables[i] = autre;
                    break;
                }
            }
        }
    }

    // La distance colorimétrique entre deux couleurs, en norme 2
    public static float distanceCouleur(Color c1, Color c2)
    {
        float res = 0;
        res += Mathf.Pow(c1.r - c2.r, 2);
        res += Mathf.Pow(c1.g - c2.g, 2);
        res += Mathf.Pow(c1.b - c2.b, 2);
        res = Mathf.Pow(res, 1 / 2);
        return res;
    }

    // Si on est suffisamment proche du barycentre, on arrête de bouger advitam-eternam ! <3
    void fixerPosition() {
        // On récupère le barycentre de nos semblables
        int xbar = 0, ybar = 0, n = 0;
        foreach(PanelScript semblable in semblables)
        {
            if(!semblable.fixe) {
                xbar += semblable.x;
                ybar += semblable.y;
                n ++;
            }
        }
        xbar = (int)xbar / n;
        ybar = (int)ybar / n;

        if(Mathf.Abs(x - xbar) <= 1 && Mathf.Abs(y -ybar) <= 1) {
            fixe = true;
        }
    }
}
