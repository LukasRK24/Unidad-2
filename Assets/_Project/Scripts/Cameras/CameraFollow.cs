using System.Collections;
using UnityEngine;

// Cámara dinámica: zona muerta (dead zone), amortiguación (damping), adelanto (look-ahead),
// límites del nivel y sacudida (shake) disparada por eventos.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
    [SerializeField] private Vector2 deadZone = new Vector2(1.5f, 1f);
    [SerializeField] private float dampingTime = 0.25f;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmoothing = 4f;
    [SerializeField] private Vector2 levelMin = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 levelMax = new Vector2(100f, 100f);

    private Camera cam;
    private Rigidbody2D targetBody;
    private Vector3 velocity;
    private Vector3 shakeOffset;
    private float lookAhead;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null) targetBody = target.GetComponent<Rigidbody2D>();
    }

    private void OnEnable() => GameEvents.OnCameraShake += Shake;
    private void OnDisable() => GameEvents.OnCameraShake -= Shake;

    private void Start()
    {
        if (target == null) return;
        Vector3 start = ClampToLevel(DesiredPosition(transform.position));
        transform.position = new Vector3(start.x, start.y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float aheadTarget = 0f;
        if (targetBody != null && Mathf.Abs(targetBody.linearVelocity.x) > 0.5f)
            aheadTarget = Mathf.Sign(targetBody.linearVelocity.x) * lookAheadDistance;
        lookAhead = Mathf.Lerp(lookAhead, aheadTarget, Time.deltaTime * lookAheadSmoothing);

        Vector3 current = transform.position - shakeOffset;
        Vector3 desired = ClampToLevel(DesiredPosition(current));
        Vector3 smoothed = Vector3.SmoothDamp(current, desired, ref velocity, dampingTime);
        smoothed.z = transform.position.z;
        transform.position = smoothed + shakeOffset;
    }

    private Vector3 DesiredPosition(Vector3 current)
    {
        Vector2 focus = (Vector2)target.position + offset + new Vector2(lookAhead, 0f);
        float x = current.x;
        float y = current.y;

        float dx = focus.x - x;
        if (Mathf.Abs(dx) > deadZone.x) x += dx - Mathf.Sign(dx) * deadZone.x;

        float dy = focus.y - y;
        if (Mathf.Abs(dy) > deadZone.y) y += dy - Mathf.Sign(dy) * deadZone.y;

        return new Vector3(x, y, current.z);
    }

    private Vector3 ClampToLevel(Vector3 position)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        position.x = ClampAxis(position.x, levelMin.x + halfWidth, levelMax.x - halfWidth);
        position.y = ClampAxis(position.y, levelMin.y + halfHeight, levelMax.y - halfHeight);
        return position;
    }

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max) return (min + max) * 0.5f;
        return Mathf.Clamp(value, min, max);
    }

    private void Shake(float duration, float strength)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector2 random = Random.insideUnitCircle * strength;
            shakeOffset = new Vector3(random.x, random.y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
