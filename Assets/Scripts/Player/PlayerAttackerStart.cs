using UnityEngine;

public class PlayerAttackerStart : MonoBehaviour
{
    public Animator animator;      // referensi ke animator player
    int comboIndex = 0;            // 0=Kanan, 1=Kiri, 2=Combo, 3=Tendang
    float lastClickTime = -999f;   // waktu klik terakhir
    public float comboResetTime = 0.1f; // waktu maksimal antar klik

    void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Klik kiri mouse = serang
        if (Input.GetMouseButtonDown(0))
        {
            // Jika sudah terlalu lama dari klik terakhir, mulai ulang dari combo 0
            if (Time.time - lastClickTime > comboResetTime)
                comboIndex = 0;

            // Set parameter animator
            animator.ResetTrigger("Attack");
            animator.SetInteger("ComboIndex", comboIndex);
            animator.SetTrigger("Attack");

            // Lanjutkan ke combo berikutnya
            comboIndex = (comboIndex + 1) % 4;
            lastClickTime = Time.time;
        }
    }

    // Fungsi kosong untuk event animasi (biar gak error)
    public void OnPunchStart() { }
    public void DoPunchHit() { }
    public void OnPunchEnd() { }
}
