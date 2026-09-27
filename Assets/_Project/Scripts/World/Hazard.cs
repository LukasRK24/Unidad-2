using UnityEngine;

// Trampa (picos) o zona de muerte (vacío): daña al jugador al tocarla.
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool instantKill;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealth health)) return;

        if (instantKill) health.Kill();
        else health.TakeDamage(damage, transform.position);
    }
}
