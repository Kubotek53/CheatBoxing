using UnityEngine;

/// <summary>
/// Persystentne dane runu — żyją między scenami (DontDestroyOnLoad).
/// Dostęp przez GameData.Instance z każdej sceny.
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    // ── Dane aktywnego runu (kasowane przy Game Over) ─────────
    [Header("Run Data")]
    public int   currentChapter  = 1;
    public int   currentLadderNode = 0;   // który szczebel drabinki
    public int   coins           = 0;
    public float playerHP        = 100f;
    public float playerMaxHP     = 100f;
    public bool  runActive       = false;  // czy jest aktywny run

    // ── Meta-progresja (opcjonalnie zapisywana do PlayerPrefs) ─
    [Header("Meta")]
    public int   highestChapter  = 1;
    public int   totalRuns       = 0;

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadMeta();
    }

    // ── API ──────────────────────────────────────────────────

    public void StartNewRun()
    {
        currentChapter    = 1;
        currentLadderNode = 0;
        coins             = 0;
        playerHP          = playerMaxHP;
        runActive         = true;
        totalRuns++;
        SaveMeta();
        Debug.Log("[GameData] Nowy run rozpoczęty");
    }

    public void ResetRun()
    {
        runActive         = false;
        currentChapter    = 1;
        currentLadderNode = 0;
        coins             = 0;
        playerHP          = playerMaxHP;
        Debug.Log("[GameData] Run zresetowany (Game Over)");
    }

    public bool HasActiveRun() => runActive;

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"[GameData] Monety: {coins} (+{amount})");
    }

    public void UpdateHP(float newHP)
    {
        playerHP = Mathf.Clamp(newHP, 0, playerMaxHP);
    }

    public void AdvanceLadder()
    {
        currentLadderNode++;
        Debug.Log($"[GameData] Szczebel: {currentLadderNode}");
    }

    public void AdvanceChapter()
    {
        currentChapter++;
        currentLadderNode = 0;
        if (currentChapter > highestChapter)
        {
            highestChapter = currentChapter;
            SaveMeta();
        }
        Debug.Log($"[GameData] Chapter: {currentChapter}");
    }

    // ── PlayerPrefs ──────────────────────────────────────────

    void SaveMeta()
    {
        PlayerPrefs.SetInt("HighestChapter", highestChapter);
        PlayerPrefs.SetInt("TotalRuns",      totalRuns);
        PlayerPrefs.Save();
    }

    void LoadMeta()
    {
        highestChapter = PlayerPrefs.GetInt("HighestChapter", 1);
        totalRuns      = PlayerPrefs.GetInt("TotalRuns",      0);
    }
}