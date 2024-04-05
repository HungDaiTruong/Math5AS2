using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointHandler : MonoBehaviour
{
    public GameObject pointPrefab; // Prefab for the point object
    private List<Vector3> points = new List<Vector3>(); // List to store the points
    private bool drawing = false; // Flag to indicate if drawing is in progress
    private Color currentColor = Color.red; // Current color selected

    void Update()
    {
        // Check if left mouse button is clicked
        if (Input.GetMouseButtonDown(0) && drawing)
        {
            // Get the mouse position in world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // Ensure z coordinate is 0 for 2D space

            // Instantiate a point object at the mouse position
            GameObject point = Instantiate(pointPrefab, mousePos, Quaternion.identity);
            point.GetComponent<Renderer>().material.color = currentColor; // Set the color of the point
            points.Add(mousePos); // Add the point to the list
        }

        // Check if right mouse button is clicked
        if (Input.GetMouseButtonDown(1) && drawing)
        {
            drawing = false; // Stop drawing
            ConnectPoints(); // Connect the points to form a polygon
        }
    }

    // Method to connect the points to form a polygon
    void ConnectPoints()
    {
        // Ensure at least 3 points are available to form a polygon
        if (points.Count < 3)
        {
            Debug.LogWarning("Not enough points to form a polygon.");
            return;
        }

        // Create a new GameObject to hold the lines
        GameObject lines = new GameObject("Lines");

        // Add a LineRenderer component to draw the lines
        LineRenderer lineRenderer = lines.AddComponent<LineRenderer>();
        lineRenderer.positionCount = points.Count; // Set the number of positions
        lineRenderer.startWidth = 0.1f; // Set the width of the line
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = true; // Connect the last point to the first point

        // Set the positions for the LineRenderer
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        // Clear the list of points for the next drawing session
        points.Clear();
    }

    // Method to set the current drawing color
    public void SetColorRed()
    {
        currentColor = Color.red;
        drawing = true; // Start drawing when color is selected
        Debug.Log("Red");
    }

    public void SetColorGreen()
    {
        currentColor = Color.green;
        drawing = true; // Start drawing when color is selected
        Debug.Log("Green");
    }

    public void SetColorBlue()
    {
        currentColor = Color.blue;
        drawing = true; // Start drawing when color is selected
        Debug.Log("Blue");
    }
}
