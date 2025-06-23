using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.XR;
using UnityEngine.UI;

public class PointHandler : MonoBehaviour
{
    public GameObject pointPrefab; // Prefab pour l'objet point
    public List<GameObject> points = new List<GameObject>(); // Liste pour stocker les points
    public List<GameObject> pointsZ = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>(); // Liste pour stocker les objets ligne (polygones)
    private bool drawing = false; // Indique si le dessin est en cours
    public Color currentColor = Color.red; // Couleur actuellement s�lectionn�e

    public bool isCheckingPolygon = false; // Indique si la v�rification des polygones est active
    public List<List<GameObject>> courbes = new List<List<GameObject>>();
    public List<GameObject> meshes = new List<GameObject>();
    List<Vector3> LastCurvePoints = new List<Vector3>();

    public Casteljau decasteljauScript;
    public Pascal pascalScript;
    public Drawing drawable;
    public bool clearOne = false;

    public bool isLinking = false;
    public int linkType;

    public GameObject extrusionPrefab;
    public GameObject extrusionAxePrefab;
    public GameObject extrusionPathPrefab;
    private bool isExtruding = false;
    private bool isExtrudingAxe = false;
    private bool isActivePascal = false;
    private bool isZCurve=false;
    public bool isRevExtruding = false;
    private bool isChoosingAxis = false;
    public float extrusionLength = 5f;
    public Vector3 axisPosition = Vector3.zero;

    private List<Vector3> curvePoints;

    public static bool setMaterialWood = false;
    public static bool setMaterialMetal = false;

    [SerializeField] private Image woodBorder;
    [SerializeField] private Image metalBorder;
    [SerializeField] private Image noneBorder;


