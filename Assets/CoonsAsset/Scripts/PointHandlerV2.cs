using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.XR;
using UnityEngine.UI;

public class PointHandlerV2 : MonoBehaviour
{
    public GameObject pointPrefab; // Prefab pour l'objet point
    public List<GameObject> points = new List<GameObject>(); // Liste pour stocker les points
    private List<GameObject> lines = new List<GameObject>(); // Liste pour stocker les objets ligne (polygones)

    public bool drawing = false; // Indique si le dessin est en cours
    public Color currentColor = Color.red; // Couleur actuellement s�lectionn�e

    public bool isCheckingPolygon = false; // Indique si la v�rification des polygones est active
    public List<List<GameObject>> courbes = new List<List<GameObject>>();
    public List<GameObject> meshes = new List<GameObject>();

    List<Vector3> LastCurvePoints = new List<Vector3>();

    public Drawing drawable;
    public bool clearOne = false;

    public Vector3 axisPosition = Vector3.zero;

    private List<Vector3> curvePoints;

    public static bool setMaterialWood = false;
    public static bool setMaterialMetal = false;


    void Update()
    {
        {
            // V�rifier si le clic gauche de la souris est enfonc� et si le dessin est en cours et que la souris n'est pas sur un objet UI
            if (Input.GetMouseButtonDown(0) && drawing && !IsPointerOverUIObject())
            {
                // Obtenir la position de la souris dans l'espace du monde
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f; // S'assurer que la coordonn�e z est 0 pour l'espace 2D

                // Instancier un objet point � la position de la souris
                GameObject point = Instantiate(pointPrefab, mousePos, Quaternion.identity);
                point.tag = "controlPoint";
                point.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du point
                points.Add(point); // Ajouter le point � la liste
            }

            // V�rifier si le clic droit de la souris est enfonc� et si le dessin est en cours
            if (Input.GetMouseButtonDown(1) && drawing)
            {
                drawing = false; // Arr�ter le dessin
                List<GameObject> currentPoints = new List<GameObject>(points);
                ConnectPoints(currentPoints); // Connecter les points pour former un polygone
                courbes.Add(currentPoints);
            }


            // V�rifier si la v�rification des polygones est active et si le clic gauche de la souris est enfonc�
            if (isCheckingPolygon && Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                isCheckingPolygon = false;
                LastCurvePoints.Clear();
                GameObject insidePolygon = IsInsidePolygon();
                if (insidePolygon != null)
                {
                    Debug.Log("Clic � l'int�rieur du polygone : " + insidePolygon.name);
                    // Faites quelque chose avec l'objet line (polygone) trouv�

                    // Get the points of the clicked polygon
                    List<GameObject> polygonPoints = new List<GameObject>();
                    foreach (Transform child in insidePolygon.transform)
                    {
                        polygonPoints.Add(child.gameObject);
                    }

                    if (clearOne)
                    {
                        Destroy(insidePolygon);
                        clearOne = false;
                        lines.Remove(insidePolygon);
                        //need to add : delete the polygon from 'courbes'
                        print("cleared one polygon");
                    }
                }
            } 
        }
    }

    public void ClearOne()
    {
        clearOne = true;
    }

    // M�thode pour connecter les points pour former un polygone
    public void ConnectPoints(List<GameObject> currentPoints)
    {
        // S'assurer qu'il y a au moins 3 points pour former un polygone
        if (currentPoints.Count < 3)
        {
            Debug.LogWarning("Pas assez de points pour former un polygone.");
            return;
        }

        // Cr�er un nouvel objet pour contenir les lignes (polygone)
        GameObject polygonObj = new GameObject("Polygon");
        lines.Add(polygonObj);

        // Ajouter un composant LineRenderer pour dessiner les lignes
        LineRenderer lineRenderer = polygonObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = currentPoints.Count; // D�finir le nombre de positions
        lineRenderer.startWidth = 0.02f; // D�finir la largeur de la ligne
        lineRenderer.endWidth = 0.02f;
        lineRenderer.loop = true; // Relier le dernier point au premier point

        // D�finir la couleur de la ligne pour correspondre � la couleur des points
        lineRenderer.material.color = currentColor;

        // D�finir les positions pour le LineRenderer
        for (int i = 0; i < currentPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, currentPoints[i].transform.position);
            currentPoints[i].transform.parent = polygonObj.transform; // Faire du point un enfant de l'objet ligne (polygone)
        }

        // Effacer la liste des points pour la prochaine session de dessin
        points.Clear();
    }

    // M�thode pour effacer tous les points et polygones
    public void Clear()
    {
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
        points.Clear();

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        courbes.Clear();
    }

    // M�thode pour d�finir la couleur de dessin actuelle
    public void SetColorRed()
    {
        currentColor = Color.red;
        drawing = true; // Commencer le dessin lorsque la couleur est s�lectionn�e
    }

    public void SetColorGreen()
    {
        currentColor = Color.green;
        drawing = true; // Commencer le dessin lorsque la couleur est s�lectionn�e
    }

    public void SetColorBlue()
    {
        currentColor = Color.blue;
        drawing = true; // Commencer le dessin lorsque la couleur est s�lectionn�e
    }

    public void SetColorBlack()
    {
        currentColor = Color.black;
        drawing = true;
    }

    // M�thode pour v�rifier si le pointeur de la souris est sur un objet UI
    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    // M�thode pour activer la v�rification des polygones
    public void StartCheckingPolygon()
    {
        isCheckingPolygon = true;
    }

    // M�thode pour v�rifier si le clic gauche de la souris est � l'int�rieur d'un polygone existant
    public GameObject IsInsidePolygon()
    {
        // Obtenir la position du clic de la souris dans l'espace du monde
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // S'assurer que la coordonn�e z est 0 pour l'espace 2D

        foreach (GameObject lineObj in lines)
        {
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                Vector2[] polygon = new Vector2[lineRenderer.positionCount];
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    polygon[i] = new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).y);
                }

                if (IsPointInPolygon(new Vector2(mousePos.x, mousePos.y), polygon))
                {
                    return lineObj; // Le clic est � l'int�rieur d'un polygone existant
                }
            }
        }
        return null; // Le clic est � l'ext�rieur de tous les polygones
    }

    // M�thode pour v�rifier si un point est � l'int�rieur d'un polygone
    private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length;
        bool inside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            if (((polygon[i].y <= point.y && point.y < polygon[j].y) || (polygon[j].y <= point.y && point.y < polygon[i].y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
