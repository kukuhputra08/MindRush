using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUI : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI questionTMP;
    public Button[] optionButtons; // 4 tombol

    private QuizBank bank;
    private QuizQuestion current;
    private System.Action<bool> onDone;

    public void Show(QuizBank bank, System.Action<bool> onDone)
    {
        this.bank = bank;
        this.onDone = onDone;

        current = bank.GetRandom(null); // atau filter kesulitan
        if (!current) { Close(false); return; }

        questionTMP.text = current.question;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            bool has = i < current.choices.Length && !string.IsNullOrEmpty(current.choices[i]);
            btn.gameObject.SetActive(has);
            if (!has) continue;

            btn.onClick.RemoveAllListeners();
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt) txt.text = current.choices[i];

            int idx = i;
            btn.onClick.AddListener(() => OnPick(idx));
        }

        gameObject.SetActive(true);
    }

    void OnPick(int idx) => Close(idx == current.correctIndex);

    void Close(bool correct)
    {
        gameObject.SetActive(false);
        onDone?.Invoke(correct);
    }
}
