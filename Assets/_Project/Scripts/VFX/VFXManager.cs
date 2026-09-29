using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Singleton de efectos: escucha eventos y reproduce partículas reutilizadas con Object Pool.
public class VFXManager : Singleton<VFXManager>
{
    [SerializeField] private ParticleSystem dustPrefab;
    [SerializeField] private ParticleSystem dashPrefab;
    [SerializeField] private ParticleSystem sparklePrefab;
    [SerializeField] private ParticleSystem hitPrefab;

    private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> pools = new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();

    private void OnEnable()
    {
        GameEvents.OnPlayerJumped += HandleJump;
        GameEvents.OnPlayerLanded += HandleLand;
        GameEvents.OnPlayerDashed += HandleDash;
        GameEvents.OnPlayerHurt += HandleHurt;
        GameEvents.OnFruitCollected += HandleFruit;
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerJumped -= HandleJump;
        GameEvents.OnPlayerLanded -= HandleLand;
        GameEvents.OnPlayerDashed -= HandleDash;
        GameEvents.OnPlayerHurt -= HandleHurt;
        GameEvents.OnFruitCollected -= HandleFruit;
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
    }

    public void Play(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;

        if (!pools.TryGetValue(prefab, out ObjectPool<ParticleSystem> pool))
        {
            pool = new ObjectPool<ParticleSystem>(
                () => Instantiate(prefab, transform),
                ps => ps.gameObject.SetActive(true),
                ps => ps.gameObject.SetActive(false),
                ps => Destroy(ps.gameObject),
                false, 8, 32);
            pools.Add(prefab, pool);
        }

        ParticleSystem instance = pool.Get();
        instance.transform.position = position;
        instance.Play();
        StartCoroutine(ReleaseAfterPlaying(instance, pool));
    }

    private IEnumerator ReleaseAfterPlaying(ParticleSystem instance, ObjectPool<ParticleSystem> pool)
    {
        ParticleSystem.MainModule main = instance.main;
        yield return new WaitForSeconds(main.duration + main.startLifetime.constantMax);
        if (instance != null && instance.gameObject.activeSelf) pool.Release(instance);
    }

    private void HandleJump(Vector3 position) => Play(dustPrefab, position);
    private void HandleLand(Vector3 position) => Play(dustPrefab, position);
    private void HandleDash(Vector3 position, float direction) => Play(dashPrefab, position);
    private void HandleHurt(Vector3 position) => Play(hitPrefab, position);
    private void HandleFruit(Vector3 position) => Play(sparklePrefab, position);
    private void HandleEnemyDefeated(Vector3 position) => Play(hitPrefab, position);
}
