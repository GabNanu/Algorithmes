using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPoint : MonoBehaviour
{
    [SerializeField] float distanceDuSol = 1f;
    [SerializeField] float D�nivel� = 1f;

    [SerializeField] Material NavChoisieMat;
    [SerializeField] Material NavDestinationMat;
    [SerializeField] Material NavVisit�Mat;
    [SerializeField] Material NavNormalMat;

    public List<NavPoint> voisins = new(); // liste publique pour pouvoir SetChoisie, SetVisit� etc. dans NavAgent

    public bool estChoisie { get; private set; } // est le point de d�part o� le joueur d�bute
    public bool estVisit� { get; private set; } // le point a �t� visit� par l'algorithme
    public bool estDestination { get; private set; } // le point est choisis comme le point final

    int x, z;

    public NavPoint Parent = null; // le point parent est null de base puisqu'il ne poss�de aucun parent

    public bool Poss�deParent()
        => Parent != null;

    public void ResetParent()
    {
        Parent = null;
    }

    public void SetXandZ(int i, int j) // est appel� par navigation g�n�rateur lors de la cr�ation d'un point 
    {
        x = i;
        z = j;
    }

    // les fonctions suivantes permettent de changer les mat�riaux d�pendamment des propri�t�s du point
    public void SetChoisie(bool valeur)
    {
        estChoisie = valeur;
        if (valeur)
        {
            if (!estDestination) // m�me si la destination fait partie du chemin, on ne veut pas changer le mat�riel de la destination
                GetComponent<MeshRenderer>().material = NavChoisieMat;
        }
        else
            GetComponent<MeshRenderer>().material = NavNormalMat;
    }
    public void SetVisit�(bool valeur)
    {
        estVisit� = valeur;
        if (valeur)
        {
            if (!estDestination) // m�me si la destination est visit�, on ne veut pas changer le mat�riel de la destination
                GetComponent<MeshRenderer>().material = NavVisit�Mat;
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
        StartCoroutine(Cr�erPoint());
    }

    IEnumerator Cr�erPoint()
    {
        yield return new WaitForSeconds(1);// on attend 1 seconde pour s'assurer que les points pr�c�dents aient bien �t� supprim�s et qu'ils n'int�rf�rent pas avec les nouveaux points
        Projeter(); // projette le point vers le sol

        yield return new WaitForSeconds(1); // on attend 1 seconde. Cela �vite qu'on fasse r�f�rence sur un objet n'ayant pas encore �t� instanci� dans NavGen
        EffectuerConnections();// trouve les voisins de ce navpoint et les ajoute � la liste voisins
    }

    void Projeter()
    {
        // yield return new WaitForSeconds(1); // on attend 1 seconde pour s'assurer que les points pr�c�dents aient bien �t� supprim�s et qu'ils n'int�rf�rent pas avec les nouveaux points

        // on envoie un Ray vers le sol
        Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit);

        // on set la position du point un peu plus haut que le point de contact du RaycastHit 
        transform.position = hit.point + Vector3.up * distanceDuSol;
    }

    void EffectuerConnections()
    {
        // on get la 'grille' de points cr��e par NavigationG�n�rateur
        GameObject[,] points = GameObject.FindGameObjectWithTag("NavGen").GetComponent<NavigationG�n�rateur>().points;

        // on consid�re que x et z ont �t� d�clar�s par la m�thode SetXandZ lorsqu'on cr�e un point dans navigation g�n�rateur
        // on regarde les points environnants dans la grille et on s'assure que les points environnants de d�passent pas les limites de la matrice 2D points
        for (int i = x - 1 >= 0 ? x - 1 : 0; x + 1 < points.GetLength(0) ? i <= x + 1 : i < points.GetLength(0); i++)
        {
            for (int j = z - 1 >= 0 ? z - 1 : 0; z + 1 < points.GetLength(1) ? j <= z + 1 : j < points.GetLength(1); j++)
            {
                // on regarde si le point rempli les conditions n�cessaires pour �tre voisin et si le point n'est pas lui-m�me
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
            // on v�rifie que le d�nivel� est plus petit que la valeur D�nivel�,
            // et on regarde si le raycastHit est entr� en contact avec un autre point (on v�rifie donc qu'il n'y a aucun obstacle en chemin)
            return Mathf.Abs(transform.position.y - autre.transform.position.y) < D�nivel� && hit.collider.CompareTag("NavPoint");
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
