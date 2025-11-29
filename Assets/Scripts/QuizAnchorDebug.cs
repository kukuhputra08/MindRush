using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class QuizAnchorDebug : MonoBehaviour
{
    [Header("Detect (tanpa collider)")]
    public float interactRadius = 6f;
    public string playerTag = "Player";

    [Header("Visual Debug")]
    public bool drawGizmo = true;         // lingkaran radius di Scene
    public bool showPrompt = true;        // tulisan "Tekan F" saat dekat
    public Vector3 promptOffset = new Vector3(0, 1.8f, 0);

    [Header("Logging")]
    public bool logEnterExit = true;      // log saat masuk/keluar radius
    public bool logNavMeshCheck = true;   // periksa posisi anchor di NavMesh saat Start

    Transform player;
    bool inRange;
    Camera cam;

    void Start()
    {
        if (!Application.isPlaying) return;

        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
        cam = Camera.main;

        if (logNavMeshCheck)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
                Debug.Log($"✅ [{name}] di atas NavMesh @ {hit.position}");
            else
                Debug.LogWarning($"⚠️ [{name}] BUKAN di atas NavMesh! Geser anchor ke area biru.");
        }
    }

    void Update()
    {
        if (!Application.isPlaying || !player) return;

        float d = Vector3.Distance(player.position, transform.position);
        bool nowInRange = d <= interactRadius;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (logEnterExit)
            {
                if (inRange) Debug.Log($"➡️  [{name}] ENTER radius ({d:F1} m)");
                else Debug.Log($"⬅️  [{name}] EXIT radius ({d:F1} m)");
            }
        }

        // Debug: deteksi tombol F (hanya log, belum buka quiz)
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"🟦 [{name}] F ditekan di dalam radius. (Siap trigger kuis)");
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    void OnGUI()
    {
        if (!Application.isPlaying || !showPrompt || !inRange || cam == null) return;

        Vector3 world = transform.position + promptOffset;
        Vector3 screen = cam.WorldToScreenPoint(world);
        if (screen.z > 0)
        {
            var size = new Vector2(150, 24);
            var pos = new Rect(screen.x - size.x / 2f, Screen.height - screen.y - size.y, size.x, size.y);
            GUI.Box(pos, "Tekan [F] untuk Kuis");
        }
    }
}
