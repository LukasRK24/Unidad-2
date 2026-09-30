using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Singleton de efectos: escucha los eventos y suelta las particulas.
// Las particulas se reciclan con Object Pool en vez de crearlas y destruirlas cada vez
public class ControladorEfectos : Singleton<ControladorEfectos>
{
    [SerializeField] private ParticleSystem prefabPolvo;
    [SerializeField] private ParticleSystem prefabDash;
    [SerializeField] private ParticleSystem prefabChispa;
    [SerializeField] private ParticleSystem prefabGolpe;

    // Un pool por cada tipo de particula
    private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> pools = new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();

    private void OnEnable()
    {
        EventosJuego.AlSaltarJugador += ManejarSalto;
        EventosJuego.AlAterrizarJugador += ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador += ManejarDash;
        EventosJuego.AlHerirJugador += ManejarHerido;
        EventosJuego.AlRecogerFruta += ManejarFruta;
        EventosJuego.AlDerrotarEnemigo += ManejarEnemigoDerrotado;
    }

    private void OnDisable()
    {
        EventosJuego.AlSaltarJugador -= ManejarSalto;
        EventosJuego.AlAterrizarJugador -= ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador -= ManejarDash;
        EventosJuego.AlHerirJugador -= ManejarHerido;
        EventosJuego.AlRecogerFruta -= ManejarFruta;
        EventosJuego.AlDerrotarEnemigo -= ManejarEnemigoDerrotado;
    }

    public void Reproducir(ParticleSystem prefab, Vector3 posicion)
    {
        if (prefab == null) return;

        // Si no hay pool para esta particula todavia, se crea
        if (!pools.TryGetValue(prefab, out ObjectPool<ParticleSystem> pool))
        {
            pool = new ObjectPool<ParticleSystem>(
                () => Instantiate(prefab, transform),
                particula => particula.gameObject.SetActive(true),
                particula => particula.gameObject.SetActive(false),
                particula => Destroy(particula.gameObject),
                false, 8, 32);
            pools.Add(prefab, pool);
        }

        ParticleSystem instancia = pool.Get();
        instancia.transform.position = posicion;
        instancia.Play();
        StartCoroutine(DevolverAlTerminar(instancia, pool));
    }

    // Cuando la particula termina vuelve al pool
    private IEnumerator DevolverAlTerminar(ParticleSystem instancia, ObjectPool<ParticleSystem> pool)
    {
        ParticleSystem.MainModule principal = instancia.main;
        yield return new WaitForSeconds(principal.duration + principal.startLifetime.constantMax);
        if (instancia != null && instancia.gameObject.activeSelf) pool.Release(instancia);
    }

    private void ManejarSalto(Vector3 posicion) => Reproducir(prefabPolvo, posicion);
    private void ManejarAterrizaje(Vector3 posicion) => Reproducir(prefabPolvo, posicion);
    private void ManejarDash(Vector3 posicion, float direccion) => Reproducir(prefabDash, posicion);
    private void ManejarHerido(Vector3 posicion) => Reproducir(prefabGolpe, posicion);
    private void ManejarFruta(Vector3 posicion) => Reproducir(prefabChispa, posicion);
    private void ManejarEnemigoDerrotado(Vector3 posicion) => Reproducir(prefabGolpe, posicion);
}
