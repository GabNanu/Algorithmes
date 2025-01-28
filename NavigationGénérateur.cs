using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.IO.LowLevel.Unsafe;

public class NavigationGénérateur : MonoBehaviour
{
    [SerializeField] public int dimensionX { get; private set; }
    [SerializeField] public int dimensionZ { get; private set; }

    [SerializeField] TextMeshProUGUI valX;
    [SerializeField] TextMeshProUGUI valZ;

    [SerializeField] GameObject point;

    Bounds bounds;

    Personnage perso;

    public GameObject[,] points;

    // Start is called before the first frame update
    void Start()
    {
        // le personnage est le seul objet avec le tag Player et il a toujours le component Personnage
        perso = GameObject.FindGameObjectWithTag("Player").GetComponent<Personnage>();
        bounds = GetComponent<BoxCollider>().bounds;
    }

    public void GénérerNav()
    {
        // si le personnage se déplace, alors les points sont encore entrain d'être utilisés et donc nous n'allons pas générer une nouvelle grille de point
        if (!perso.seDéplace)
        {
            DétruirePoints(); // Détruit les anciens points
            InstantierPoints();
        }
    }


    void DétruirePoints()
    {
        if (points != null)
        {
            foreach (GameObject point in points)
                Destroy(point);
        }
    }

    void InstantierPoints()
    {
        // On crée une nouveau tableau de points, puisqu'on souhaite utiliser de nouveaux points et non les précédents
        points = new GameObject[dimensionX, dimensionZ];

        // On veut calculer la distance qu'il y a entre chaque points. 
        // Si on a 10 points en x, il y aura donc 9 espacements entre chaque point, on veut donc diviser la taille en X par le nombre d'espacemnts qu'il y aura en X, soit un de moins que le nombre de points
        float distancePointsX = dimensionX != 1 ? bounds.size.x / (dimensionX-1) : 0, // s'il n'y a qu'un seul point, alors la distance entre les points est de 0 (on évite ici une division par zéro)
            distancePointsZ = dimensionZ != 1 ? bounds.size.z / (dimensionZ-1) : 0;
        


        Vector3 pointDépart = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);

        for (int x = 0; x < dimensionX; ++x)
        {
            for (int z = 0; z < dimensionZ; ++z)
            {
                // On instantie les points en fonction de la position de départ, à laquelle on rajoute la distance en X et la distance en Z
                // On utilise Quaternion.identity, puisqu'on ne désire aucune rotation sur nos points
                GameObject nouveauPoint = Instantiate(point, new Vector3(pointDépart.x - distancePointsX * x, pointDépart.y, pointDépart.z - distancePointsZ * z), Quaternion.identity);
                points[x, z] = nouveauPoint;
                nouveauPoint.GetComponent<NavPoint>().SetXandZ(x, z);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        dimensionX = int.Parse(valX.text);
        dimensionZ = int.Parse(valZ.text);
    }
}
