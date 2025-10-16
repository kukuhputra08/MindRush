using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuizBank", menuName = "Quiz/Bank")]
public class QuizBank : ScriptableObject
{
    public List<QuizQuestion> questions = new List<QuizQuestion>();

    /// <summary>
    /// Ambil 1 soal acak. Kalau filter null, ambil dari semua.
    /// </summary>
    public QuizQuestion GetRandom(QuizQuestion.Difficulty? filter = null)
    {
        if (questions == null || questions.Count == 0) return null;

        if (filter.HasValue)
        {
            var list = questions.FindAll(q => q && q.difficulty == filter.Value);
            if (list.Count > 0)
                return list[Random.Range(0, list.Count)];
        }

        // fallback: dari semua
        QuizQuestion q = null;
        int safety = 0;
        while (!q && safety++ < 50)
            q = questions[Random.Range(0, questions.Count)];
        return q;
    }
}
