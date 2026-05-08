using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Tworzy CAŁY UI sceny Fight przez kod — zero ręcznego składania w Inspectorze.
/// Dodaj ten skrypt do pustego GameObject "FightUIBootstrap" w scenie.
/// Resztę zrobi sam przy Start().
/// </summary>
public class FightUIBootstrap : MonoBehaviour
{
    // ── Referencje tworzone dynamicznie ──────────────────────
    private Canvas      canvas;
    private Slider      playerHPSlider, enemyHPSlider;
    private Image       playerHPFill, enemyHPFill;
    private TextMeshProUGUI hitResultText, turnText, roundResultText;
    private TextMeshProUGUI playerHPLabel, enemyHPLabel;
    private Image       screenFlash;
    private GameObject  hitZoneRing;       // kółko docelowe dla nut

    // ── Paleta kolorów ────────────────────────────────────────
    static readonly Color COL_BG         = new Color(0.07f, 0.07f, 0.10f, 0.95f);
    static readonly Color COL_PLAYER_BAR = new Color(0.18f, 0.85f, 0.44f);   // zielony
    static readonly Color COL_ENEMY_BAR  = new Color(0.95f, 0.25f, 0.25f);   // czerwony
    static readonly Color COL_PERFECT    = new Color(1f,    0.92f, 0.016f);  // złoty
    static readonly Color COL_GOOD       = new Color(0.18f, 0.85f, 0.44f);
    static readonly Color COL_OK         = new Color(0.25f, 0.60f, 1f);
    static readonly Color COL_MISS       = new Color(1f,    0.25f, 0.25f);
    static readonly Color COL_PANEL      = new Color(0f,    0f,    0f,    0.55f);

    // ─────────────────────────────────────────────────────────

    void Start()
    {
        BuildCanvas();
        BuildHPBars();
        BuildHitZoneRing();
        BuildTexts();
        BuildScreenFlash();
        HookEvents();

        // Autostart walki po chwili
        StartCoroutine(AutoStartFight());
    }

    IEnumerator AutoStartFight()
    {
        yield return new WaitForSeconds(0.5f);
        if (FightManager.Instance != null)
            FightManager.Instance.StartFight();
    }

    // ═══════════════════════════════════════════════════════
    // BUDOWANIE UI
    // ═══════════════════════════════════════════════════════

    void BuildCanvas()
    {
        var go = new GameObject("FightCanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    // ── Paski HP ──────────────────────────────────────────────
    void BuildHPBars()
    {
        // Gracz — lewy górny róg
        playerHPSlider = CreateHPBar("PlayerHP", new Vector2(20, -20),
            new Vector2(420, 48), TextAnchor.UpperLeft, out playerHPFill, out playerHPLabel,
            COL_PLAYER_BAR, "GRACZ");

        // Wróg — prawy górny róg
        enemyHPSlider = CreateHPBar("EnemyHP", new Vector2(-20, -20),
            new Vector2(420, 48), TextAnchor.UpperRight, out enemyHPFill, out enemyHPLabel,
            COL_ENEMY_BAR, "WRÓG");
    }

    Slider CreateHPBar(string name, Vector2 anchoredPos, Vector2 size,
        TextAnchor anchor, out Image fillImg, out TextMeshProUGUI label,
        Color barColor, string labelText)
    {
        bool isLeft = (anchor == TextAnchor.UpperLeft);

        // Panel tła
        var panel = CreateUIObject(name + "Panel", canvas.transform);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(size.x, size.y + 28);
        SetAnchorCorner(panelRT, isLeft ? Corner.TopLeft : Corner.TopRight, anchoredPos);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = COL_PANEL;

        // Label (GRACZ / WRÓG)
        var labelGO = CreateUIObject(name + "Label", panel.transform);
        label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 14;
        label.fontStyle = FontStyles.Bold;
        label.color = barColor;
        label.alignment = isLeft ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 1); labelRT.anchorMax = new Vector2(1, 1);
        labelRT.pivot = new Vector2(0.5f, 1);
        labelRT.offsetMin = new Vector2(8, -22);
        labelRT.offsetMax = new Vector2(-8, 0);

        // Slider
        var sliderGO = CreateUIObject(name + "Slider", panel.transform);
        var slider = sliderGO.AddComponent<Slider>();
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0, 0); sliderRT.anchorMax = new Vector2(1, 0);
        sliderRT.pivot = new Vector2(0.5f, 0);
        sliderRT.offsetMin = new Vector2(8, 6);
        sliderRT.offsetMax = new Vector2(-8, 6 + size.y);

        // Background paska
        var bg = CreateUIObject("Background", sliderGO.transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Fill Area
        var fillArea = CreateUIObject("Fill Area", sliderGO.transform);
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(2, 2);
        fillAreaRT.offsetMax = new Vector2(-2, -2);

        // Fill
        var fill = CreateUIObject("Fill", fillArea.transform);
        fillImg = fill.AddComponent<Image>();
        fillImg.color = barColor;
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1, 1);
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;

        slider.fillRect = fillRT;
        slider.direction = isLeft ? Slider.Direction.LeftToRight : Slider.Direction.RightToLeft;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        return slider;
    }

