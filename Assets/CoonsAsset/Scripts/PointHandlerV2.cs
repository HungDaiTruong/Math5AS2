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
    public List<GameObject> lines = new List<GameObject>(); // Liste pour stocker les objets ligne (polygones)

    public bool drawing = false; // Indique si le dessin est en cours
    public Color currentColor = Color.red; // Couleur actuellement s�lectionn�e

    public bool isCheckingPolygon = false; // Indique si la v�rification des polygones est active
    public List<List<GameObject>> courbes = new List<List<GameObject>>();
    public List<GameObject> meshes = new List<GameObject>();

    List<Vector3> LastCurvePoints = new List<Vector3>();

    public Drawing drawable;
    public bool clearOne = false;

    public bool isLinking = false;
    public int linkType;

    private GameObject firstPolygonToConnect = null;
    public bool isConnectingPolygons = false;

    private GameObject firstPointToConnect = null;
    public bool isConnectingPoints = false;

    public bool isConnectingCurves = false;

    public bool isConnectingCurvePoints = false;

    public List<GameObject> coonSelectedCurves = new List<GameObject>();
    public bool isSelectingCoonCurves = false;


    public Vector3 axisPosition = Vector3.zero;

    private List<Vector3> curvePoints;

    public CasteljauV2 decasteljauScript;
    public ChaikinCurve chaikinScript;
    public MatriceOperationsV2 matrixOperation;

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

                    if (decasteljauScript.decasteljau)
                    {
                        decasteljauScript.DrawBezierCurve(polygonPoints, insidePolygon);
                        decasteljauScript.decasteljau = false;
                        print("casteljau function worked");

                        List<Vector3> curvePoints = decasteljauScript.GetCurvePoints(polygonPoints);
                        LastCurvePoints = curvePoints;
                    }
                    else if(chaikinScript.chaikin)
                    {
                        chaikinScript.DrawChaikinCurve(polygonPoints, insidePolygon);
                        chaikinScript.chaikin = false;
                        print("chaikin function worked");

                        List<Vector3> curvePoints = chaikinScript.GetChaikinCurvePoints(polygonPoints, chaikinScript.iterations);
                        LastCurvePoints = curvePoints;
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
                    else if (isConnectingPolygons)
                    {
                        if (firstPolygonToConnect == null)
                        {
                            firstPolygonToConnect = insidePolygon;
                            StartCheckingPolygon(); // Wait for second click
                            Debug.Log("Now click on the polygon to connect to " + firstPolygonToConnect);
                        }
                        else
                        {
                            GameObject secondPolygon = insidePolygon;
                            matrixOperation.ConnectPolygons(firstPolygonToConnect, secondPolygon);

                            // Reset connection state
                            firstPolygonToConnect = null;
                            isConnectingPolygons = false;
                            insidePolygon = null;
                            Debug.Log("Polygons connected.");
                        }
                        return;
                    }
                    else if (isConnectingCurves)
                    {
                        if (firstPolygonToConnect == null)
                        {
                            firstPolygonToConnect = insidePolygon;
                            StartCheckingPolygon(); // Wait for second click
                            Debug.Log("Now click on the polygon to connect to " + firstPolygonToConnect);
                        }
                        else
                        {
                            GameObject secondPolygon = insidePolygon;
                            matrixOperation.ConnectCurves(firstPolygonToConnect, secondPolygon);

                            // Reset connection state
                            firstPolygonToConnect = null;
                            isConnectingCurves = false;
                            insidePolygon = null;
                            Debug.Log("Curves connected.");
                        }
                        return;
                    }
                    else if (isSelectingCoonCurves)
                    {
                        isCheckingPolygon = true;

                        if (!coonSelectedCurves.Contains(insidePolygon))
                        {
                            coonSelectedCurves.Add(insidePolygon);
                            Debug.Log($"Selected curve {coonSelectedCurves.Count}/4 for Coon patch.");
                        }

                        if (coonSelectedCurves.Count == 4)
                        {
                            isSelectingCoonCurves = false;
                            isCheckingPolygon = false;

                            // Call the function to create the Coon mesh
                            chaikinScript.CreateCoonMesh(coonSelectedCurves);
                            coonSelectedCurves.Clear(); // reset for next use
                            Debug.Log("Coon patch created.");
                        }
                        return;
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
            }

            if (isConnectingPoints && Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.CompareTag("controlPoint"))
                    {
                        if (firstPointToConnect == null)
                        {
                            firstPointToConnect = hit.collider.gameObject;
                            Debug.Log("Now click on the point to connect to " + hit.collider.gameObject);
                        }
                        else
                        {
                            GameObject secondPoint = hit.collider.gameObject;
                            matrixOperation.ConnectPoints(firstPointToConnect, secondPoint);

                            // Reset connection state
                            firstPointToConnect = null;
                            isConnectingPoints = false;
                            Debug.Log("Points connected.");
                        }
                        return;
                    }
                }
            }
            else if (isConnectingCurvePoints && Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.CompareTag("controlPoint"))
                    {
                        if (firstPointToConnect == null)
                        {
                            firstPointToConnect = hit.collider.gameObject;
                            Debug.Log("Now click on the point to connect to " + hit.collider.gameObject);
                        }
                        else
                        {
                            GameObject secondPoint = hit.collider.gameObject;
                            matrixOperation.ConnectCurvePoints(firstPointToConnect, secondPoint);

                            // Reset connection state
                            firstPointToConnect = null;
                            isConnectingCurvePoints = false;
                            Debug.Log("Curve Points connected.");
                        }
                        return;
                    }
                }
            }
        }
    }

    public void ClearOne()
    {
        clearOne = true;
    }

    public void ActivateConnectPolygons()
    {
        isConnectingPolygons |= true;
    }

    public void ActivateConnectPoints()
    {
        isConnectingPoints |= true;
    }

    public void ActivateConnectCurves()
    {
        isConnectingCurves |= true;
    }

    public void ActivateConnectCurvePoints()
    {
        isConnectingCurvePoints |= true;
    }
    public void ActivateCoonMesh()
    {
        StartCoonCurveSelection();
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

    public void StartCoonCurveSelection()
    {
        coonSelectedCurves.Clear();
        isSelectingCoonCurves = true;
        isCheckingPolygon = true;
        Debug.Log("Select 4 Chaikin curves (click on their polygons).");
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
