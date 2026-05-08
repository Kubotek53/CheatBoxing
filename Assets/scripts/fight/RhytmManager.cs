using UnityEngine;
using System;

/// <summary>
/// Zarządza metrum gry. Wysyła eventy na każdy beat i dostarcza
/// czas do następnego/poprzedniego beatu dla systemu rytmicznego.
/// </summary>
public class RhytmManager : MonoBehaviour
{
    public static RhytmManager Instance { get; private set; }

    [Header("BPM")]
    public float bpm = 120f;

    [Header("Audio (opcjonalne)")]
    public AudioSource musicSource;
    public AudioClip metronomeClip; // opcjonalny klik metronomu

    // Publiczny dostęp do bieżącego numeru beatu
    public int BeatCount { get; private set; } = 0;

    // Czas trwania jednego beatu w sekundach
    public float BeatInterval { get; private set; }

    // Ile sekund minęło od ostatniego beatu (0..BeatInterval)
    public float TimeSinceLastBeat { get; private set; }

    // Ile sekund zostało do następnego beatu
    public float TimeToNextBeat => BeatInterval - TimeSinceLastBeat;

    // --- Eventy ---
    // Wywoływany co beat; parametr = numer beatu
    public event Action<int> OnBeat;

    // Wywoływany co 4 beaty (takt 4/4)
    public event Action<int> OnBar;

    private float beatTimer;
    private bool isRunning = false;

    // -------------------------------------------------------

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        SetBPM(bpm);
        StartRhythm();
    }

    void Update()
    {
        if (!isRunning) return;

        beatTimer += Time.deltaTime;
        TimeSinceLastBeat = beatTimer;

        if (beatTimer >= BeatInterval)
        {
            beatTimer -= BeatInterval;
            BeatCount++;

            // Opcjonalny dźwięk kliknięcia
            if (metronomeClip != null && musicSource != null)
                musicSource.PlayOneShot(metronomeClip, 0.3f);

            OnBeat?.Invoke(BeatCount);

            // Co 4 beaty wysyłamy event taktu
            if (BeatCount % 4 == 0)
                OnBar?.Invoke(BeatCount / 4);

            Debug.Log($"[RhytmManager] Beat {BeatCount} | BPM: {bpm}");
        }
    }

    // -------------------------------------------------------
    // API publiczne

    public void StartRhythm()
    {
        isRunning = true;
        beatTimer = 0f;
        BeatCount = 0;
    }

    public void StopRhythm()
    {
        isRunning = false;
    }

    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
        BeatInterval = 60f / bpm;
    }

    /// <summary>
    /// Zwraca jak blisko następnego beatu jesteśmy (0 = zaraz po, 1 = tuż przed).
    /// Przydatne do oceny timingu kliknięcia.
    /// </summary>
    public float GetBeatPhase()
    {
        return TimeSinceLastBeat / BeatInterval;
    }

    /// <summary>
    /// Zwraca odległość w sekundach od najbliższego beatu
    /// (poprzedniego LUB następnego – bierze mniejszą wartość).
    /// </summary>
    public float GetDistanceToNearestBeat()
    {
        float distPrev = TimeSinceLastBeat;
        float distNext = TimeToNextBeat;
        return Mathf.Min(distPrev, distNext);
    }
}