using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    private GameObject selectedObject;
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
                int insertIndex = selectedObject.transform.GetSiblingIndex(); // Calcul de l'index pour insérer le clone juste après l'objet original
                cloned.transform.SetSiblingIndex(insertIndex);
                newPoly.Insert(insertIndex, cloned); // Insérer le clone à l'index calculé
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

    private void UpdatePolygon(List<GameObject> selectedPolygon)
    {
        //pour modifier les lignes du polygone
        renderer = selectedObject.transform.parent.GetComponent<LineRenderer>();

        renderer.positionCount = newPoly.Count;
        for (int i = 0; i < newPoly.Count; i++)
        {
            renderer.SetPosition(i, newPoly[i].transform.position);
            newPoly[i].transform.parent = selectedObject.transform.parent.transform;
        }
        renderer.sortingOrder = 1; 

        //pour modifier la courbe
        BezierCurveObj = BezierCurveIsPresent(selectedObject.transform.parent.gameObject);
        if (BezierCurveObj != null)
        {

/*            print("curve present");
            if (BezierCurveObj.name == "CasteljauBezierCurve")
            {
                decasteljauScript.UpdateDecasteljau(newPoly, BezierCurveObj);
                print("casteljau function worked");
            } else if (BezierCurveObj.name == "PascalBezierCurve")
            {
                pascalScript.UpdateCurve(newPoly, BezierCurveObj);
                print("pascal function worked");
            }*/
                
            
        } else
        {
            print("curve not present");
        }
        
    }

    private GameObject BezierCurveIsPresent(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name == "CasteljauBezierCurve" || child.gameObject.name == "PascalBezierCurve")
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
