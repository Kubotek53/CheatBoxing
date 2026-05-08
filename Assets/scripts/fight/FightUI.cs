using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Obsługuje cały UI sceny Fight:
///   - Paski HP gracza i przeciwnika
///   - Flash ekranu przy trafieniu (kolor = wynik timing window)
///   - Tekst komunikatu (PERFECT! / GOOD / OK / MISS)
///   - Numer tury i stan rundy
///
/// Podłącz referencje w Inspectorze.
/// </summary>
public class FightUI : MonoBehaviour
{
    [Header("HP Bars")]
    public Slider playerHPSlider;
    public Slider enemyHPSlider;
    public Image  playerHPFill;   // opcjonalnie: zmienia kolor gdy HP niskie
    public Image  enemyHPFill;

    [Header("Timing Feedback")]
    public TextMeshProUGUI hitResultText;   // "PERFECT!" "GOOD" "OK" "MISS"
    public Image           screenFlash;     // pełnoekranowy Image (alpha 0 na co dzień)

    [Header("Info")]
    public TextMeshProUGUI turnText;        // "Tura 2/4"
    public TextMeshProUGUI roundResultText; // "WYGRANA!" "PORAŻKA" "BONUS TURA"

    [Header("Kolory feedbacku")]
    public Color perfectColor = new Color(1f,   0.92f, 0.016f); // złoty
    public Color goodColor    = new Color(0.18f, 0.8f,  0.44f); // zielony
    public Color okColor      = new Color(0.2f,  0.6f,  1f);    // niebieski
    public Color missColor    = new Color(1f,    0.2f,  0.2f);  // czerwony

    // -------------------------------------------------------

    void Start()
    {
        // Schowaj teksty startowo
        if (hitResultText)   hitResultText.alpha = 0;
        if (roundResultText) roundResultText.alpha = 0;
        if (screenFlash)     screenFlash.color = Color.clear;

        // Subskrybuj eventy
        var fm = FightManager.Instance;
        if (fm != null)
        {
            fm.OnPlayerHPChanged += UpdatePlayerHP;
            fm.OnEnemyHPChanged  += UpdateEnemyHP;
            fm.OnPlayerAttack    += ShowHitResult;
            fm.OnEnemyAttack     += ShowEnemyHit;
            fm.OnTurnStarted     += UpdateTurnText;
            fm.OnFightEnded      += ShowRoundResult;
        }
    }

    void OnDestroy()
    {
        var fm = FightManager.Instance;
        if (fm != null)
        {
            fm.OnPlayerHPChanged -= UpdatePlayerHP;
            fm.OnEnemyHPChanged  -= UpdateEnemyHP;
            fm.OnPlayerAttack    -= ShowHitResult;
            fm.OnEnemyAttack     -= ShowEnemyHit;
            fm.OnTurnStarted     -= UpdateTurnText;
            fm.OnFightEnded      -= ShowRoundResult;
        }
    }

    // -------------------------------------------------------
    // HP

    void UpdatePlayerHP(float current, float max)
    {
        if (playerHPSlider) playerHPSlider.value = current / max;

        // Kolor paska: zielony→żółty→czerwony
        if (playerHPFill)
            playerHPFill.color = Color.Lerp(missColor, goodColor, current / max);
    }

    void UpdateEnemyHP(float current, float max)
    {
        if (enemyHPSlider) enemyHPSlider.value = current / max;
    }

    // -------------------------------------------------------
    // Timing feedback

    void ShowHitResult(InputHandler.HitResult result, float damage)
    {
        if (hitResultText == null) return;

        (string label, Color col) = result switch
        {
            InputHandler.HitResult.Perfect => ("PERFECT!", perfectColor),
            InputHandler.HitResult.Good    => ("GOOD",     goodColor),
            InputHandler.HitResult.Ok      => ("OK",       okColor),
            InputHandler.HitResult.Miss    => ("MISS",     missColor),
            _                              => ("?",        Color.white)
        };

        hitResultText.text  = label;
        hitResultText.color = col;

        StopCoroutine(nameof(FadeText));
        StartCoroutine(FadeText(hitResultText, 0.8f));
        StartCoroutine(FlashScreen(col, 0.08f, 0.18f));
    }

    void ShowEnemyHit(float damage)
    {
        // Czerwony flash gdy gracz dostaje cios
        StartCoroutine(FlashScreen(missColor, 0.15f, 0.3f));
    }

    // -------------------------------------------------------
    // Tura

    void UpdateTurnText(int turn)
    {
        if (turnText == null) return;
        turnText.text = $"Tura {turn}/{FightManager.Instance.totalTurns}";
    }

    // -------------------------------------------------------
    // Wynik rundy

    void ShowRoundResult(FightManager.RoundResult result)
    {
        if (roundResultText == null) return;

        (string label, Color col) = result switch
        {
            FightManager.RoundResult.Win               => ("WYGRANA!",    goodColor),
            FightManager.RoundResult.Lose              => ("PORAŻKA",     missColor),
            FightManager.RoundResult.BonusTurnStarted  => ("BONUS TURA!", okColor),
            _                                          => ("?",           Color.white)
        };

        roundResultText.text  = label;
        roundResultText.color = col;
        StartCoroutine(FadeText(roundResultText, 2.5f));
        StartCoroutine(FlashScreen(col, 0.2f, 0.4f));
    }

    // -------------------------------------------------------
    // Coroutines

    IEnumerator FadeText(TextMeshProUGUI tmp, float displayTime)
    {
        tmp.alpha = 1f;
        yield return new WaitForSeconds(displayTime);

        float t = 0f, fadeDur = 0.4f;
        while (t < fadeDur)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(1f, 0f, t / fadeDur);
            yield return null;
        }
        tmp.alpha = 0f;
    }

    IEnumerator FlashScreen(Color col, float peakAlpha, float duration)
    {
        if (screenFlash == null) yield break;

        float half = duration * 0.5f;
        float t = 0f;

        // Fade in
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, peakAlpha, t / half);
            screenFlash.color = new Color(col.r, col.g, col.b, a);
            yield return null;
        }

        t = 0f;
        // Fade out
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(peakAlpha, 0f, t / half);
            screenFlash.color = new Color(col.r, col.g, col.b, a);
            yield return null;
        }

        screenFlash.color = Color.clear;
    }
}