using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class MatriceOperationsV2 : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject menuCanvas;
    public GameObject menuCanvasFill;
    public Button translateButton;
    public Button rotateButton;

    private Vector3 initialMousePosition;
    private Vector3 initialGameObjectPosition;

    [Header("Debugging variables")]
    public bool translating = false;
    public bool rotating = false;
    public bool scaling = false;
    public bool shearing = false;
    public bool multiply = false;
    private bool isDragging = false;
    public bool deletePoint = false;

    public GameObject selectedObject;
    private Vector3 lastMousePosition;

    [Header("Translation Settings")]
    public float translationSpeed = 5f;
    private GameObject translationIconInstance;
    public GameObject translationIconPrefab;
    public Vector3 translationIconOffset = new Vector3(5f, 5f, 0f);

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    private GameObject rotationIconInstance;
    public GameObject rotationIconPrefab;
    public Vector3 rotationIconOffset = new Vector3(0f, 0f, 0f);

    [Header("Scaling Settings")]
    public float scalingSpeed = 5f;
    private GameObject scalingIconInstance;
    public GameObject scalingIconPrefab;
    public Vector3 scalingIconOffset = new Vector3(5f, 5f, 0f);
    public Vector3 initialScale = new Vector3(1f, 1f, 1f);
    public float scaleSensitivity = 0.5F;

    [Header("Shearing Settings")]
    public float shearingSpeed = 5f;
    private GameObject shearingIconInstance;
    public GameObject shearingIconPrefab;
    public Vector3 shearingIconOffset = new Vector3(0f, 0f, 0f);

    [Header("Scripts")]
    public PointHandlerV2 pointHandler;

    private GameObject BezierCurveObj;

    public CasteljauV2 decasteljauScript;
    public ChaikinCurve chaikinScript;


    private new LineRenderer renderer;
    private List<GameObject> newPoly = new List<GameObject>();

    GameObject parentGO;
    private void Start()
    {
        translateButton.onClick.AddListener(StartTranslation);
        rotateButton.onClick.AddListener(StartRotation);
        HideMenu();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            HideMenu();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("controlPoint"))
                {
                    ShowMenu();
                    menuCanvasFill.SetActive(false);
                    initialMousePosition = Input.mousePosition;
                    initialGameObjectPosition = hit.transform.position;
                    selectedObject = hit.collider.gameObject;
                    lastMousePosition = Input.mousePosition;
                    isDragging = true;
                } 
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!Physics.Raycast(ray, out hit))
                {
                    HideMenu();
                }
            }
        }

        if (deletePoint)
        {
            rotating = false;
            translating = false;
            scaling = false;

            if (Input.GetMouseButton(0) && selectedObject != null)
            {
                newPoly = FindPolygon(selectedObject);
                Destroy(selectedObject);
                newPoly.Remove(selectedObject);
                UpdatePolygon(newPoly);

                selectedObject = null;
            }
        }

        if (translating)
        {
           
            if (Input.GetMouseButton(0) && selectedObject != null)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 mouseDelta = currentMousePosition - lastMousePosition;

                selectedObject.transform.Translate(mouseDelta * Time.deltaTime);

                lastMousePosition = currentMousePosition;

                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }

            if (selectedObject != null) { 
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging && selectedObject != null)
            {
                Vector3 offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(initialMousePosition);
                selectedObject.transform.position = initialGameObjectPosition + offset;
            }

            // Translation logic using keyboard keys 
            Vector3 translationVector = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
            {
                translationVector += Vector3.left;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.S))
            {
                translationVector += Vector3.down;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.D))
            {
                translationVector += Vector3.right;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.W))
            {
                translationVector += Vector3.up;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }

            if (translationVector.magnitude > 1f)
            {
                translationVector.Normalize();
            }

            selectedObject.transform.Translate(translationVector * translationSpeed * Time.deltaTime);

            // Show translation icon above the selected object
            if (translationIconPrefab != null && translationIconInstance == null)
            {
                translationIconInstance = Instantiate(translationIconPrefab, selectedObject.transform.position + translationIconOffset, Quaternion.identity);
            }
            else if (translationIconInstance != null)
            {
                translationIconInstance.transform.position = selectedObject.transform.position + translationIconOffset;
            }

            }
        } else
        {
            // Destroy translation icon when not in translation mode or no selected object
            if (translationIconInstance != null)
            {
                Destroy(translationIconInstance);
                translationIconInstance = null;
            }
        }

        if (rotating)
        {
            if (Input.GetMouseButton(0) && selectedObject != null && selectedObject.transform.parent != null)
            {
                parentGO = selectedObject.transform.parent.gameObject;
                float rotationAmount = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                parentGO.transform.Rotate(Vector3.forward, rotationAmount);

                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }

            // Handle keyboard input for rotation
            if (selectedObject != null && selectedObject.transform.parent != null)
            {
                parentGO = selectedObject.transform.parent.gameObject;

                GameObject[] points = new GameObject[parentGO.transform.childCount];
                for (int i = 0; i < parentGO.transform.childCount; i++)
                {
                    points[i] = parentGO.transform.GetChild(i).gameObject;
                }

                Vector3 barycenter = CalculateBarycenter(points);

                float rotationAmountKeyboard = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;


                parentGO.transform.RotateAround(barycenter, Vector3.forward, rotationAmountKeyboard);
                
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }

            // Show rotation icon above the selected object
            if (rotationIconPrefab != null && rotationIconInstance == null)
            {
                rotationIconInstance = Instantiate(rotationIconPrefab, selectedObject.transform.position + rotationIconOffset, Quaternion.identity);
            }
            else if (rotationIconInstance != null)
            {
                rotationIconInstance.transform.position = selectedObject.transform.position + rotationIconOffset;
            }

            
        }
        else
        {
            // Destroy rotation icon when not in rotation mode or no selected object
            if (rotationIconInstance != null)
            {
                Destroy(rotationIconInstance);
                rotationIconInstance = null;
            }
        }

        if (scaling)
        {

            if (Input.GetMouseButton(0) && selectedObject != null && selectedObject.transform.parent != null)
            {
                parentGO = selectedObject.transform.parent.gameObject;

                if (!isDragging)
                {
                    isDragging = true;
                    initialMousePosition = Input.mousePosition;
                    initialScale = parentGO.transform.localScale;
                }
                else
                {
                    Vector3 dragDelta = (Input.mousePosition - initialMousePosition) * scalingSpeed * Time.deltaTime;
                    float scalingFactor = 1.0f + dragDelta.magnitude * scaleSensitivity;
                    parentGO.transform.localScale = initialScale * scalingFactor;

                    newPoly = FindPolygon(selectedObject);
                    UpdatePolygon(newPoly);
                }
            }
            else
            {
                isDragging = false;
            }

            // Show scaling icon above the selected object
            if (scalingIconPrefab != null && scalingIconInstance == null)
            {
                scalingIconInstance = Instantiate(scalingIconPrefab, selectedObject.transform.position + scalingIconOffset, Quaternion.identity);
            }
            else if (scalingIconInstance != null)
            {
                scalingIconInstance.transform.position = selectedObject.transform.position + scalingIconOffset;
            }

            if (selectedObject != null && selectedObject.transform.parent != null)
            {
                parentGO = selectedObject.transform.parent.gameObject;
                // scaling logic using keyboard keys 
                float scalingFctr = 1.0f;
                if (Input.GetKey(KeyCode.S))
                {
                    scalingFctr -= scalingSpeed * Time.deltaTime;
                    newPoly = FindPolygon(selectedObject);
                    UpdatePolygon(newPoly);
                }
                if (Input.GetKey(KeyCode.W))
                {
                    scalingFctr += scalingSpeed * Time.deltaTime;
                    newPoly = FindPolygon(selectedObject);
                    UpdatePolygon(newPoly);
                }

                parentGO.transform.localScale *= scalingFctr;
            }
        }
        else
        {
            // Destroy scaling icon when not in scaling mode or no selected object
            if (scalingIconInstance != null)
            {
                Destroy(scalingIconInstance);
                scalingIconInstance = null;
            }
        }

        if (multiply)
        {
            rotating = false;
            translating = false;
            scaling = false;

            if (Input.GetMouseButton(0) && selectedObject != null)
            {
                newPoly = FindPolygon(selectedObject);
                GameObject cloned = Instantiate(selectedObject, selectedObject.transform.position, selectedObject.transform.rotation);
                int insertIndex = selectedObject.transform.GetSiblingIndex(); // Calcul de l'index pour ins�rer le clone juste apr�s l'objet original
                cloned.transform.SetSiblingIndex(insertIndex);
                newPoly.Insert(insertIndex, cloned); // Ins�rer le clone � l'index calcul�
                UpdatePolygon(newPoly);

                Debug.Log("Mul");
                selectedObject = null;
                multiply = false;
            }
        }

        if (shearing)
        {
            // Shearing logic using keyboard keys
            Vector3 shearVector = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
            {
                shearVector += Vector3.left;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.S))
            {
                shearVector += Vector3.down;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.D))
            {
                shearVector += Vector3.right;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }
            if (Input.GetKey(KeyCode.W))
            {
                shearVector += Vector3.up;
                newPoly = FindPolygon(selectedObject);
                UpdatePolygon(newPoly);
            }

            if (shearVector.magnitude > 1f)
            {
                shearVector.Normalize();
            }

            if (selectedObject != null)
            {
                Vector3 shearAmount = shearVector * shearingSpeed * Time.deltaTime;
                selectedObject.transform.position += shearAmount;
            }
        }
    }

    private Vector3 CalculateBarycenter(GameObject[] points)
    {
        Vector3 barycenter = Vector3.zero;
        foreach (GameObject point in points)
        {
            barycenter += point.transform.position;
        }
        barycenter /= points.Length;
        return barycenter;
    }

    public void UpdatePolygon(List<GameObject> polygonPoints)
    {
        // pour modifier les lignes du polygone
        LineRenderer renderer = selectedObject.transform.parent.GetComponent<LineRenderer>();

        renderer.positionCount = polygonPoints.Count;
        for (int i = 0; i < newPoly.Count; i++)
        {
            renderer.SetPosition(i, newPoly[i].transform.position);
            polygonPoints[i].transform.parent = selectedObject.transform.parent.transform;
        }
        renderer.sortingOrder = 1;

        //pour modifier la courbe
        BezierCurveObj = BezierCurveIsPresent(selectedObject.transform.parent.gameObject);
        if (BezierCurveObj != null)
        {
            print("curve present");
            if (BezierCurveObj.name == "CasteljauBezierCurve")
            {
                decasteljauScript.UpdateDecasteljau(newPoly, BezierCurveObj);
                print("casteljau function worked");
            }
            else if (BezierCurveObj.name == "ChaikinCurve")
            {
                chaikinScript.UpdateCurve(newPoly, BezierCurveObj);
                print("chaikin function worked");
            }
        }
        else
        {
            print("curve not present");
        }
    }

    public void ManualUpdatePolygon(List<GameObject> controlPoints)
    {
        if (controlPoints == null || controlPoints.Count < 2) return;

        GameObject polygonObj = controlPoints[0].transform.parent.gameObject;
        LineRenderer polyLine = polygonObj.GetComponent<LineRenderer>();
        if (polyLine == null)
        {
            Debug.LogWarning("Polygon GameObject does not have a LineRenderer.");
            return;
        }

        Vector3[] positions = controlPoints.Select(p => p.transform.position).ToArray();

        polyLine.positionCount = positions.Length + 1;
        for (int i = 0; i < positions.Length; i++)
        {
            polyLine.SetPosition(i, positions[i]);
        }
        // Close the loop
        polyLine.SetPosition(positions.Length, positions[0]);
    }


    private GameObject BezierCurveIsPresent(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name == "CasteljauBezierCurve" || child.gameObject.name == "PascalBezierCurve" || child.gameObject.name == "ChaikinCurve")
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public List<GameObject> FindPolygon(GameObject point)
    {
        foreach (List<GameObject> polygon in pointHandler.courbes)
        {
            foreach (GameObject polygonPoint in polygon)
            {
                if (polygonPoint == point)
                {
                    return polygon;
                }
            }
        }
        return null; 
    }

    public void ConnectPolygons(GameObject polygonA, GameObject polygonB)
    {
        if (polygonA == null || polygonB == null)
        {
            Debug.LogWarning("Both polygons must be provided.");
            return;
        }

        // Get control points of each polygon
        var pointsA = polygonA.GetComponentsInChildren<Transform>()
                       .Where(t => t.CompareTag("controlPoint"))
                       .Select(t => t).ToList();
        var pointsB = polygonB.GetComponentsInChildren<Transform>()
                       .Where(t => t.CompareTag("controlPoint"))
                       .Select(t => t).ToList();

        if (pointsA.Count == 0 || pointsB.Count == 0)
        {
            Debug.LogWarning("One polygon has no control points.");
            return;
        }

        // Move A to B (connect last of B to first of A)
        Vector3 endB = pointsB.Last().position;
        Vector3 startA = pointsA.First().position;
        Vector3 offset = endB - startA;

        // Apply offset to polygon A
        foreach (var t in pointsA)
            t.position += offset;

        // Update polygonA's LineRenderer after moving its points
        LineRenderer lr = polygonA.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = pointsA.Count;
            for (int i = 0; i < pointsA.Count; i++)
            {
                lr.SetPosition(i, pointsA[i].position);
            }
        }

        chaikinScript.UpdateCurve(pointsA.Select(t => t.gameObject).ToList(), BezierCurveIsPresent(polygonA));

        Debug.Log("Moved Polygon A to connect with Polygon B using offset {offset}");
    }

    public void ConnectPoints(GameObject pointA, GameObject pointB)
    {
        // Need to fix the Polygon Curve Update

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Both points must be provided.");
            return;
        }

        // Calculate offset from A to B
        Vector3 offset = pointB.transform.position - pointA.transform.position;

        // Move pointA to pointB
        pointA.transform.position += offset;

        // Find and update the polygon pointA belongs to
        UpdatePolygon(FindPolygon(pointA));

        chaikinScript.UpdateCurve(FindPolygon(pointA).ToList(), BezierCurveIsPresent(pointA.transform.parent.gameObject));

        Debug.Log($"Moved Point A to align with Point B using offset {offset}");
    }

    public void ConnectCurves(GameObject polygonA, GameObject polygonB)
    {
        if (polygonA == null || polygonB == null)
        {
            Debug.LogWarning("Both polygons must be provided.");
            return;
        }

        GameObject curveA = BezierCurveIsPresent(polygonA);
        GameObject curveB = BezierCurveIsPresent(polygonB);

        if (curveA == null || curveB == null)
        {
            Debug.LogWarning("Both polygons must have a Chaikin curve to connect.");
            return;
        }

        if (curveA.name != "ChaikinCurve" || curveB.name != "ChaikinCurve")
        {
            Debug.LogWarning("Both curves must be Chaikin curves.");
            return;
        }

        LineRenderer curveRendererA = curveA.GetComponent<LineRenderer>();
        LineRenderer curveRendererB = curveB.GetComponent<LineRenderer>();

        if (curveRendererA.positionCount == 0 || curveRendererB.positionCount == 0)
        {
            Debug.LogWarning("One of the curves has no points.");
            return;
        }

        Vector3 startA = curveRendererA.GetPosition(0);
        Vector3 endB = curveRendererB.GetPosition(curveRendererB.positionCount - 1);

        Vector3 offset = endB - startA;

        // Move polygonA's control points by the offset
        var pointsA = polygonA.GetComponentsInChildren<Transform>()
                       .Where(t => t.CompareTag("controlPoint"))
                       .ToList();

        foreach (var point in pointsA)
            point.position += offset;

        // Also update the Chaikin curve
        var chaikinCurveManager = FindObjectOfType<ChaikinCurve>();
        if (chaikinCurveManager != null)
        {
            chaikinCurveManager.UpdateCurve(pointsA.Select(t => t.gameObject).ToList(), curveA);
        }

        List<GameObject> pointObjsA = pointsA.Select(t => t.gameObject).ToList();
        ManualUpdatePolygon(pointObjsA); // Updates wireframe
        chaikinCurveManager.UpdateCurve(pointObjsA, curveA); // Updates Chaikin curve


        Debug.Log($"Moved Polygon A's Chaikin curve to connect with Polygon B. Offset: {offset}");
    }


    public void ConnectCurvePoints(GameObject pointA, GameObject pointB)
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Both points must be provided.");
            return;
        }

        GameObject polygonObjA = pointA.transform.parent.gameObject;
        GameObject polygonObjB = pointB.transform.parent.gameObject;

        ChaikinCurve chaikinCurveManager = FindObjectOfType<ChaikinCurve>();
        if (chaikinCurveManager == null)
        {
            Debug.LogWarning("ChaikinCurve manager not found.");
            return;
        }

        // Get Chaikin curves
        if (!chaikinCurveManager.TryGetCurveByPolygon(polygonObjA, out List<Vector3> curvePointsA, out GameObject curveObjA)
            || !chaikinCurveManager.TryGetCurveByPolygon(polygonObjB, out List<Vector3> curvePointsB, out _))
        {
            Debug.LogWarning("One or both curves not found in registry.");
            return;
        }

        // Target is the START of polygon B's Chaikin curve
        Vector3 targetPosition = curvePointsB.First();

        // Get control points of polygon A
        List<GameObject> polygonAPoints = chaikinCurveManager.GetControlPointsForPolygon(polygonObjA);
        if (polygonAPoints == null || polygonAPoints.Count < 1)
        {
            Debug.LogWarning("Polygon A control points are missing or invalid.");
            return;
        }

        GameObject lastControlPoint = polygonAPoints[polygonAPoints.Count - 1];

        // Iteratively adjust last control point to align curve end with target
        const int maxIterations = 10;
        const float threshold = 0.001f;

        for (int i = 0; i < maxIterations; i++)
        {
            // Recalculate A's curve
            List<Vector3> updatedCurve = chaikinCurveManager.GetChaikinCurvePoints(polygonAPoints, chaikinCurveManager.iterations);
            Vector3 currentEnd = updatedCurve.Last();

            Vector3 diff = targetPosition - currentEnd;

            if (diff.magnitude < threshold)
                break;

            // Move the control point a bit closer
            lastControlPoint.transform.position += diff;
        }

        // Final update to make sure curve and polygon visuals are synced
        chaikinCurveManager.UpdateCurve(polygonAPoints, curveObjA);
        ManualUpdatePolygon(polygonAPoints); // Also update polygon's outline

        Debug.Log("Connected Polygon A's curve end to Polygon B's curve start.");
    }




    private void ShowMenu()
    {
        menuCanvas.SetActive(true);
    }

    private void HideMenu()
    {
        menuCanvas.SetActive(false);
        translating= false;
        rotating = false;
        deletePoint = false;
        scaling = false;
        shearing = false;
        multiply = false;
    }

    public void StartTranslation()
    {
        translating=true; 
        rotating = false;
        scaling = false;
        deletePoint = false;
        shearing = false;
        multiply = false;
        Debug.Log("Translation started");
    }

    public void StartRotation()
    {
        rotating = true;
        scaling = false;
        translating = false;
        deletePoint = false;
        shearing = false;
        multiply = false;
        Debug.Log("Rotation started");
    }

    public void StartScale()
    {
        scaling = true;
        rotating = false;
        translating = false;
        shearing = false;
        deletePoint = false;
        multiply = false;
        Debug.Log("Scaling started");
    }

    public void DeletePoint()
    {
        deletePoint = true;
        scaling = false;
        rotating = false;
        translating = false;
        shearing = false;
        multiply = false; 
        print("delele point");
    }

    public void StartShearing()
    {
        shearing = true;
        deletePoint = false;
        scaling = false;
        rotating = false;
        translating = false;
        multiply = false;
        print("delele point");
    }

    public void StartMultiply()
    {
        multiply = true;
        shearing = false;
        deletePoint = false;
        scaling = false;
        rotating = false;
        translating = false;
        print("multiply point");
    }
}
