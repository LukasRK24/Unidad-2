using System.Collections;
using UnityEngine;

// Juice visual del jugador: squash & stretch al saltar/aterrizar y parpadeo al recibir daño.
public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float recoverSpeed = 14f;
    [SerializeField] private float blinkSeconds = 1.2f;

    private Vector3 baseScale;
    private Coroutine squashRoutine;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        baseScale = visual.localScale;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerJumped += HandleJump;
        GameEvents.OnPlayerLanded += HandleLand;
        GameEvents.OnPlayerHurt += HandleHurt;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerJumped -= HandleJump;
        GameEvents.OnPlayerLanded -= HandleLand;
        GameEvents.OnPlayerHurt -= HandleHurt;
    }

    private void HandleJump(Vector3 position) => Squash(new Vector3(0.75f, 1.3f, 1f));
    private void HandleLand(Vector3 position) => Squash(new Vector3(1.3f, 0.7f, 1f));

    private void HandleHurt(Vector3 position)
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(Blink());
    }

    private void Squash(Vector3 deformation)
    {
        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashRoutine(deformation));
    }

    private IEnumerator SquashRoutine(Vector3 deformation)
    {
        visual.localScale = Vector3.Scale(baseScale, deformation);
        while (Vector3.Distance(visual.localScale, baseScale) > 0.01f)
        {
            visual.localScale = Vector3.Lerp(visual.localScale, baseScale, Time.deltaTime * recoverSpeed);
            yield return null;
        }
        visual.localScale = baseScale;
    }

    private IEnumerator Blink()
    {
        float elapsed = 0f;
        while (elapsed < blinkSeconds)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.08f);
            elapsed += 0.08f;
        }
        spriteRenderer.enabled = true;
    }
}
