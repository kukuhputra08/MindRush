using UnityEngine;

public class QuizCheckpoint : MonoBehaviour
{
    [Header("Refs (drag dari Hierarchy/Project)")]
    public GameObject promptUI;     // Canvas/TekanF
    public GameObject soalUI;       // Canvas/Soal
    public QuizUI quizUI;           // component di "Soal"
    public QuizBank quizBank;       // asset bank soal (dari CSV)
    public MonoBehaviour playerController; // script gerak player (optional untuk di-disable)

    bool playerIn = false;
    float nextUseTime = 0f;
    public float useCooldown = 0.25f;

    void Awake()
    {
        if (promptUI) promptUI.SetActive(false);
        if (soalUI)   soalUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIn = true;
            if (promptUI) promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIn = false;
            if (promptUI) promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerIn) return;

        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextUseTime)
        {
            nextUseTime = Time.time + useCooldown;
            StartQuiz();
        }
    }

    void StartQuiz()
    {
        if (promptUI) promptUI.SetActive(false);

        // kunci kontrol & tampilkan mouse (biar bisa klik tombol)
        if (playerController) playerController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        soalUI.SetActive(true);
        quizUI.Show(quizBank, OnQuizDone);
    }

void OnQuizDone(bool correct)
{
    soalUI.SetActive(false);

    // Kembalikan kontrol player
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    if (playerController) playerController.enabled = true;

    // --- Tambahkan bagian ini ---
    if (ScoreManager.Instance)
    {
        if (correct)
            ScoreManager.Instance.AddScore(+20);
        else
            ScoreManager.Instance.AddScore(-20);
    }
    // -----------------------------

    Debug.Log("Jawaban benar? " + correct);
}

}
