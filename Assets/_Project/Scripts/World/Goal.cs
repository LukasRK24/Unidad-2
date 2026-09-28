using UnityEngine;

// Meta del nivel: avisa al llegar; GameManager decide si el nivel está completo.
[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    [SerializeField] private float repeatSeconds = 2f;

    private float nextAllowedTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || Time.time < nextAllowedTime) return;

        nextAllowedTime = Time.time + repeatSeconds;
        GameEvents.GoalReached();
    }
}
