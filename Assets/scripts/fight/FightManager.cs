using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Serce walki. Zarządza turami, HP gracza i przeciwnika,
/// oraz warunkami zakończenia rundy zgodnie z zasadami roguelike.
/// </summary>
public class FightManager : MonoBehaviour
{
    public static FightManager Instance { get; private set; }

    // -------------------------------------------------------
    // Konfiguracja rundy

    [Header("HP")]
    public float playerMaxHP    = 100f;
    public float enemyMaxHP     = 100f;

    [Header("Obrażenia bazowe")]
    public float playerBaseDmg  = 20f;  // mnożony przez timing window
    public float enemyBaseDmg   = 15f;

    [Header("Ilość tur (ustawia ChapterManager)")]
    public int totalTurns       = 4;

    [Header("Opóźnienie tury przeciwnika (sekundy)")]
    public float enemyTurnDelay = 1.2f;

    // -------------------------------------------------------
    // Stan walki

    public float PlayerHP { get; private set; }
    public float EnemyHP  { get; private set; }
    public int   CurrentTurn { get; private set; } = 0;
    public bool  IsFightActive { get; private set; } = false;

    public enum FightState { WaitingForInput, EnemyTurn, BonusTurn, Finished }
    public FightState State { get; private set; } = FightState.Finished;

    // -------------------------------------------------------
    // Eventy

    public event Action<float, float> OnPlayerHPChanged;   // (nowe HP, max HP)
    public event Action<float, float> OnEnemyHPChanged;
    public event Action<int>          OnTurnStarted;        // numer tury
    public event Action<InputHandler.HitResult, float> OnPlayerAttack; // wynik, obrażenia
    public event Action<float>        OnEnemyAttack;        // obrażenia
    public event Action               OnFightStarted;
    public event Action<RoundResult>  OnFightEnded;

    public enum RoundResult { Win, Lose, BonusTurnStarted }

    // -------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Subskrybujemy InputHandler
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnHit += HandlePlayerHit;
    }

    void OnDestroy()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnHit -= HandlePlayerHit;
    }

    // -------------------------------------------------------
    // API

    public void StartFight(int turns = 4)
    {
        totalTurns   = turns;
        PlayerHP     = playerMaxHP;
        EnemyHP      = enemyMaxHP;
        CurrentTurn  = 0;
        IsFightActive = true;
        State        = FightState.WaitingForInput;

        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);
        OnEnemyHPChanged?.Invoke(EnemyHP,  enemyMaxHP);
        OnFightStarted?.Invoke();

        StartTurn();
    }

    public void StartFight() => StartFight(totalTurns);

    // -------------------------------------------------------
    // Tury

    void StartTurn()
    {
        CurrentTurn++;
        State = FightState.WaitingForInput;
        Debug.Log($"[FightManager] === Tura {CurrentTurn}/{totalTurns} ===");
        OnTurnStarted?.Invoke(CurrentTurn);
    }

    // Gracz kliknął – oceniamy cios
    void HandlePlayerHit(InputHandler.HitResult result)
    {
        if (State != FightState.WaitingForInput) return;

        float mult   = InputHandler.GetDamageMultiplier(result);
        float damage = playerBaseDmg * mult;

        EnemyHP = Mathf.Max(0, EnemyHP - damage);
        OnPlayerAttack?.Invoke(result, damage);
        OnEnemyHPChanged?.Invoke(EnemyHP, enemyMaxHP);

        Debug.Log($"[FightManager] Gracz atakuje: {result} | dmg={damage:F1} | HP wroga={EnemyHP:F1}");

        // Wróg odpowiada po opóźnieniu
        State = FightState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(enemyTurnDelay);

        // Prosty AI: zadaje pełne obrażenia (rozbuduj w EnemyAI.cs)
        float damage = enemyBaseDmg;
        PlayerHP = Mathf.Max(0, PlayerHP - damage);
        OnEnemyAttack?.Invoke(damage);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);

        Debug.Log($"[FightManager] Wróg atakuje: dmg={damage:F1} | HP gracza={PlayerHP:F1}");

        // Czy tura kończy walkę?
        bool isFinalTurn = (CurrentTurn >= totalTurns);

        if (!isFinalTurn)
        {
            StartTurn();
        }
        else
        {
            EvaluateRoundEnd(isBonus: false);
        }
    }

    // -------------------------------------------------------
    // Warunki zakończenia rundy

    void EvaluateRoundEnd(bool isBonus)
    {
        float hpPercent = PlayerHP / playerMaxHP;
        State = FightState.Finished;
        IsFightActive = false;

        Debug.Log($"[FightManager] Koniec {"bonusowej " + (isBonus ? "tury" : "rundy")} | HP%={hpPercent*100:F0}%");

        if (hpPercent < 0.50f)
        {
            // < 50% — PORAŻKA
            Debug.Log("[FightManager] PORAŻKA — restart runu");
            OnFightEnded?.Invoke(RoundResult.Lose);
        }
        else if (!isBonus && hpPercent < 0.75f)
        {
            // 50–75% na normalnym końcu — BONUS TURA
            Debug.Log("[FightManager] BONUS TURA — ostatnia szansa");
            State = FightState.BonusTurn;
            IsFightActive = true;
            OnFightEnded?.Invoke(RoundResult.BonusTurnStarted);
            StartCoroutine(StartBonusTurn());
        }
        else
        {
            // > 75% (lub przeżyłeś bonus turę) — WYGRANA
            Debug.Log("[FightManager] WYGRANA rundy!");
            OnFightEnded?.Invoke(RoundResult.Win);
        }
    }

    IEnumerator StartBonusTurn()
    {
        yield return new WaitForSeconds(0.5f);

        // Jedna dodatkowa tura
        CurrentTurn++;
        State = FightState.WaitingForInput;
        OnTurnStarted?.Invoke(CurrentTurn);

        // Po bonusowej turze wróg też odpowiada — słuchamy kolejnego HitResult
        // Tymczasowo podmieniamy handler na wersję "bonus"
        InputHandler.Instance.OnHit -= HandlePlayerHit;
        InputHandler.Instance.OnHit += HandleBonusHit;
    }

    void HandleBonusHit(InputHandler.HitResult result)
    {
        InputHandler.Instance.OnHit -= HandleBonusHit;
        if (State != FightState.WaitingForInput) return;

        float mult   = InputHandler.GetDamageMultiplier(result);
        float damage = playerBaseDmg * mult;

        EnemyHP  = Mathf.Max(0, EnemyHP - damage);
        OnPlayerAttack?.Invoke(result, damage);
        OnEnemyHPChanged?.Invoke(EnemyHP, enemyMaxHP);

        State = FightState.EnemyTurn;
        StartCoroutine(BonusEnemyTurnRoutine());
    }

    IEnumerator BonusEnemyTurnRoutine()
    {
        yield return new WaitForSeconds(enemyTurnDelay);

        float damage = enemyBaseDmg;
        PlayerHP = Mathf.Max(0, PlayerHP - damage);
        OnEnemyAttack?.Invoke(damage);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);

        EvaluateRoundEnd(isBonus: true);

        // Przywracamy normalny handler
        InputHandler.Instance.OnHit += HandlePlayerHit;
    }
}