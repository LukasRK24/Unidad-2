using UnityEngine;

// Fruta coleccionable: solo anuncia que fue recogida; el conteo lo lleva GameManager.
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 0.15f;

    private Vector3 startPosition;
    private bool collected;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        GameEvents.FruitCollected(transform.position);
        Destroy(gameObject);
    }
}
