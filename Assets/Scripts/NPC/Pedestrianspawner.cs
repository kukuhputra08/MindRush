using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    public List<GameObject> prefabs;  // Masukkan 13 prefab NPC di sini
    public int count = 50;            // Jumlah NPC yang mau muncul

    [Header("Spawn Area")]
    public Vector3 center = Vector3.zero;   // Titik tengah area
    public Vector3 halfExtents = new Vector3(200, 0, 200); // Ukuran setengah area (ubah sesuai peta)

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            if (TrySample(center, halfExtents, out var pos))
            {
                var pf = prefabs[Random.Range(0, prefabs.Count)];
                var go = Instantiate(pf, pos, Quaternion.identity, transform);

                // Sedikit variasi agar lebih natural
                var agent = go.GetComponent<NavMeshAgent>();
                if (agent)
                {
                    agent.speed = Random.Range(1.2f, 1.8f);
                }
                float scale = Random.Range(0.95f, 1.05f);
                go.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    bool TrySample(Vector3 c, Vector3 he, out Vector3 pos)
    {
        for (int i = 0; i < 40; i++)
        {
            Vector3 rnd = new Vector3(
                Random.Range(-he.x, he.x),
                0,
                Random.Range(-he.z, he.z)
            );
            if (NavMesh.SamplePosition(c + rnd, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }
        }
        pos = c;
        return false;
    }
}
