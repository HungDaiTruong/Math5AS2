using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drawing : MonoBehaviour
{

    public List<Vector2> polygonVertices = new List<Vector2>();
    
    public Camera mainCamera;
    public float maxZoom = 5f;
    public float minZoom = 1f;
    public float zoomSpeed = 1f;
    public Transform textureTransform;
    private bool isDragging = false;
    private Vector3 offset;

    public static Color Pen_Colour = Color.blue;

    public LayerMask Drawing_Layers;

    public bool Reset_Canvas_On_Play = true;
    public Color Reset_Colour = new Color(0, 0, 0, 0);

    public static Drawing drawable;
    Sprite drawable_sprite;
    Texture2D drawable_texture;
    Color[] clean_colours_array;
    Color32[] cur_colors;

    bool is_drawing_line = false;
    Vector2 start_point;
    Vector2 end_point;
    Vector2 first_point;

    // Method to set pen brush color to red
    public void SetPenBrushRed()
    {
        Pen_Colour = red;
    }

    // Method to set pen brush color to blue
    public void SetPenBrushBlue()
    {
        Pen_Colour = blue;
    }

    // Method to set pen brush color to green
    public void SetPenBrushGreen()
    {
        Pen_Colour = green;
    }

    private Color red = Color.red;
    private Color blue = Color.blue;
    private Color green = Color.green;
    private Color white = Color.white;
    public Color[] penColors => new Color[] { red, green, blue, white };
    public int W => (int)drawable_sprite.rect.width;
    public int H => (int)drawable_sprite.rect.height;
    // Method to set pen brush color to black
    public void SetPenBrushWhite()
    {
        Pen_Colour = white;
    }

    // Function to mark a pixel for color change
    public void MarkPixelToChange(int x, int y, Color color)
    {
        if (!TryGetArrayPos(x, y, out int array_pos))
            return;
        cur_colors[array_pos] = color;
    }

    private bool TryGetArrayPos(int x, int y, out int p)
    {
        if (y < 0 || y >= H || x < 0 || x >= W)
        {
            p = -1;
            return false;
        }
        p = y * (int)drawable_sprite.rect.width + x;
        return true;
    }
    public Color32? GetCurColor(int x, int y)
    {
        if (!TryGetArrayPos(x, y, out int array_pos))
            return null;
        return cur_colors[array_pos];
    }
    public bool TryGetCurColor(int x, int y, out Color32 color)
    {
        var c = GetCurColor(x, y);
        color = c.GetValueOrDefault();
        if (!c.HasValue)
            return false;
        return true;
    }

    // Function to draw a line between two points
    void DrawLine(Vector2 start, Vector2 end)
    {
        if (!is_drawing_line)
        {
            polygonVertices.Add(start);
        }
        
        cur_colors = drawable_texture.GetPixels32();
        Vector2Int start_pixel = WorldToPixelCoordinates(start);
        Vector2Int end_pixel = WorldToPixelCoordinates(end);

        Vector2Int delta = end_pixel - start_pixel;

        if (delta == Vector2Int.zero)
            return;

        int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

        for (int i = 0; i <= steps; i++)
        {
            Vector2Int pixel = start_pixel + (delta * i / steps);
            MarkPixelToChange(pixel.x, pixel.y, Pen_Colour);
        }

        // Ajoute toujours le point final
        polygonVertices.Add(end);

        ApplyMarkedPixelChanges();

        // Set the last point as the first point for the next line
        start_point = end;
        is_drawing_line = true;
    }

    // Function to apply marked pixel changes to the texture
    public void ApplyMarkedPixelChanges()
    {
        drawable_texture.SetPixels32(cur_colors);
        drawable_texture.Apply();
    }

    // Function to convert world coordinates to pixel coordinates
    Vector2Int WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector3 local_pos = transform.InverseTransformPoint(world_position);
        float pixelWidth = W;
        float pixelHeight = H;
        float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;
        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;
        return new Vector2Int(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
    }

    // Function to check if the mouse pointer is over a UI object
    bool IsPointerOverUIObject()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Function to get the mouse position in world coordinates
    Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void Update()
    {
        float zoomValue = Input.GetAxis("Mouse ScrollWheel");

        Vector3 zoomPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float newSize = mainCamera.orthographicSize - zoomValue * zoomSpeed;

        newSize = Mathf.Clamp(newSize, minZoom, maxZoom);

        float newSizeChange = newSize - mainCamera.orthographicSize;
        mainCamera.orthographicSize = newSize;

        mainCamera.transform.position += (zoomPoint - mainCamera.transform.position) * newSizeChange / mainCamera.orthographicSize;

        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
            Vector3 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            offset = textureTransform.position - clickPosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(2))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            textureTransform.position = mouseWorldPosition + offset;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(GetMouseWorldPosition(), Drawing_Layers.value);
            if (hit != null && hit.transform != null || !IsPointerOverUIObject())
            {
                if (!is_drawing_line)
                {
                    first_point = GetMouseWorldPosition();
                    start_point = first_point;
                    is_drawing_line = true;
                }
                else
                {
                    end_point = GetMouseWorldPosition();
                    DrawLine(start_point, end_point);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (is_drawing_line)
            {
                end_point = first_point;
                DrawLine(start_point, end_point);
                is_drawing_line = false;
            }
        }
    }

    // Function to reset the canvas
    public void ResetCanvas()
    {
        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }

    void Awake()
    {
        drawable = this;

        drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
        drawable_texture = drawable_sprite.texture;
        cur_colors = drawable_texture.GetPixels32();

        clean_colours_array = new Color[W * H];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Reset_Colour;

        if (Reset_Canvas_On_Play)
            ResetCanvas();
    }

    
}
