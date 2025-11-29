using UnityEngine;
using System.Collections.Generic;

public class QuizControllerLite : MonoBehaviour
{
    public static QuizControllerLite Instance { get; private set; }

    [Header("Anchors")]
    public Transform anchorsRoot;                         // ← tambahkan ini

    [Header("Anchors (drag parent QuizzAnchor atau isi manual)")]
    public List<QuizBuildingAnchor> anchors = new List<QuizBuildingAnchor>();

    [Header("Quiz UI (pakai Show(QuizBank, Action<bool>))")]
    public QuizUI quizUI;

    [Header("Bank per tingkat kesulitan")]
    public QuizBank bankEasy;
    public QuizBank bankMedium;
    public QuizBank bankHard;
    public QuizBank bankVeryHard;

    [Header("Kontrol Player saat kuis")]
    public MonoBehaviour playerControllerToDisable;

    [Header("Difficulty / DDA")]
    public QuizQuestion.Difficulty startDifficulty = QuizQuestion.Difficulty.Medium;
    [SerializeField] QuizQuestion.Difficulty currentDifficulty;

    [Header("Scoring")]
    public int correctScore = 20;
    public int wrongScore = -20;
    public bool useSpeedBonus = true;

    // Ambang waktu (detik) untuk cepat/lambat per tingkat
    readonly (float fast, float slow) E = (8f, 20f);
    readonly (float fast, float slow) M = (12f, 25f);
    readonly (float fast, float slow) H = (18f, 35f);
    readonly (float fast, float slow) VH = (25f, 45f);

    QuizBuildingAnchor active;
    float startTime;  // durasi jawab

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentDifficulty = startDifficulty;
    }

    void Start()
    {
        if (anchors.Count == 0)
        {
            if (anchorsRoot)
                anchors.AddRange(anchorsRoot.GetComponentsInChildren<QuizBuildingAnchor>(true));
            else
                anchors.AddRange(FindObjectsOfType<QuizBuildingAnchor>(true)); // fallback cari di seluruh scene
        }

        PickNewAnchor();
    }


    void PickNewAnchor()
    {
        if (anchors.Count == 0) return;

        if (active) active.SetActive(false);

        var next = anchors[Random.Range(0, anchors.Count)];
        if (next == active && anchors.Count > 1)
            next = anchors[(anchors.IndexOf(active) + 1) % anchors.Count];

        active = next;
        active.SetActive(true);
        // Debug.Log($"[QuizControllerLite] Target: {active.name}, diff={currentDifficulty}");
    }

    public void TryStartQuizAt(QuizBuildingAnchor anchor)
    {
        if (anchor != active) return;
        if (!quizUI) { Debug.LogError("[Quiz] QuizUI belum di-assign"); return; }

        QuizBank chosen = GetBankForDifficulty(currentDifficulty);
        if (!chosen) { Debug.LogError("[Quiz] Bank belum di-assign"); return; }

        // Lock kontrol + kursor
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;

        // >>> FREEZE permainan <<<
        Time.timeScale = 0f;

        // pakai waktu yang tidak terpengaruh timescale
        startTime = Time.unscaledTime;

        quizUI.Show(chosen, (bool correct) =>
        {
            float duration = Time.unscaledTime - startTime;

            // >>> RESUME permainan <<<
            Time.timeScale = 1f;

            if (playerControllerToDisable) playerControllerToDisable.enabled = true;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;

            // ====== SKOR dasar ======
            if (ScoreManager.Instance)
                ScoreManager.Instance.AddScore(correct ? correctScore : wrongScore);

            // ====== Bonus cepat (opsional) ======
            if (useSpeedBonus && correct)
            {
                int bonus = CalcSpeedBonus(currentDifficulty, duration);
                if (bonus > 0 && ScoreManager.Instance) ScoreManager.Instance.AddScore(bonus);
            }

            // DDA
            UpdateDifficulty(correct, duration);

            // Pindah target
            PickNewAnchor();
        });
    }


    QuizBank GetBankForDifficulty(QuizQuestion.Difficulty diff)
    {
        switch (diff)
        {
            case QuizQuestion.Difficulty.Easy: return bankEasy;
            case QuizQuestion.Difficulty.Medium: return bankMedium;
            case QuizQuestion.Difficulty.Hard: return bankHard;
            case QuizQuestion.Difficulty.VeryHard: return bankVeryHard;
            default: return bankEasy;
        }
    }

    int CalcSpeedBonus(QuizQuestion.Difficulty diff, float t)
    {
        var (fast, _) = GetBand(diff);
        if (t <= fast * 0.5f) return 10;   // sangat cepat
        if (t <= fast) return 5;   // cepat
        return 0;
    }

    void UpdateDifficulty(bool correct, float t)
    {
        var (fast, slow) = GetBand(currentDifficulty);

        if (!correct) currentDifficulty = Down(currentDifficulty);
        else if (t <= fast) currentDifficulty = Up(currentDifficulty);
        else if (t > slow) currentDifficulty = Down(currentDifficulty);
        // else: tetap
        // Debug.Log($"[DDA] -> {currentDifficulty} (t={t:F1}s, correct={correct})");
    }

    (float fast, float slow) GetBand(QuizQuestion.Difficulty d)
    {
        switch (d)
        {
            case QuizQuestion.Difficulty.Easy: return E;
            case QuizQuestion.Difficulty.Medium: return M;
            case QuizQuestion.Difficulty.Hard: return H;
            default: return VH;
        }
    }

    QuizQuestion.Difficulty Up(QuizQuestion.Difficulty d)
        => d == QuizQuestion.Difficulty.VeryHard ? d : d + 1;

    QuizQuestion.Difficulty Down(QuizQuestion.Difficulty d)
        => d == QuizQuestion.Difficulty.Easy ? d : d - 1;


    public Transform CurrentTarget
    {
        get { return active ? active.transform : null; }
    }

}


