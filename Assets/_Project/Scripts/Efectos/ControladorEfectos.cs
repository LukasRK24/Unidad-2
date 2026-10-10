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

    [Header("Textos (Juice)")]
    [SerializeField] private TextoFlotante prefabTextoFlotante;
    private ObjectPool<TextoFlotante> poolTextos;

    private void Awake()
    {
        // El pool de textos se crea en Awake
        poolTextos = new ObjectPool<TextoFlotante>(
            () => Instantiate(prefabTextoFlotante, transform),
            texto => texto.gameObject.SetActive(true),
            texto => texto.gameObject.SetActive(false),
            texto => Destroy(texto.gameObject),
            false, 10, 30);
    }

    private void OnEnable()
    {
        EventosJuego.AlSaltarJugador += ManejarSalto;
        EventosJuego.AlAterrizarJugador += ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador += ManejarDash;
        EventosJuego.AlHerirJugador += ManejarHerido;
        EventosJuego.AlRecogerFruta += ManejarFruta;
        EventosJuego.AlDerrotarEnemigo += ManejarEnemigoDerrotado;
        EventosJuego.AlHacerCombo += ManejarCombo;
    }

    private void OnDisable()
    {
        EventosJuego.AlSaltarJugador -= ManejarSalto;
        EventosJuego.AlAterrizarJugador -= ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador -= ManejarDash;
        EventosJuego.AlHerirJugador -= ManejarHerido;
        EventosJuego.AlRecogerFruta -= ManejarFruta;
        EventosJuego.AlDerrotarEnemigo -= ManejarEnemigoDerrotado;
        EventosJuego.AlHacerCombo -= ManejarCombo;
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
    
    private void ManejarFruta(Vector3 posicion) 
    {
        Reproducir(prefabChispa, posicion);
        if (prefabTextoFlotante != null)
        {
            TextoFlotante texto = poolTextos.Get();
            texto.transform.position = posicion + Vector3.up * 0.5f;
            texto.Configurar("+1", Color.yellow);
        }
    }
    
    private void ManejarEnemigoDerrotado(Vector3 posicion) 
    {
        Reproducir(prefabGolpe, posicion);
        if (prefabTextoFlotante != null)
        {
            TextoFlotante texto = poolTextos.Get();
            texto.transform.position = posicion + Vector3.up * 0.5f;
            texto.Configurar("POW!", Color.red);
        }
    }
    
    private void ManejarCombo(Vector3 posicion, int cantidad) 
    {
        if (prefabTextoFlotante != null)
        {
            TextoFlotante texto = poolTextos.Get();
            texto.transform.position = posicion + Vector3.up * 1f;
            texto.Configurar("x" + cantidad + "!", Color.magenta);
        }
    }
}
