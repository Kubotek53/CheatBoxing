using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Odbiera input gracza (klik/spacja) — kompatybilny z NOWYM Input System Unity.
/// Ocenia timing względem beatu i wysyła event z wynikiem.
/// </summary>
public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("Timing Windows (sekundy)")]
    public float perfectWindow = 0.05f; // ±50 ms
    public float goodWindow    = 0.12f; // ±120 ms
    public float okWindow      = 0.20f; // ±200 ms

    public enum HitResult { Perfect, Good, Ok, Miss }

    public event Action<HitResult> OnHit;
    public event Action OnMiss;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        bool clicked = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            clicked = true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;

        if (clicked)
            EvaluateInput();
    }

    void EvaluateInput()
    {
        if (RhytmManager.Instance == null)
        {
            Debug.LogWarning("[InputHandler] Brak RhytmManager!");
            return;
        }

        float dist = RhytmManager.Instance.GetDistanceToNearestBeat();
        HitResult result = ClassifyHit(dist);

        Debug.Log($"[InputHandler] Klik | dist={dist * 1000:F0}ms | wynik={result}");

        if (result == HitResult.Miss)
            OnMiss?.Invoke();

        OnHit?.Invoke(result);
    }

    HitResult ClassifyHit(float dist)
    {
        if (dist <= perfectWindow) return HitResult.Perfect;
        if (dist <= goodWindow)    return HitResult.Good;
        if (dist <= okWindow)      return HitResult.Ok;
        return HitResult.Miss;
    }

    public static float GetDamageMultiplier(HitResult result)
    {
        return result switch
        {
            HitResult.Perfect => 1.0f,
            HitResult.Good    => 0.7f,
            HitResult.Ok      => 0.4f,
            HitResult.Miss    => 0.0f,
            _                 => 0.0f
        };
    }
}