    void Update()
    {
        if (isZCurve)
        {
            Debug.Log("CurveZ");
            GameObject curveZGameObject = IsInsidePolygon();
            // V�rifier si le clic gauche de la souris est enfonc� et si le dessin est en cours et que la souris n'est pas sur un objet UI
            if (Input.GetMouseButtonDown(0) && drawing && !IsPointerOverUIObject())
            {
                // Obtenir la position de la souris dans l'espace du monde
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //mousePos.y = 0f; // S'assurer que la coordonn�e z est 0 pour l'espace 2D

                // Instancier un objet point � la position de la souris
                GameObject point = Instantiate(pointPrefab, mousePos, Quaternion.identity);
                point.tag = "controlPoint";
                point.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du point
                pointsZ.Add(point); // Ajouter le point � la liste
            }

            // V�rifier si le clic droit de la souris est enfonc� et si le dessin est en cours
            if (Input.GetMouseButtonDown(1) && drawing)
            {
                drawing = false; // Arr�ter le dessin
                List<GameObject> currentPointsZ = new List<GameObject>(pointsZ);
                ConnectPoints(currentPointsZ); // Connecter les points pour former un polygone
                courbes.Add(currentPointsZ);
                decasteljauScript.DrawBezierCurve(currentPointsZ, curveZGameObject);
                List<Vector3> curvePoints = decasteljauScript.GetCurvePoints(currentPointsZ);
                CreateExtrusionPath(LastCurvePoints, curvePoints, curveZGameObject.transform);
                isZCurve = false;
            }
        }
        else
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

                    if (decasteljauScript.decasteljau)
                    {
                        decasteljauScript.DrawBezierCurve(polygonPoints, insidePolygon);
                        decasteljauScript.decasteljau = false;
                        print("casteljau function worked");

                        isActivePascal = false;

                        List<Vector3> curvePoints = decasteljauScript.GetCurvePoints(polygonPoints);
                        LastCurvePoints = curvePoints;

                    }
                    else if (pascalScript.pascal)
                    {
                        pascalScript.DrawCurve(polygonPoints, insidePolygon);
                        pascalScript.pascal = false;
                        print("pascal function worked");
                        isActivePascal = true;

                        List<Vector3> curvePoints = decasteljauScript.GetCurvePoints(polygonPoints);
                        LastCurvePoints = curvePoints;
                    }
                    else if (isExtruding)
                    {
                        List<Vector3> curvePoints = decasteljauScript.decasteljau ?
                            decasteljauScript.GetCurvePoints(polygonPoints) :
                            pascalScript.GetCurvePoints(polygonPoints);

                        decasteljauScript.DrawBezierCurve(polygonPoints, insidePolygon);
                        CreateAndExtrudeObject(curvePoints, insidePolygon.transform);
                        isExtruding = false; 
                    }
                    else if (isRevExtruding)
                    {
                        if (isChoosingAxis)
                        {
                            Debug.Log("Revolve");
                            List<Vector3> curvePoints = decasteljauScript.decasteljau ?
                                decasteljauScript.GetCurvePoints(polygonPoints) :
                                pascalScript.GetCurvePoints(polygonPoints);

                            decasteljauScript.DrawBezierCurve(polygonPoints, insidePolygon);
                            CreateExtrusionAxe(curvePoints, insidePolygon.transform, axisPosition);

                            isChoosingAxis = false;
                        }

                    }
                    else if (isLinking)
                    {
                        if (linkType == 0)
                        {
                            C0Link(polygonPoints);
                        }
                        else if (linkType == 1)
                        {
                            C1Link(polygonPoints);
                        }
                        else if (linkType == 2)
                        {
                            C2Link(polygonPoints);
                        }
                        isLinking = false;
                        drawing = true;
                    }
                    else if (clearOne)
                    {
                        Destroy(insidePolygon);
                        clearOne = false;
                        lines.Remove(insidePolygon);
                        //need to add : delete the polygon from 'courbes'
                        print("cleared one polygon");
                    }
                }
                else
                {
                    if (isRevExtruding)
                    {
                        // V�rifier si le clic gauche de la souris est enfonc� et si le dessin est en cours et que la souris n'est pas sur un objet UI
                        if (Input.GetMouseButtonDown(0) && !isChoosingAxis && !IsPointerOverUIObject())
                        {
                            // Obtenir la position de la souris dans l'espace du monde
                            axisPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            axisPosition.z = 0f; // S'assurer que la coordonn�e z est 0 pour l'espace 2D

                            isChoosingAxis = true;
                            isCheckingPolygon = true;
                        }
                    }

                    Debug.Log("Clic � l'ext�rieur de tous les polygones.");
                }
            }
        
            
        }
    }
    public void zcurve()
    {
        isZCurve = true;
    }
    private void CreateExtrusionPath(List<Vector3> polygonPoints, List<Vector3> Path, Transform parent)
    {
        Debug.Log("Entra a path");
        // Create the extrusion object from the prefab
        GameObject extrusionPath = Instantiate(extrusionPathPrefab);

        extrusionPath.transform.SetParent(parent.transform);
        meshes.Add(extrusionPath);

        // Get the ExtrudeBezier component and update the extrusion
        ExtrusionLongCurve extrusionpath = extrusionPath.GetComponent<ExtrusionLongCurve>();
        //extrusionpath.ExtrudeAlongCurve(polygonPoints, Path, parent, currentColor);
        extrusionpath.StartAnimation(polygonPoints, Path, extrusionpath.segmentCount, currentColor, 5);
    }
    private void CreateExtrusionAxe(List<Vector3> polygonPoints, Transform parent, Vector3 axis)
    {
        // Create the extrusion object from the prefab
        GameObject extrusionAxe = Instantiate(extrusionAxePrefab);

        extrusionAxe.transform.SetParent(parent.transform);
        meshes.Add(extrusionAxe);

        // Get the ExtrudeBezier component and update the extrusion
        ExtrusionAxe extrusionaxe = extrusionAxe.GetComponent<ExtrusionAxe>();
        //extrusionaxe.ExtrudeSurAxe(polygonPoints,parent, currentColor);
        extrusionaxe.StartAnimation(polygonPoints, extrusionaxe.segmentCount, axis, currentColor, 1);
    }
    private void CreateAndExtrudeObject(List<Vector3> curvePoints, Transform parent)
    {
        // Create the extrusion object from the prefab
        GameObject extrusionObject = Instantiate(extrusionPrefab);

        extrusionObject.transform.SetParent(parent.transform);
        meshes.Add(extrusionObject);

        // Get the ExtrudeBezier component and update the extrusion
        Extrusion extrusionScript = extrusionObject.GetComponent<Extrusion>();
        extrusionScript.UpdateExtrusion(curvePoints, currentColor, parent);
    }
    public void ClearOne()
    {
        clearOne = true;
    }

    // Method to activate extrusion mode
    public void ActivateExtrusion()
    {
        isExtruding = true;
    }
    public void ActivateAxeExtrusion()
    {
        Debug.Log("Extrude True");
        isExtrudingAxe = true;  
    }

    public void ActivateRevolutionExtrusion()
    {
        isRevExtruding = true;
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

    public void SetMaterialWood()
    {
        setMaterialMetal = false;
        setMaterialWood = true;

        woodBorder.enabled = true;
        metalBorder.enabled = false;
        noneBorder.enabled = false;
    }

    public void SetMaterialMetal()
    {
        setMaterialWood = false;
        setMaterialMetal = true;

        woodBorder.enabled = false;
        metalBorder.enabled = true;
        noneBorder.enabled = false;
    }

    public void SetMaterialNone()
    {
        setMaterialWood = false;
        setMaterialMetal = false;

        woodBorder.enabled = false;
        metalBorder.enabled = false;
        noneBorder.enabled = true;

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

                if (IsPointInPolygon(new Vector2(mousePos.x, mousePos.y), polygon) || isZCurve)
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

    // M�thode pour le raccordement C0
    public void C0Link(List<GameObject> polygonPoints)
    {
        // Cr�er un nouveau point au m�me emplacement que le dernier point du polygone existant
        GameObject newPoint = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        newPoint.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du nouveau point
        points.Add(newPoint); // Ajouter le point � la liste des points
    }

    // M�thode pour le raccordement C1
    public void C1Link(List<GameObject> polygonPoints)
    {
        // Cr�er deux nouveaux points
        GameObject newPoint1 = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        GameObject newPoint2 = Instantiate(pointPrefab, CalculateC1Position(polygonPoints), Quaternion.identity);

        newPoint1.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint2.tag = "controlPoint"; // Ajouter le tag "controlPoint"

        newPoint1.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du premier nouveau point
        newPoint2.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du deuxi�me nouveau point

        points.Add(newPoint1); // Ajouter le premier nouveau point � la liste des points
        points.Add(newPoint2); // Ajouter le deuxi�me nouveau point � la liste des points
    }

    // M�thode pour le raccordement C2
    public void C2Link(List<GameObject> polygonPoints)
    {
        // Cr�er trois nouveaux points
        GameObject newPoint1 = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        GameObject newPoint2 = Instantiate(pointPrefab, CalculateC1Position(polygonPoints), Quaternion.identity);
        GameObject newPoint3 = Instantiate(pointPrefab, CalculateC2Position(polygonPoints), Quaternion.identity);

        newPoint1.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint2.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint3.tag = "controlPoint"; // Ajouter le tag "controlPoint"

        newPoint1.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du premier nouveau point
        newPoint2.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du deuxi�me nouveau point
        newPoint3.GetComponent<Renderer>().material.color = currentColor; // D�finir la couleur du troisi�me nouveau point

        points.Add(newPoint1); // Ajouter le premier nouveau point � la liste des points
        points.Add(newPoint2); // Ajouter le deuxi�me nouveau point � la liste des points
        points.Add(newPoint3); // Ajouter le troisi�me nouveau point � la liste des points
    }

    // M�thode pour calculer la position du deuxi�me point pour le raccordement C1
    private Vector3 CalculateC1Position(List<GameObject> polygonPoints)
    {
        // Obtenir la position du dernier point du polygone
        Vector3 lastPointPosition = polygonPoints[polygonPoints.Count - 1].transform.position;

        // Obtenir la position du deuxi�me dernier point du polygone
        Vector3 secondLastPointPosition = polygonPoints[polygonPoints.Count - 2].transform.position;

        // Calculer la position du deuxi�me nouveau point
        // P1' = P0' + (Pn - Pn-1) -> Miroir du point Pn-1
        Vector3 c1Position = lastPointPosition + (lastPointPosition - secondLastPointPosition);

        return c1Position;
    }

    // M�thode pour calculer la position du troisi�me point pour le raccordement C2
    private Vector3 CalculateC2Position(List<GameObject> polygonPoints)
    {
        // Obtenir la position du dernier point du polygone
        Vector3 lastPointPosition = polygonPoints[polygonPoints.Count - 1].transform.position;

        // Obtenir la position du deuxi�me dernier point du polygone
        Vector3 secondLastPointPosition = polygonPoints[polygonPoints.Count - 2].transform.position;

        // Obtenir la position du troisi�me dernier point du polygone
        Vector3 thirdLastPointPosition = polygonPoints[polygonPoints.Count - 3].transform.position;

        // Calculer la position du troisi�me nouveau point
        // P2' = Pn+2 + 2*(P1' - Pn-1) -> Miroir des points Pn-1 et Pn-2
        Vector3 c2Position = lastPointPosition + (lastPointPosition - secondLastPointPosition) + (lastPointPosition - 2 * secondLastPointPosition + thirdLastPointPosition);

        return c2Position;
    }

    public void SetLinkType(int type)
    {
        isLinking = true;
        linkType = type;
    }
}
