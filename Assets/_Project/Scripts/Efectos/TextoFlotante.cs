using UnityEngine;
using TMPro;

// Texto que flota hacia arriba y desaparece lentamente usando Object Pool
public class TextoFlotante : MonoBehaviour
{
    [SerializeField] private float velocidadSubida = 1.5f;
    [SerializeField] private float duracion = 1f;
    [SerializeField] private TextMeshPro texto;

    private float tiempoAparicion;

    private void OnEnable()
    {
        tiempoAparicion = Time.time;
        // Reiniciar la transparencia
        if (texto != null)
        {
            Color c = texto.color;
            c.a = 1f;
            texto.color = c;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * velocidadSubida * Time.deltaTime;
        
        float progreso = (Time.time - tiempoAparicion) / duracion;
        
        if (texto != null)
        {
            Color c = texto.color;
            // Curva lineal simple para desvanecerse
            c.a = Mathf.Clamp01(1f - progreso);
            texto.color = c;
        }

        if (progreso >= 1f)
        {
            gameObject.SetActive(false); // Vuelve al Pool
        }
    }

    public void Configurar(string mensaje, Color color)
    {
        if (texto != null)
        {
            texto.text = mensaje;
            texto.color = color;
        }
    }
}
