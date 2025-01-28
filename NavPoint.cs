using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPoint : MonoBehaviour
{
    [SerializeField] float distanceDuSol = 1f;
    [SerializeField] float Dénivelé = 1f;

    [SerializeField] Material NavChoisieMat;
    [SerializeField] Material NavDestinationMat;
    [SerializeField] Material NavVisitéMat;
    [SerializeField] Material NavNormalMat;

    public List<NavPoint> voisins = new(); // liste publique pour pouvoir SetChoisie, SetVisité etc. dans NavAgent

    public bool estChoisie { get; private set; } // est le point de départ où le joueur débute
    public bool estVisité { get; private set; } // le point a été visité par l'algorithme
    public bool estDestination { get; private set; } // le point est choisis comme le point final

    int x, z;

    public NavPoint Parent = null; // le point parent est null de base puisqu'il ne possède aucun parent

    public bool PossèdeParent()
        => Parent != null;

    public void ResetParent()
    {
        Parent = null;
    }

    public void SetXandZ(int i, int j) // est appelé par navigation générateur lors de la création d'un point 
    {
        x = i;
        z = j;
    }

    // les fonctions suivantes permettent de changer les matériaux dépendamment des propriétés du point
    public void SetChoisie(bool valeur)
    {
        estChoisie = valeur;
        if (valeur)
        {
            if (!estDestination) // même si la destination fait partie du chemin, on ne veut pas changer le matériel de la destination
                GetComponent<MeshRenderer>().material = NavChoisieMat;
        }
        else
            GetComponent<MeshRenderer>().material = NavNormalMat;
    }
    public void SetVisité(bool valeur)
    {
        estVisité = valeur;
        if (valeur)
        {
            if (!estDestination) // même si la destination est visité, on ne veut pas changer le matériel de la destination
                GetComponent<MeshRenderer>().material = NavVisitéMat;
        }
        else
            GetComponent<MeshRenderer>().material = NavNormalMat;
    }
    public void SetDestination(bool valeur)
    {
        estDestination = valeur;
        if (valeur)
            GetComponent<MeshRenderer>().material = NavDestinationMat;
        else
            GetComponent<MeshRenderer>().material = NavNormalMat;
    }

    private void Awake()
    {
        StartCoroutine(CréerPoint());
    }

    IEnumerator CréerPoint()
    {
        yield return new WaitForSeconds(1);// on attend 1 seconde pour s'assurer que les points précédents aient bien été supprimés et qu'ils n'intérfèrent pas avec les nouveaux points
        Projeter(); // projette le point vers le sol

        yield return new WaitForSeconds(1); // on attend 1 seconde. Cela évite qu'on fasse référence sur un objet n'ayant pas encore été instancié dans NavGen
        EffectuerConnections();// trouve les voisins de ce navpoint et les ajoute à la liste voisins
    }

    void Projeter()
    {
        // yield return new WaitForSeconds(1); // on attend 1 seconde pour s'assurer que les points précédents aient bien été supprimés et qu'ils n'intérfèrent pas avec les nouveaux points

        // on envoie un Ray vers le sol
        Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit);

        // on set la position du point un peu plus haut que le point de contact du RaycastHit 
        transform.position = hit.point + Vector3.up * distanceDuSol;
    }

    void EffectuerConnections()
    {
        // on get la 'grille' de points créée par NavigationGénérateur
        GameObject[,] points = GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationGénérateur>().points;

        // on considère que x et z ont été déclarés par la méthode SetXandZ lorsqu'on crée un point dans navigation générateur
        // on regarde les points environnants dans la grille et on s'assure que les points environnants de dépassent pas les limites de la matrice 2D points
        for (int i = x - 1 >= 0 ? x - 1 : 0; x + 1 < points.GetLength(0) ? i <= x + 1 : i < points.GetLength(0); i++)
        {
            for (int j = z - 1 >= 0 ? z - 1 : 0; z + 1 < points.GetLength(1) ? j <= z + 1 : j < points.GetLength(1); j++)
            {
                // on regarde si le point rempli les conditions nécessaires pour être voisin et si le point n'est pas lui-même
                if (EstVoisin(points[i, j]) && !points[x, z].Equals(points[i,j]))
                    voisins.Add(points[i, j].GetComponent<NavPoint>());
            }
        }
    }

    bool EstVoisin(GameObject autre)
    {
        // On calcule le vecteur direction de notre point et du point autre et on raycast ce ray dans cette direction
        if(Physics.Raycast(new Ray(transform.position, (autre.transform.position - transform.position).normalized), out RaycastHit hit))
        {
            // on vérifie que le dénivelé est plus petit que la valeur Dénivelé,
            // et on regarde si le raycastHit est entré en contact avec un autre point (on vérifie donc qu'il n'y a aucun obstacle en chemin)
            return Mathf.Abs(transform.position.y - autre.transform.position.y) < Dénivelé && hit.collider.CompareTag("NavPoint");
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (NavPoint voisin in voisins)
            Gizmos.DrawLine(transform.position, voisin.transform.position);
    }
}
