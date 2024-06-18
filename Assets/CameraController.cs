using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private float currentZoom = 10f;

    private bool vueFace = false;
    private bool vueProfile=false;

    public Vector3 PositionFace = new Vector3(0, 0, -10f);
    public Vector3 RotationFace = new Vector3(0, 0, 0);

    public Vector3 PositionProfile = new Vector3(10f, 0, 0);
    public Vector3 RotationProfile = new Vector3(0, -90f, 0);

    private void Start()
    {
        PositionFace = new Vector3(0, 0, -10f);
        RotationFace = new Vector3(0, 0, 0);

        PositionProfile = new Vector3(0, 0, 0);
        RotationProfile = new Vector3(0, -90f, 0);
}
    void Update()
    {
        if (vueProfile)
        {
            //rotate to profile
            transform.position = PositionProfile;
            transform.rotation = Quaternion.Euler(RotationProfile);
            vueProfile = false;
        } else if (vueFace)
        {
            //rotate to face
            transform.position = PositionFace;
            transform.rotation = Quaternion.Euler(RotationFace);
            vueFace = false;
        } else
        {
            // Handle movement
            float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            transform.Translate(moveX, moveY, scroll, Space.Self);

            // Handle rotation
            if (Input.GetMouseButton(1)) // Right mouse button for looking around
            {
                float rotationX = Input.GetAxis("Mouse X") * lookSpeed;
                float rotationY = -Input.GetAxis("Mouse Y") * lookSpeed;
                transform.eulerAngles += new Vector3(rotationY, rotationX, 0);
            }

            // Handle zoom
            //float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            //currentZoom -= scroll;
            //currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            //Camera.main.fieldOfView = currentZoom;
        }
    }

    public void VueDeFace()
    {
        vueFace = true;
    }

    public void VueDeProfile()
    {
        vueProfile = true;
    }
}
