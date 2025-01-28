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

    [SerializeField] public List<NavPoint> chemin; // le chemin de point trouvé par l'algorithme

    private void Start()
    {
        perso = GetComponent<Personnage>();
    }

    public void CommencerAlgo() // est appelé par personnage lorsqu'une destination a été sélectionnée
    {
        // liste des fonctions pouvant être appelé en ordre selon le dropdown
        List<Action> fonctionsAlgos = new List<Action>() { DepthFirstSearch, BreadthFirstSearch, Dijsktra };

        perso.TrouverPointDépart(); // on trouve le point de départ 

        fonctionsAlgos[dropdown.value](); // on fait appel à la fonction représentant l'algorithme sélectionné dans le dropdown
    }

    private bool VérifierEstDestination(NavPoint point)
    {
        point.SetVisité(true); // maintenant qu'on a vérifié si le point est la destination, ce point a été visité
        return point.estDestination;
    }

    void DepthFirstSearch()
    {
        NavPoint pointActuel = perso.TrouverPointDépart();
        Stack<NavPoint> points = new Stack<NavPoint>();
        points.Push(pointActuel);

        while (!VérifierEstDestination(pointActuel))
        {
            // puisque le point actuel a été visité, on peut le retirer de la stack
            // et le nouveau point actuel sera le prochain dans le stack
            if (points.Count != 0) // on regarde si la stack est vide avant d'accéder au prochain point
                pointActuel = points.Pop();
            else
                return; // si la stack est vide, alors l'algorithme se termine sans avoir trouvé sa destination puisqu'il a fait le tour de tous les points accessibles

            foreach (NavPoint voisin in pointActuel.voisins)
                if (!voisin.estVisité) // on vérifie si le point a déjà été visité ou si le point n'est pas déja contenu dans la stack
                {
                    voisin.SetVisité(true);
                    points.Push(voisin); // le nouveau point actuel est rajouté au top de la liste et sera visité à la prochaine itération
                    voisin.Parent = pointActuel;
                }
        }
        TrouverChemin();
        perso.seDéplace = true;
    }

    void BreadthFirstSearch()
    {
        NavPoint pointActuel = perso.TrouverPointDépart();
        Queue<NavPoint> points = new Queue<NavPoint>(); // la file des points potentiels, soit les points qui seront visités. À la fin de l'algorithme, points devrait contenir la liste des points choisis
        points.Enqueue(pointActuel); // le premier point à visiter est bien évidemment le point de départ

        while (!VérifierEstDestination(pointActuel)) // on arrête l'algorithme lorsque le point a atteint sa destination
        {
            foreach (NavPoint voisin in pointActuel.voisins) // on ajoute tous les voisins non-visités à la file des voisins à visiter et on vérifie si le voisin n'a pas déjà été ajouté a la liste
                if(!voisin.estVisité) 
                {
                    voisin.SetVisité(true);
                    points.Enqueue(voisin);
                    voisin.Parent = pointActuel;
                }

            points.Dequeue(); // on retire le point actuel

            if (points.Count != 0) // on vérifie si la queue est vide avant de regarder le premier point
                pointActuel = points.Peek(); // le nouveau point actuel sera celui en tête de la liste 
            else // l'alog se termine lorsque plus aucun point n'est disponible (la queue est vide)
                return;
        }
        TrouverChemin();
        perso.seDéplace = true;
    }

    void Dijsktra()
    {
        NavPoint pointActuel = perso.TrouverPointDépart(); // le premier point est le point de départ

        // il n'y a aucune distance entre le point de départ et son parent, puisque son parent est non existant
        //        <point   , distance entre point et les points précédents>
        Dictionary<NavPoint, float> pointsInfo = new Dictionary<NavPoint, float>() { { pointActuel, 0f } };
        while (!VérifierEstDestination(pointActuel))
        {
            // On passe au travers de tous les voisins non visités du point actuel et
            // on les ajoute à un dictionnaire ainsi que leur distance par rapport au point actuel
            Dictionary<NavPoint, float> voisinsInfo = new Dictionary<NavPoint, float>();

            foreach(NavPoint voisin in pointActuel.voisins)
                if (!voisin.estVisité)
                {
                    voisin.Parent = pointActuel;
                    voisin.SetVisité(true); // le point ajouté au dictionnaire est maintenant visité
                    voisinsInfo.Add(voisin, GetDistanceEntre(voisin, pointActuel) + pointsInfo.First().Value); 
                    // la distance entre le point et le point parent + la distance précédente "pointActuel.Value"
                }
            pointsInfo.AddRange(voisinsInfo); // on ajoute ces voisins au dictionnaire
            pointsInfo = pointsInfo.OrderBy(v => v.Value).ToDictionary(v => v.Key, v => v.Value); // on met en ordre nos points de la plus petite à la plus grande distance

            pointsInfo.Remove(pointActuel); // on retire le pointActuel, celui qui vient d'être visité

            if (pointsInfo.Count != 0)
                pointActuel = pointsInfo.First().Key; // le nouveau point est celui avec la plus petite distance, donc le plus récent 
            else
                return;
        }

        TrouverChemin();
        perso.seDéplace = true;
    }

    float GetDistanceEntre(NavPoint p1, NavPoint p2) // la distance entre deux NavPoints
        => Vector3.Distance(p1.transform.position, p2.transform.position);

    private void TrouverChemin()
    {
        NavPoint pointActuel = perso.Destination; // on retrace nos pas pour trouver le chemin inverse, donc on commence par la destination
        chemin = new List<NavPoint>(); // on instantie une nouvelle liste pour reset le précédent chemin enregistré

        while (pointActuel != null)
        {
            chemin.Add(pointActuel);
            pointActuel.SetChoisie(true); // le point a été choisie pour le chemin
            pointActuel = pointActuel.Parent; // le point de la prochaine itération est le point parent de celui-ci
        }

        chemin.Reverse(); // on reverse la liste pour commencer du point de départ à l'arrivée
    }
}

/*
* navpoint = destination ?
* -> ChercherVoisins
*      -> Ajouter nouveau
*      -> Si point dans liste ET n'est pas visité ET la distance pour les atteindre est plus petite que liste
*          -> Update distance
*  -> Sort < Distance
**/