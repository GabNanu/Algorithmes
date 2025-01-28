using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Apple;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class NavAgent : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    Personnage perso;

    [SerializeField]�public List<NavPoint> chemin; // le chemin de point trouv� par l'algorithme

    private void Start()
    {
        perso = GetComponent<Personnage>();
    }

    public void CommencerAlgo() // est appel� par personnage lorsqu'une destination a �t� s�lectionn�e
    {
        // liste des fonctions pouvant �tre appel� en ordre selon le dropdown
        List<Action> fonctionsAlgos = new List<Action>() { DepthFirstSearch, BreadthFirstSearch, Dijsktra };

        perso.TrouverPointD�part(); // on trouve le point de d�part 

        fonctionsAlgos[dropdown.value](); // on fait appel � la fonction repr�sentant l'algorithme s�lectionn� dans le dropdown
    }

    private bool V�rifierEstDestination(NavPoint point)
    {
        point.SetVisit�(true); // maintenant qu'on a v�rifi� si le point est la destination, ce point a �t� visit�
        return point.estDestination;
    }

    void DepthFirstSearch()
    {
        NavPoint pointActuel = perso.TrouverPointD�part();
        Stack<NavPoint> points = new Stack<NavPoint>();
        points.Push(pointActuel);

        while (!V�rifierEstDestination(pointActuel))
        {
            // puisque le point actuel a �t� visit�, on peut le retirer de la stack
            // et le nouveau point actuel sera le prochain dans le stack
            if (points.Count != 0) // on regarde si la stack est vide avant d'acc�der au prochain point
                pointActuel = points.Pop();
            else
                return; // si la stack est vide, alors l'algorithme se termine sans avoir trouv� sa destination puisqu'il a fait le tour de tous les points accessibles

            foreach (NavPoint voisin in pointActuel.voisins)
                if (!voisin.estVisit�) // on v�rifie si le point a d�j� �t� visit� ou si le point n'est pas d�ja contenu dans la stack
                {
                    voisin.SetVisit�(true);
                    points.Push(voisin); // le nouveau point actuel est rajout� au top de la liste et sera visit� � la prochaine it�ration
                    voisin.Parent = pointActuel;
                }
        }
        TrouverChemin();
        perso.seD�place = true;
    }

    void BreadthFirstSearch()
    {
        NavPoint pointActuel = perso.TrouverPointD�part();
        Queue<NavPoint> points = new Queue<NavPoint>(); // la file des points potentiels, soit les points qui seront visit�s. � la fin de l'algorithme, points devrait contenir la liste des points choisis
        points.Enqueue(pointActuel); // le premier point � visiter est bien �videmment le point de d�part

        while (!V�rifierEstDestination(pointActuel)) // on arr�te l'algorithme lorsque le point a atteint sa destination
        {
            foreach (NavPoint voisin in pointActuel.voisins) // on ajoute tous les voisins non-visit�s � la file des voisins � visiter et on v�rifie si le voisin n'a pas d�j� �t� ajout� a la liste
                if(!voisin.estVisit�) 
                {
                    voisin.SetVisit�(true);
                    points.Enqueue(voisin);
                    voisin.Parent = pointActuel;
                }

            points.Dequeue(); // on retire le point actuel

            if (points.Count != 0) // on v�rifie si la queue est vide avant de regarder le premier point
                pointActuel = points.Peek(); // le nouveau point actuel sera celui en t�te de la liste 
            else // l'alog se termine lorsque plus aucun point n'est disponible (la queue est vide)
                return;
        }
        TrouverChemin();
        perso.seD�place = true;
    }

    void Dijsktra()
    {
        NavPoint pointActuel = perso.TrouverPointD�part(); // le premier point est le point de d�part

        // il n'y a aucune distance entre le point de d�part et son parent, puisque son parent est non existant
        //        <point   , distance entre point et les points pr�c�dents>
        Dictionary<NavPoint, float> pointsInfo = new Dictionary<NavPoint, float>() { { pointActuel, 0f } };
        while (!V�rifierEstDestination(pointActuel))
        {
            // On passe au travers de tous les voisins non visit�s du point actuel et
            // on les ajoute � un dictionnaire ainsi que leur distance par rapport au point actuel
            Dictionary<NavPoint, float> voisinsInfo = new Dictionary<NavPoint, float>();

            foreach(NavPoint voisin in pointActuel.voisins)
                if (!voisin.estVisit�)
                {
                    voisin.Parent = pointActuel;
                    voisin.SetVisit�(true); // le point ajout� au dictionnaire est maintenant visit�
                    voisinsInfo.Add(voisin, GetDistanceEntre(voisin, pointActuel) + pointsInfo.First().Value); 
                    // la distance entre le point et le point parent + la distance pr�c�dente "pointActuel.Value"
                }
            pointsInfo.AddRange(voisinsInfo); // on ajoute ces voisins au dictionnaire
            pointsInfo = pointsInfo.OrderBy(v => v.Value).ToDictionary(v => v.Key, v => v.Value); // on met en ordre nos points de la plus petite � la plus grande distance

            pointsInfo.Remove(pointActuel); // on retire le pointActuel, celui qui vient d'�tre visit�

            if (pointsInfo.Count != 0)
                pointActuel = pointsInfo.First().Key; // le nouveau point est celui avec la plus petite distance, donc le plus r�cent 
            else
                return;
        }

        TrouverChemin();
        perso.seD�place = true;
    }

    float GetDistanceEntre(NavPoint p1, NavPoint p2) // la distance entre deux NavPoints
        => Vector3.Distance(p1.transform.position, p2.transform.position);

    private void TrouverChemin()
    {
        NavPoint pointActuel = perso.Destination; // on retrace nos pas pour trouver le chemin inverse, donc on commence par la destination
        chemin = new List<NavPoint>(); // on instantie une nouvelle liste pour reset le pr�c�dent chemin enregistr�

        while (pointActuel != null)
        {
            chemin.Add(pointActuel);
            pointActuel.SetChoisie(true); // le point a �t� choisie pour le chemin
            pointActuel = pointActuel.Parent; // le point de la prochaine it�ration est le point parent de celui-ci
        }

        chemin.Reverse(); // on reverse la liste pour commencer du point de d�part � l'arriv�e
    }
}

/*
* navpoint = destination ?
* -> ChercherVoisins
*      -> Ajouter nouveau
*      -> Si point dans liste ET n'est pas visit� ET la distance pour les atteindre est plus petite que liste
*          -> Update distance
*  -> Sort < Distance
**/