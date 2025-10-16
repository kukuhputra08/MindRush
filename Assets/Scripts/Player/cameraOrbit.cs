using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;        // drag: Player (atau empty child di torso)
    public float targetHeight = 2f;

    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    public float yawSpeed = 180f;  // mouse X
    public float pitchSpeed = 120f;  // mouse Y
    public float minPitch = -30f;
    public float maxPitch = 60f;

    float yaw, pitch;

    void Start()
    {
        if (!target) { enabled = false; return; }

        // ambil rotasi awal kamera relatif target
        Vector3 focus = target.position + Vector3.up * targetHeight;
        Vector3 dir = (transform.position - focus).normalized;
        distance = Mathf.Clamp(Vector3.Distance(transform.position, focus), minDistance, maxDistance);

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        Vector3 e = look.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;

        // kunci kursor biar enak lihat
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // mouse → kamera saja (POV)
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * yawSpeed * Time.deltaTime;
        pitch -= my * pitchSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // zoom (scroll)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
            distance = Mathf.Clamp(distance - scroll * 4f, minDistance, maxDistance);

        // posisi & rotasi kamera
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * targetHeight;

        transform.position = focus + rot * new Vector3(0f, 0f, -distance);
        transform.rotation = rot;
    }
}
