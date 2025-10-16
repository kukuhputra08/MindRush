#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Import file CSV menjadi asset QuizBank (ScriptableObject).
/// Kompatibel dengan kelas:
///   public class QuizQuestion { public string question; public string[] choices = new string[4]; public int correctIndex; }
///   public class QuizBank : ScriptableObject { public List<QuizQuestion> questions = new List<QuizQuestion>(); }
///
/// Format CSV yang didukung:
/// - Dengan header (direkomendasikan):
///   difficulty;question;choice0;choice1;choice2;choice3;correctIndex
///   atau
///   question,optionA,optionB,optionC,optionD,correctIndex
///
/// - Tanpa header (fallback):
///   question,choice0,choice1,choice2,choice3,correctIndex
///
/// Delimiter otomatis terdeteksi: koma (,) atau titik koma (;)
/// Nilai correctIndex berbasis 0 (0..3).
/// String ber-kutip "..." didukung (akan unescape).
/// </summary>
public class QuizCSVImporter : EditorWindow
{
    [Header("CSV")]
    public TextAsset csvFile;

    [Header("Output")]
    public string saveFolder = "Assets/Soal/Generated";
    public string assetBaseName = "QuizBank";

    [Tooltip("Pisahkan ke beberapa bank (berguna untuk ratusan soal).")]
    public bool splitIntoChunks = false;
    public int chunkSize = 25;

