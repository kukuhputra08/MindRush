using UnityEngine;
using TMPro;

public class AnchorArrowPointer : MonoBehaviour
{
    public Camera mainCam;                 // drag Main Camera
    public RectTransform arrow;            // RectTransform image panah (objek ini)
    public TextMeshProUGUI distanceText;   // TMP tulisan jarak
    public float edgePadding = 40f;

    void Reset()
    {
        mainCam = Camera.main;
        arrow = GetComponent<RectTransform>();
        distanceText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void LateUpdate()
    {
        var ctrl = QuizControllerLite.Instance;
        if (!ctrl || !arrow || !mainCam) return;

        Transform target = ctrl.CurrentTarget;
        if (!target)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);

        // jarak
        float dist = Vector3.Distance(mainCam.transform.position, target.position);
        if (distanceText) distanceText.text = $"{dist:0} m";

        // posisi target di layar
        Vector3 sp = mainCam.WorldToScreenPoint(target.position);
        bool behind = sp.z < 0f;
        if (behind)
        {
            // balik kalau di belakang kamera
            sp.x = Screen.width - sp.x;
            sp.y = Screen.height - sp.y;
            sp.z = 0.1f;
        }

        // clamp ke tepi layar
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 posScreen = new Vector2(sp.x, sp.y);
        bool onScreen = !behind &&
                        sp.x >= 0 && sp.x <= Screen.width &&
                        sp.y >= 0 && sp.y <= Screen.height;

        if (!onScreen)
        {
            Vector2 dir = (posScreen - center).normalized;
            posScreen = center + dir * (Mathf.Min(center.x, center.y) - edgePadding);
        }
        else
        {
            posScreen = new Vector2(
                Mathf.Clamp(posScreen.x, edgePadding, Screen.width - edgePadding),
                Mathf.Clamp(posScreen.y, edgePadding, Screen.height - edgePadding)
            );
        }

        // rotasi panah mengarah ke target di ruang layar
        Vector2 dirScreen = (posScreen - center).normalized;
        float ang = Mathf.Atan2(dirScreen.y, dirScreen.x) * Mathf.Rad2Deg - 90f;
        arrow.rotation = Quaternion.Euler(0, 0, ang);

        // taruh panah di canvas overlay
        arrow.anchoredPosition = ScreenToCanvas(posScreen, (RectTransform)arrow.parent);
    }

    Vector2 ScreenToCanvas(Vector2 screen, RectTransform canvasRT)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screen, null, out var local);
        return local;
    }
}
