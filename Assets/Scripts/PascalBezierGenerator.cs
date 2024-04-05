using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PascalBezierGenerator : MonoBehaviour
{
    public Drawing drawingScript; // Référence au script Drawing
    public List<Vector2Int> lastPolygonPixelVertices; // Derniers points de polygones
    public List<List<Vector2Int>> polygons; // Liste des polygones

    public List<Vector2> controlPoints = new List<Vector2>(); // Points de contrôle de la courbe de Bézier
    public List<List<int>> pascalTriangle = new List<List<int>>(); // Triangle de Pascal

    public LineRenderer lineRenderer; // Référence au LineRenderer pour tracer la courbe de Bézier
    public float step = 0.01f; // Pas pour tracer la courbe de Bézier

    void Start()
    {
        drawingScript = GetComponent<Drawing>(); // Récupérer le script Drawing attaché à ce GameObject

        // Initialiser les listes de points de polygones et de polygones en utilisant les données du script Drawing
        lastPolygonPixelVertices = drawingScript.lastPolygonPixelVertices;
        polygons = drawingScript.polygons;

        // Générer le triangle de Pascal
        GeneratePascalTriangle();

        // Récupérer le LineRenderer attaché à ce GameObject
        lineRenderer = GetComponent<LineRenderer>();
    }

    void GeneratePascalTriangle()
    {
        // Initialisation du triangle de Pascal avec les valeurs de la première ligne
        pascalTriangle.Add(new List<int> { 1 });

        // Générer les lignes suivantes du triangle de Pascal
        for (int i = 1; i < controlPoints.Count; i++)
        {
            List<int> currentRow = new List<int>();
            List<int> previousRow = pascalTriangle[i - 1];

            // La première valeur de chaque ligne est 1
            currentRow.Add(1);

            // Calculer les valeurs suivantes en utilisant la formule de Pascal
            for (int j = 1; j < i; j++)
            {
                int value = previousRow[j - 1] + previousRow[j];
                currentRow.Add(value);
            }

            // La dernière valeur de chaque ligne est 1
            currentRow.Add(1);

            // Ajouter la ligne actuelle au triangle de Pascal
            pascalTriangle.Add(currentRow);
        }
    }

    Vector2 CalculateBezierPoint(float t)
    {
        int n = controlPoints.Count - 1;
        Vector2 point = Vector2.zero;
        for (int i = 0; i <= n; i++)
        {
            int coeff = pascalTriangle[n][i];
            float term = coeff * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
            point += controlPoints[i] * term;
        }
        return point;
    }

    void Update()
    {
        // Gérer le clic pour générer la courbe de Bézier
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsClickInControlPolygon(mousePosition))
            {
                DrawBezierCurve();
            }
        }

        // Mettre à jour le tracé de la courbe de Bézier si les points de contrôle sont modifiés
        if (controlPoints.Count > 0)
        {
            DrawBezierCurve();
        }
    }

    bool IsClickInControlPolygon(Vector2 mousePosition)
    {
        // Implémentez ici la logique pour vérifier si le clic est dans le polygone des points de contrôle
        // Vous pouvez utiliser des techniques de géométrie pour déterminer si le point cliqué est à l'intérieur du polygone
        return true;
    }

    void DrawBezierCurve()
    {
        lineRenderer.positionCount = 0; // Réinitialiser les positions du LineRenderer

        // Dessiner la courbe de Bézier en utilisant CalculateBezierPoint() pour chaque valeur de t
        for (float t = 0; t <= 1; t += step)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, CalculateBezierPoint(t));
        }
    }
}