    [MenuItem("Tools/Quiz/Import CSV → QuizBank")]
    public static void Open()
    {
        GetWindow<QuizCSVImporter>("Quiz CSV Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV → QuizBank", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Pilih CSV berisi soal. Delimiter , atau ; didukung. Header opsional.\nKolom umum: question, choice0..3 (atau optionA..D), correctIndex.", MessageType.Info);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
        assetBaseName = EditorGUILayout.TextField("Asset Base Name", assetBaseName);

        splitIntoChunks = EditorGUILayout.Toggle("Split Into Chunks", splitIntoChunks);
        if (splitIntoChunks)
            chunkSize = Mathf.Max(1, EditorGUILayout.IntField("Chunk Size", chunkSize));

        GUILayout.Space(8);
        using (new EditorGUI.DisabledScope(csvFile == null))
        {
            if (GUILayout.Button("Generate QuizBank"))
                Import();
        }
    }

    void Import()
    {
        if (csvFile == null)
        {
            Debug.LogError("Pilih CSV terlebih dahulu.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            AssetDatabase.Refresh();
        }

        var all = ParseCSV(csvFile.text);
        if (all.Count == 0)
        {
            Debug.LogError("Tidak ada soal yang berhasil diparse dari CSV.");
            return;
        }

        if (!splitIntoChunks)
        {
            string path = Path.Combine(saveFolder, $"{assetBaseName}.asset").Replace("\\", "/");
            CreateBankAsset(all, path);
        }
        else
        {
            int i = 0;
            int bankNo = 1;
            while (i < all.Count)
            {
                int take = Mathf.Min(chunkSize, all.Count - i);
                var slice = all.GetRange(i, take);
                string path = Path.Combine(saveFolder, $"{assetBaseName}_{bankNo:00}.asset").Replace("\\", "/");
                CreateBankAsset(slice, path);
                i += take;
                bankNo++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Selesai import. Total soal: {all.Count}");
    }

    // ================= Helpers =================

    List<QuizQuestion> ParseCSV(string text)
    {
        var list = new List<QuizQuestion>();
        if (string.IsNullOrEmpty(text)) return list;

        // Normalisasi newline
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = text.Split('\n');
        if (lines.Length == 0) return list;

        // Deteksi delimiter dari baris pertama
        string header = lines[0];
        char delimiter = DetectDelimiter(header);

        // Coba baca header
        var headerCols = SplitCSVLine(header, delimiter);
        bool hasHeader = headerCols.Exists(s => s.Trim().ToLower().Contains("question"));

        // Mapping index kolom (jika ada header)
        int idxQuestion = -1, idxC0 = -1, idxC1 = -1, idxC2 = -1, idxC3 = -1, idxCorrect = -1;

        if (hasHeader)
        {
            idxQuestion = IndexOf(headerCols, "question", "pertanyaan");
            idxC0 = IndexOf(headerCols, "choice0", "optiona", "a", "answer0");
            idxC1 = IndexOf(headerCols, "choice1", "optionb", "b", "answer1");
            idxC2 = IndexOf(headerCols, "choice2", "optionc", "c", "answer2");
            idxC3 = IndexOf(headerCols, "choice3", "optiond", "d", "answer3");
            idxCorrect = IndexOf(headerCols, "correctindex", "correct", "answerindex", "kunci", "kunci_jawaban");
        }

        int startLine = hasHeader ? 1 : 0;

        for (int i = startLine; i < lines.Length; i++)
        {
            string raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var cols = SplitCSVLine(raw, delimiter);
            if (cols.Count == 0) continue;

            if (!hasHeader)
            {
                // fallback: question,c0,c1,c2,c3,correctIndex
                if (cols.Count < 6) continue;

                var q = new QuizQuestion
                {
                    question = Safe(cols, 0),
                    choices = new string[4]
                    {
                        Safe(cols, 1),
                        Safe(cols, 2),
                        Safe(cols, 3),
                        Safe(cols, 4),
                    },
                    correctIndex = ClampIndex(ParseInt(Safe(cols, 5), 0))
                };
                list.Add(q);
            }
            else
            {
                if (idxQuestion < 0 || idxC0 < 0 || idxC1 < 0 || idxC2 < 0 || idxC3 < 0 || idxCorrect < 0)
                    continue;

                var q = new QuizQuestion
                {
                    question = Safe(cols, idxQuestion),
                    choices = new string[4]
                    {
                        Safe(cols, idxC0),
                        Safe(cols, idxC1),
                        Safe(cols, idxC2),
                        Safe(cols, idxC3),
                    },
                    correctIndex = ClampIndex(ParseInt(Safe(cols, idxCorrect), 0))
                };
                list.Add(q);
            }
        }

        return list;
    }

    static char DetectDelimiter(string sample)
    {
        // Pilih yang lebih banyak di baris pertama
        int commas = 0, semis = 0;
        foreach (char c in sample)
        {
            if (c == ',') commas++;
            else if (c == ';') semis++;
        }
        return (semis > commas) ? ';' : ',';
    }

    static int ClampIndex(int i) => Mathf.Clamp(i, 0, 3);

    static int ParseInt(string s, int def)
    {
        int x;
        return int.TryParse(s, out x) ? x : def;
    }

    static string Safe(List<string> cols, int idx)
    {
        if (idx < 0 || idx >= cols.Count) return "";
        return cols[idx]?.Trim() ?? "";
    }

    static int IndexOf(List<string> cols, params string[] keys)
    {
        for (int i = 0; i < cols.Count; i++)
        {
            string c = cols[i].Trim().ToLower();
            foreach (var k in keys)
                if (c == k) return i;
        }
        return -1;
    }

    // Split CSV satu baris, mendukung "teks, dengan, koma"
    static List<string> SplitCSVLine(string line, char delimiter)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(line)) return result;

        string pattern = $"(?:^|{Regex.Escape(delimiter.ToString())})(\"(?:[^\"]|\"\")*\"|[^{Regex.Escape(delimiter.ToString())}]*)";
        foreach (Match m in Regex.Matches(line, pattern))
        {
            string val = m.Groups[1].Value;
            if (val.StartsWith("\"") && val.EndsWith("\""))
                val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
            result.Add(val.Trim());
        }
        return result;
    }

    void CreateBankAsset(List<QuizQuestion> questions, string assetPath)
    {
        var bank = ScriptableObject.CreateInstance<QuizBank>();
        bank.questions = new List<QuizQuestion>(questions);
        AssetDatabase.CreateAsset(bank, assetPath);
        EditorUtility.SetDirty(bank);
        Debug.Log($"Created: {assetPath} (soal: {questions.Count})");
    }
}
#endif
