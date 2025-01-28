using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Personnage : MonoBehaviour
{
    NavAgent navAgent;
    public NavPoint Destination;

    public bool seD�place = false; // le personnage est toujours immobile au lancement
    int indexChemin = 0; // l'index du point o� le personnage devra se d�placer

    [SerializeField] float vitesse = 15f;

    private void Start()
    {
        navAgent = GetComponent<NavAgent>();
    }
    private void Update()
    {
        if(seD�place)
        {
            D�placerPersonnage();
        }
        else if(Input.GetMouseButtonDown(0)) // si le personnage n'est plus en mouvement on peut commencer un nouvel algo
        {
            // on converti la position de la souris en ray pour ensuite trouver le point le plus proche de o� le raycast a hit
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                NavPoint pointCliqu� = TrouverPointLePlusProche(hit.point);
                if(pointCliqu� != null) // on v�rifie si un point a �t� retourn�, sinon cela signifie que la navigation n'a pas �t� g�n�r�e
                {
                    ResetPoints(); // on reset les points de l'algo pr�c�dent
                    SetNewDestination(pointCliqu�);
                    navAgent.CommencerAlgo(); // on commence l'algorithme maintenant qu'on a la destination souhait�e
                }
            }
        }
    }

    private void SetNewDestination(NavPoint point)
    {
        if (Destination != null) // on retire la destination de l'ancien point s'il en existe un
            Destination.SetDestination(false);
        point.SetDestination(true);
        Destination = point; // on enregistre le point pour qu'on puisse le r�utiliser dans NavAgent sans devoir retrouver
    }

    public NavPoint TrouverPointD�part()
        => TrouverPointLePlusProche(transform.position); // le point le plus proche du joueur est le point de d�part

    private NavPoint TrouverPointLePlusProche(Vector3 pos)
    {
        GameObject[,] points = GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationG�n�rateur>().points;
        if (points == null) // Si la liste des points est null, cela signifie qu'aucun point n'a �t� instanci� dans la liste et donc on retourne null
            return null;
        float distance = 1000; // valeur arbitraire (la distance devrait toujours �tre plus petite que 1000 dans notre situation)
        GameObject pointProche = points[0,0]; // On commence avec le premier point (il devrait toujours avoir un point en 0,0 )

        // On regarde tous les points et on store la distance la plus petite. Si la distance est plus petite que la pr�c�dente, alors on store le nouveau point le plus proche
        foreach(GameObject point in points)
            if(Vector3.Distance(point.transform.position, pos) < distance)
            {
                distance = Vector3.Distance(point.transform.position, pos);
                pointProche = point;
            }

        return pointProche.GetComponent<NavPoint>();
    }

    private void D�placerPersonnage()
    {
        // on d�place le personnage vers le point s�lectionn� 
        transform.position = Vector3.MoveTowards(transform.position, navAgent.chemin[indexChemin].transform.position, Time.deltaTime * vitesse);

        //si la distance entre le personnage et le point est assez petite, alors on peut passer au point suivant et donc augmenter l'index
        if(Vector3.Distance(transform.position, navAgent.chemin[indexChemin].transform.position) < .05f)
        {
            // on v�rifie si on a termin� le chemin avant d'augmenter l'index
            if (indexChemin + 1 < navAgent.chemin.Count)
                indexChemin++;
            else // maintenant que le joueur a fini de se d�placer, seD�place est false et on remet l'index � 0 pour le prochain d�placement
            {
                seD�place = false;
                indexChemin = 0;
            }
        }
    }

    void ResetPoints()
    {
        foreach(GameObject objet in GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationG�n�rateur>().points)
        {
            NavPoint point = objet.GetComponent<NavPoint>();
            point.SetVisit�(false);
            point.SetChoisie(false);
            point.ResetParent();
        }
    }
}
