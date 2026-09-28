using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Observer: el HUD solo se suscribe a eventos; no hay referencias al jugador ni lógica en Update().
public class HUDController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthLabel;
    [SerializeField] private Image damageOverlay;

    [Header("Frutas y mensajes")]
    [SerializeField] private TMP_Text fruitsLabel;
    [SerializeField] private TMP_Text hintLabel;

    private Coroutine blinkRoutine;
    private Coroutine flashRoutine;
    private Coroutine hintRoutine;
    private Color healthColor;

    private void Awake()
    {
        healthColor = healthFill.color;
        damageOverlay.color = new Color(1f, 0f, 0f, 0f);
        hintLabel.text = string.Empty;
    }

    private void OnEnable()
    {
        GameEvents.OnHealthChanged += UpdateHealth;
        GameEvents.OnFruitsChanged += UpdateFruits;
        GameEvents.OnPlayerHurt += HandleHurt;
        GameEvents.OnHint += ShowHint;
    }

    private void OnDisable()
    {
        GameEvents.OnHealthChanged -= UpdateHealth;
        GameEvents.OnFruitsChanged -= UpdateFruits;
        GameEvents.OnPlayerHurt -= HandleHurt;
        GameEvents.OnHint -= ShowHint;
    }

    private void UpdateHealth(int current, int max)
    {
        healthFill.fillAmount = max > 0 ? (float)current / max : 0f;
        healthLabel.text = $"{current}/{max}";
    }

    private void UpdateFruits(int collected, int total)
    {
        fruitsLabel.text = $"{collected} / {total}";
    }

    private void HandleHurt(Vector3 position)
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        blinkRoutine = StartCoroutine(BlinkHealthBar());
        flashRoutine = StartCoroutine(FlashOverlay());
    }

    private void ShowHint(string message)
    {
        if (hintRoutine != null) StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(HintRoutine(message));
    }

    private IEnumerator BlinkHealthBar()
    {
        for (int i = 0; i < 6; i++)
        {
            healthFill.color = i % 2 == 0 ? Color.white : healthColor;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        healthFill.color = healthColor;
    }

    private IEnumerator FlashOverlay()
    {
        float duration = 0.3f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            damageOverlay.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.45f, 0f, elapsed / duration));
            yield return null;
        }
        damageOverlay.color = new Color(1f, 0f, 0f, 0f);
    }

    private IEnumerator HintRoutine(string message)
    {
        hintLabel.text = message;
        yield return new WaitForSecondsRealtime(2.2f);
        hintLabel.text = string.Empty;
    }
}
