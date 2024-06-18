using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private float currentZoom = 10f;

    void Update()
    {
        // Handle movement
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(moveX, 0, moveZ, Space.Self);

        // Handle rotation
        if (Input.GetMouseButton(1)) // Right mouse button for looking around
        {
            float rotationX = Input.GetAxis("Mouse X") * lookSpeed;
            float rotationY = -Input.GetAxis("Mouse Y") * lookSpeed;
            transform.eulerAngles += new Vector3(rotationY, rotationX, 0);
        }

        // Handle zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom -= scroll;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        Camera.main.fieldOfView = currentZoom;
    }
}
