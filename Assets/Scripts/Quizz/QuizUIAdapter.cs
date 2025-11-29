using UnityEngine;
using System;

public class QuizUIAdapter : MonoBehaviour
{
    [Header("Referensi ke UI lama")]
    public QuizUI quizUI;  // drag QuizUI milikmu di Inspector

    float startTime;
    Action<bool, float, QuizQuestion.Difficulty> onDoneWithTime;
    QuizQuestion.Difficulty shownDifficulty;

    /// <summary>
    /// Versi baru: panggil quiz dan kembalikan (benar/salah, durasi, difficulty).
    /// </summary>
    public void Show(QuizBank bank,
                     Action<bool, float, QuizQuestion.Difficulty> onDone,
                     QuizQuestion.Difficulty forceDifficulty)
    {
        if (!quizUI) { Debug.LogError("[QuizUIAdapter] QuizUI belum di-assign"); return; }

        shownDifficulty = forceDifficulty;
        onDoneWithTime = onDone;
        startTime = Time.time;

        // Panggil UI lama (2 argumen). Setelah pemain menjawab, hitung durasi dan teruskan.
        quizUI.Show(
            bank,
            (correct, duration, diff) => { onDone?.Invoke(correct, duration, diff); },
            forceDifficulty
        );

    }
}
