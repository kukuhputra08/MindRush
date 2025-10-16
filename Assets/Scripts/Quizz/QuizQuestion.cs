using UnityEngine;

[CreateAssetMenu(fileName = "NewQuizQuestion", menuName = "Quiz/Question")]
public class QuizQuestion : ScriptableObject
{
    public enum Difficulty { Easy, Medium, Hard, VeryHard }

    [TextArea(2, 6)]
    public string question;

    [Tooltip("Isi 4 opsi. Biarkan kosong kalau tidak dipakai.")]
    public string[] choices = new string[4];

    [Range(0,3)]
    public int correctIndex = 0;

    public Difficulty difficulty = Difficulty.Easy;
}
