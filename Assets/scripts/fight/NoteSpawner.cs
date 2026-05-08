using UnityEngine;
using System.Collections;

/// <summary>
/// Spawnuje wizualne "nuty" (kółka) zsynchronizowane z beatami.
/// Nota leci od punktu startowego do celu (hit zone) i tam powinna
/// dotrzeć dokładnie w momencie beatu — gracz klika gdy dotrze.
///
/// Ustaw w Inspectorze:
///   noteпрефab  — sprite kółka (np. biały okrąg)
///   spawnPoint  — Transform poza ekranem (np. lewy bok)
///   hitZone     — Transform w centrum (target ring)
///   beatsLeadIn — ile beatów przed docelowym beatem nota zostaje spawnowana
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Referencje")]
    public GameObject notePrefab;   // Prefab noty (kółko sprite)
    public Transform  spawnPoint;   // Skąd startuje nota
    public Transform  hitZone;      // Dokąd zmierza nota

    [Header("Timing")]
    [Tooltip("Ile beatów wcześniej nota pojawia się na ekranie")]
    public int beatsLeadIn = 2;

    [Header("Wygląd")]
    public Color noteColor   = Color.white;
    public float noteScale   = 1f;
    public float hitZoneSize = 1.2f; // skala kółka docelowego

    // Czy aktualnie spawnujemy nuty
    private bool isActive = false;

    // -------------------------------------------------------

    void Start()
    {
        if (RhytmManager.Instance != null)
            RhytmManager.Instance.OnBeat += OnBeat;

        if (FightManager.Instance != null)
        {
            FightManager.Instance.OnFightStarted += () => isActive = true;
            FightManager.Instance.OnFightEnded   += _ => isActive = false;
        }
    }

    void OnDestroy()
    {
        if (RhytmManager.Instance != null)
            RhytmManager.Instance.OnBeat -= OnBeat;
    }

    // -------------------------------------------------------

    void OnBeat(int beatNumber)
    {
        if (!isActive) return;
        SpawnNote();
    }

    void SpawnNote()
    {
        if (notePrefab == null || spawnPoint == null || hitZone == null)
        {
            Debug.LogWarning("[NoteSpawner] Brak referencji — ustaw w Inspectorze");
            return;
        }

        GameObject note = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        note.transform.localScale = Vector3.one * noteScale;

        // Kolor
        var sr = note.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = noteColor;

        // Animujemy notę: ma dotrzeć do hitZone w ciągu (beatsLeadIn * BeatInterval) sekund
        float travelTime = beatsLeadIn * RhytmManager.Instance.BeatInterval;
        StartCoroutine(MoveNote(note, spawnPoint.position, hitZone.position, travelTime));
    }

    IEnumerator MoveNote(GameObject note, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (note == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-in-out dla ładniejszego ruchu
            float smooth = t * t * (3f - 2f * t);
            note.transform.position = Vector3.Lerp(from, to, smooth);

            yield return null;
        }

        if (note != null)
        {
            note.transform.position = to;
            // Mały efekt "uderzenia" — nota pulsuje i znika
            StartCoroutine(HitEffect(note));
        }
    }

    IEnumerator HitEffect(GameObject note)
    {
        var sr = note.GetComponent<SpriteRenderer>();
        float t = 0f;
        float duration = 0.15f;
        Vector3 originalScale = note.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float scale = Mathf.Lerp(1f, 1.4f, progress);
            note.transform.localScale = originalScale * scale;

            if (sr != null)
                sr.color = new Color(noteColor.r, noteColor.g, noteColor.b, 1f - progress);

            yield return null;
        }

        Destroy(note);
    }
}