    // ── Kółko docelowe (HitZone) ──────────────────────────────
    void BuildHitZoneRing()
    {
        hitZoneRing = new GameObject("HitZoneRing");
        hitZoneRing.transform.position = new Vector3(0, -1.5f, 0); // centrum sceny

        // Zewnętrzny ring (niebieski)
        var outer = CreateCircleSprite(hitZoneRing.transform, "OuterRing", 1.3f,
            new Color(0.25f, 0.60f, 1f, 0.5f));

        // Wewnętrzny (biały rdzeń)
        var inner = CreateCircleSprite(hitZoneRing.transform, "InnerRing", 0.35f,
            new Color(1f, 1f, 1f, 0.85f));

        // Pulsacja kółka zewnętrznego co beat
        StartCoroutine(PulseRing(outer));

        // Podepnij HitZone do NoteSpawnera jeśli istnieje
        var spawner = FindObjectOfType<NoteSpawner>();
        if (spawner != null)
            spawner.hitZone = hitZoneRing.transform;
    }

    IEnumerator PulseRing(GameObject ring)
    {
        if (ring == null) yield break;
        var sr = ring.GetComponent<SpriteRenderer>();
        Vector3 baseScale = ring.transform.localScale;

        while (true)
        {
            // Czekaj na beat
            bool beat = false;
            System.Action<int> onBeat = _ => beat = true;
            if (RhytmManager.Instance != null)
                RhytmManager.Instance.OnBeat += onBeat;

            yield return new WaitUntil(() => beat);

            if (RhytmManager.Instance != null)
                RhytmManager.Instance.OnBeat -= onBeat;

            // Szybki pulse: scale up → down
            float t = 0f; float dur = 0.12f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / dur * Mathf.PI) * 0.25f;
                if (ring != null) ring.transform.localScale = baseScale * s;
                yield return null;
            }
            if (ring != null) ring.transform.localScale = baseScale;
        }
    }

    // ── Teksty ───────────────────────────────────────────────
    void BuildTexts()
    {
        // Numer tury — góra środek
        var turnGO = CreateUIObject("TurnText", canvas.transform);
        turnText = turnGO.AddComponent<TextMeshProUGUI>();
        turnText.text = "Tura 0/4";
        turnText.fontSize = 22;
        turnText.fontStyle = FontStyles.Bold;
        turnText.color = Color.white;
        turnText.alignment = TextAlignmentOptions.Center;
        var tRT = turnGO.GetComponent<RectTransform>();
        SetAnchorCorner(tRT, Corner.TopCenter, new Vector2(0, -20));
        tRT.sizeDelta = new Vector2(220, 40);

        // Wynik trafienia — środek ekranu
        var hitGO = CreateUIObject("HitResultText", canvas.transform);
        hitResultText = hitGO.AddComponent<TextMeshProUGUI>();
        hitResultText.text = "";
        hitResultText.fontSize = 52;
        hitResultText.fontStyle = FontStyles.Bold;
        hitResultText.color = COL_PERFECT;
        hitResultText.alignment = TextAlignmentOptions.Center;
        hitResultText.alpha = 0;
        var hRT = hitGO.GetComponent<RectTransform>();
        SetAnchorCenter(hRT, new Vector2(0, 80));
        hRT.sizeDelta = new Vector2(500, 80);

        // Wynik rundy — środek ekranu
        var rrGO = CreateUIObject("RoundResultText", canvas.transform);
        roundResultText = rrGO.AddComponent<TextMeshProUGUI>();
        roundResultText.text = "";
        roundResultText.fontSize = 64;
        roundResultText.fontStyle = FontStyles.Bold;
        roundResultText.color = COL_GOOD;
        roundResultText.alignment = TextAlignmentOptions.Center;
        roundResultText.alpha = 0;
        var rrRT = rrGO.GetComponent<RectTransform>();
        SetAnchorCenter(rrRT, Vector2.zero);
        rrRT.sizeDelta = new Vector2(600, 100);
    }

    // ── Flash ekranu ─────────────────────────────────────────
    void BuildScreenFlash()
    {
        var go = CreateUIObject("ScreenFlash", canvas.transform);
        screenFlash = go.AddComponent<Image>();
        screenFlash.color = Color.clear;
        screenFlash.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ═══════════════════════════════════════════════════════
    // PODPINANIE EVENTÓW
    // ═══════════════════════════════════════════════════════

    void HookEvents()
    {
        var fm = FightManager.Instance;
        if (fm == null) return;

        fm.OnPlayerHPChanged += (cur, max) => UpdateSlider(playerHPSlider, playerHPFill, cur, max, COL_PLAYER_BAR);
        fm.OnEnemyHPChanged  += (cur, max) => UpdateSlider(enemyHPSlider,  enemyHPFill,  cur, max, COL_ENEMY_BAR);
        fm.OnPlayerAttack    += (result, dmg) => ShowHitResult(result);
        fm.OnEnemyAttack     += dmg => StartCoroutine(FlashScreen(COL_MISS, 0.18f, 0.28f));
        fm.OnTurnStarted     += turn =>
        {
            if (turnText) turnText.text = $"Tura {turn}/{fm.totalTurns}";
        };
        fm.OnFightEnded += result =>
        {
            (string label, Color col) = result switch
            {
                FightManager.RoundResult.Win              => ("WYGRANA!",    COL_GOOD),
                FightManager.RoundResult.Lose             => ("PORAŻKA",     COL_MISS),
                FightManager.RoundResult.BonusTurnStarted => ("BONUS TURA!", COL_OK),
                _                                         => ("?",           Color.white)
            };
            if (roundResultText != null)
            {
                roundResultText.text  = label;
                roundResultText.color = col;
                StartCoroutine(FadeText(roundResultText, 2.5f));
                StartCoroutine(FlashScreen(col, 0.22f, 0.45f));
            }
        };
    }

    // ═══════════════════════════════════════════════════════
    // LOGIKA UI
    // ═══════════════════════════════════════════════════════

    void UpdateSlider(Slider slider, Image fill, float cur, float max, Color baseColor)
    {
        if (slider == null) return;
        slider.value = cur / max;

        // Kolor: zielony→żółty→czerwony w zależności od HP%
        float t = cur / max;
        if (fill != null)
            fill.color = t > 0.5f
                ? Color.Lerp(new Color(1f, 0.85f, 0f), baseColor, (t - 0.5f) * 2f)
                : Color.Lerp(COL_MISS, new Color(1f, 0.85f, 0f), t * 2f);
    }

    void ShowHitResult(InputHandler.HitResult result)
    {
        if (hitResultText == null) return;

        (string label, Color col) = result switch
        {
            InputHandler.HitResult.Perfect => ("PERFECT!", COL_PERFECT),
            InputHandler.HitResult.Good    => ("GOOD",     COL_GOOD),
            InputHandler.HitResult.Ok      => ("OK",       COL_OK),
            InputHandler.HitResult.Miss    => ("MISS",     COL_MISS),
            _                              => ("?",        Color.white)
        };

        hitResultText.text  = label;
        hitResultText.color = col;
        StopCoroutine(nameof(FadeHitText));
        StartCoroutine(FadeHitText());
        StartCoroutine(FlashScreen(col, 0.07f, 0.15f));
    }

    IEnumerator FadeHitText()
    {
        if (hitResultText == null) yield break;
        hitResultText.alpha = 1f;

        // Skok w górę
        var rt = hitResultText.GetComponent<RectTransform>();
        Vector2 start = new Vector2(0, 80);
        Vector2 end   = new Vector2(0, 130);
        float t = 0f; float dur = 0.55f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            rt.anchoredPosition = Vector2.Lerp(start, end, p);
            hitResultText.alpha = Mathf.Lerp(1f, 0f, p * p);
            yield return null;
        }
        rt.anchoredPosition = start;
        hitResultText.alpha = 0f;
    }

    IEnumerator FadeText(TextMeshProUGUI tmp, float displayTime)
    {
        tmp.alpha = 1f;
        yield return new WaitForSeconds(displayTime);
        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(1f, 0f, t / dur);
            yield return null;
        }
        tmp.alpha = 0f;
    }

    IEnumerator FlashScreen(Color col, float peakAlpha, float duration)
    {
        if (screenFlash == null) yield break;
        float half = duration * 0.5f, t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            screenFlash.color = new Color(col.r, col.g, col.b, Mathf.Lerp(0f, peakAlpha, t / half));
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            screenFlash.color = new Color(col.r, col.g, col.b, Mathf.Lerp(peakAlpha, 0f, t / half));
            yield return null;
        }
        screenFlash.color = Color.clear;
    }

    // ═══════════════════════════════════════════════════════
    // HELPERY
    // ═══════════════════════════════════════════════════════

    enum Corner { TopLeft, TopRight, TopCenter, Center }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetAnchorCorner(RectTransform rt, Corner corner, Vector2 offset)
    {
        switch (corner)
        {
            case Corner.TopLeft:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
                break;
            case Corner.TopRight:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
                break;
            case Corner.TopCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
        }
        rt.anchoredPosition = offset;
    }

    static void SetAnchorCenter(RectTransform rt, Vector2 offset)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
    }

    static GameObject CreateCircleSprite(Transform parent, string name, float radius, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * radius * 2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleTexture();
        sr.color  = color;
        sr.sortingOrder = 5;
        return go;
    }

    static Sprite CreateCircleTexture()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 2f;
        float thickness = 6f;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = 0f;

            // Ring: między r-thickness a r
            if (dist >= r - thickness && dist <= r)
                alpha = 1f;
            // Wewnętrzna kropka (dla InnerRing - mały kółek bez dziury)
            else if (dist < r - thickness - 2f)
                alpha = 0f;

            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}