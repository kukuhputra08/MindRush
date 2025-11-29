using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 12f;
    public Vector2 waitRange = new Vector2(2f, 6f);
    public float repathInterval = 1.5f;

    [Header("Animator")]
    public Animator anim;
    public string speedParam = "Speed";

    NavMeshAgent agent;
    float nextRepath;
    float waitTimer;
    bool waiting;
    int areaMask = NavMesh.AllAreas;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // DEBUG: periksa apakah agent sudah berada di NavMesh
        if (agent.isOnNavMesh)
        {
            Debug.Log($"✅ {name} berada di atas NavMesh saat Start()");
        }
        else
        {
            Debug.LogWarning($"⚠️ {name} TIDAK di atas NavMesh! Pastikan spawn point-nya berada di area biru NavMesh.");
        }
    }

    void OnEnable()
    {
        PickNewDestination();
        nextRepath = Time.time + repathInterval;
    }

    void Update()
    {
        // DEBUG: log singkat setiap beberapa detik
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"{name} | isOnNavMesh={agent.isOnNavMesh} | hasPath={agent.hasPath} | velocity={agent.velocity.magnitude:F2}");
        }

        if (anim)
        {
            anim.SetFloat(speedParam, agent.velocity.magnitude);
        }

        if (!waiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waiting = true;
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                PickNewDestination();
            }
        }

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                PickNewDestination();
            }
        }
    }

    void PickNewDestination()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"⚠️ {name} tidak berada di atas NavMesh, tidak bisa mencari tujuan baru!");
            return;
        }

        if (TryGetRandomPoint(transform.position, wanderRadius, out Vector3 point))
        {
            agent.SetDestination(point);
            Debug.Log($"{name} → tujuan baru: {point}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {name} gagal menemukan titik di NavMesh untuk jalan.");
        }
    }

    bool TryGetRandomPoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * radius;
            Vector3 candidate = origin + new Vector3(rnd.x, 0f, rnd.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, areaMask))
            {
                result = hit.position;
                return true;
            }
        }
        result = origin;
        return false;
    }
}
