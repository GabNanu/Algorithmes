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

    public bool seDéplace = false; // le personnage est toujours immobile au lancement
    int indexChemin = 0; // l'index du point où le personnage devra se déplacer

    [SerializeField] float vitesse = 15f;

    private void Start()
    {
        navAgent = GetComponent<NavAgent>();
    }
    private void Update()
    {
        if(seDéplace)
        {
            DéplacerPersonnage();
        }
        else if(Input.GetMouseButtonDown(0)) // si le personnage n'est plus en mouvement on peut commencer un nouvel algo
        {
            // on converti la position de la souris en ray pour ensuite trouver le point le plus proche de où le raycast a hit
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                NavPoint pointCliqué = TrouverPointLePlusProche(hit.point);
                if(pointCliqué != null) // on vérifie si un point a été retourné, sinon cela signifie que la navigation n'a pas été générée
                {
                    ResetPoints(); // on reset les points de l'algo précédent
                    SetNewDestination(pointCliqué);
                    navAgent.CommencerAlgo(); // on commence l'algorithme maintenant qu'on a la destination souhaitée
                }
            }
        }
    }

    private void SetNewDestination(NavPoint point)
    {
        if (Destination != null) // on retire la destination de l'ancien point s'il en existe un
            Destination.SetDestination(false);
        point.SetDestination(true);
        Destination = point; // on enregistre le point pour qu'on puisse le réutiliser dans NavAgent sans devoir retrouver
    }

    public NavPoint TrouverPointDépart()
        => TrouverPointLePlusProche(transform.position); // le point le plus proche du joueur est le point de départ

    private NavPoint TrouverPointLePlusProche(Vector3 pos)
    {
        GameObject[,] points = GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationGénérateur>().points;
        if (points == null) // Si la liste des points est null, cela signifie qu'aucun point n'a été instancié dans la liste et donc on retourne null
            return null;
        float distance = 1000; // valeur arbitraire (la distance devrait toujours être plus petite que 1000 dans notre situation)
        GameObject pointProche = points[0,0]; // On commence avec le premier point (il devrait toujours avoir un point en 0,0 )

        // On regarde tous les points et on store la distance la plus petite. Si la distance est plus petite que la précédente, alors on store le nouveau point le plus proche
        foreach(GameObject point in points)
            if(Vector3.Distance(point.transform.position, pos) < distance)
            {
                distance = Vector3.Distance(point.transform.position, pos);
                pointProche = point;
            }

        return pointProche.GetComponent<NavPoint>();
    }

    private void DéplacerPersonnage()
    {
        // on déplace le personnage vers le point sélectionné 
        transform.position = Vector3.MoveTowards(transform.position, navAgent.chemin[indexChemin].transform.position, Time.deltaTime * vitesse);

        //si la distance entre le personnage et le point est assez petite, alors on peut passer au point suivant et donc augmenter l'index
        if(Vector3.Distance(transform.position, navAgent.chemin[indexChemin].transform.position) < .05f)
        {
            // on vérifie si on a terminé le chemin avant d'augmenter l'index
            if (indexChemin + 1 < navAgent.chemin.Count)
                indexChemin++;
            else // maintenant que le joueur a fini de se déplacer, seDéplace est false et on remet l'index à 0 pour le prochain déplacement
            {
                seDéplace = false;
                indexChemin = 0;
            }
        }
    }

    void ResetPoints()
    {
        foreach(GameObject objet in GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationGénérateur>().points)
        {
            NavPoint point = objet.GetComponent<NavPoint>();
            point.SetVisité(false);
            point.SetChoisie(false);
            point.ResetParent();
        }
    }
}
