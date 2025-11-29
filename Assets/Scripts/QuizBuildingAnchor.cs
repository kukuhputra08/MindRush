using UnityEngine;

public class QuizBuildingAnchor : MonoBehaviour
{
    [Header("Prompt [F] (opsional)")]
    public GameObject promptUI;                 // world-space canvas kecil "Tekan [F]"
    [Range(2f, 25f)] public float interactRadius = 6f;

    Transform player;
    bool isActive;
    bool inRange;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
        SetActive(false);
    }

    public void SetActive(bool value)
    {
        isActive = value;
        inRange = false;
        if (promptUI) promptUI.SetActive(false);
        enabled = value; // kalau nonaktif, Update() berhenti
    }

    void Update()
    {
        if (!isActive || !player) return;

        float d = Vector3.Distance(player.position, transform.position);
        bool nowInRange = d <= interactRadius;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (promptUI) promptUI.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            QuizControllerLite.Instance?.TryStartQuizAt(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
