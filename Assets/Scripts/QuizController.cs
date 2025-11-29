using UnityEngine;
using System.Collections.Generic;

public class QuizController : MonoBehaviour
{
    public static QuizController Instance { get; private set; }

    [Header("Anchors (drag parent QuizzAnchor atau isi manual)")]
    public List<QuizBuildingAnchor> anchors = new List<QuizBuildingAnchor>();

    [Header("Quiz System")]
    public QuizBank quizBank;
    public QuizUIAdapter quizUIAdapter;               // <- gunakan adapter
    [Tooltip("Skrip gerak Player (mis. Player.cs) untuk dimatikan saat kuis")]
    public MonoBehaviour playerControllerToDisable;

    [Header("Difficulty / DDA")]
    public QuizQuestion.Difficulty startDifficulty = QuizQuestion.Difficulty.Medium;
    [SerializeField] QuizQuestion.Difficulty currentDifficulty;

    [Header("Scoring")]
    public int correctScore = 20;
    public int wrongScore = -20;
    public bool useSpeedBonus = true;

    // Ambang "cepat" / "lambat" per tingkat (detik)
    readonly (float fast, float slow) E = (8f, 20f);
    readonly (float fast, float slow) M = (12f, 25f);
    readonly (float fast, float slow) H = (18f, 35f);
    readonly (float fast, float slow) VH = (25f, 45f);

    QuizBuildingAnchor active;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentDifficulty = startDifficulty;
    }

    void Start()
    {
        if (anchors.Count == 0)
            anchors.AddRange(GetComponentsInChildren<QuizBuildingAnchor>(true));

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
        // Debug.Log($"[QuizController] Target: {active.name}, diff={currentDifficulty}");
    }

    public void TryStartQuizAt(QuizBuildingAnchor anchor)
    {
        if (anchor != active) return;
        if (!quizUIAdapter || !quizBank)
        {
            Debug.LogError("[QuizController] QuizUIAdapter/QuizBank belum di-assign");
            return;
        }

        // Lock kontrol player + tampilkan kursor
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;

        // Tampilkan quiz sesuai difficulty saat ini
        quizUIAdapter.Show(quizBank, OnQuizDone, currentDifficulty);
    }

    // Callback dari adapter: (benar/salah, durasi jawab, difficulty yang ditampilkan)
    void OnQuizDone(bool correct, float duration, QuizQuestion.Difficulty shownDifficulty)
    {
        // Kembalikan kontrol
        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;

        // Skor dasar
        ScoreManager.Instance.AddScore(correct ? correctScore : wrongScore);

        // Bonus cepat (opsional)
        if (useSpeedBonus && correct)
        {
            int bonus = CalcSpeedBonus(shownDifficulty, duration);
            if (bonus > 0) ScoreManager.Instance.AddScore(bonus);
        }

        // DDA naik/turun
        UpdateDifficulty(correct, duration);

        // Pindah target
        PickNewAnchor();
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
        var old = currentDifficulty;

        if (!correct) currentDifficulty = Down(currentDifficulty);
        else if (t <= fast) currentDifficulty = Up(currentDifficulty);
        else if (t > slow) currentDifficulty = Down(currentDifficulty);
        // else: tetap

        // Debug.Log($"[DDA] {old} -> {currentDifficulty} (t={t:F1}s, correct={correct})");
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
}
