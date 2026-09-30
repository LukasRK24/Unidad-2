using UnityEngine;

// Animacion simple cambiando de sprite en orden, para cosas que no necesitan Animator
// (las frutas y los enemigos)
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorSprites : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float fotogramasPorSegundo = 12f;
    [SerializeField] private bool repetir = true;

    private SpriteRenderer renderizador;
    private float tiempo;

    private void Awake()
    {
        renderizador = GetComponent<SpriteRenderer>();
    }

    public void Reproducir(Sprite[] nuevosSprites, float fps, bool repetirAnimacion)
    {
        sprites = nuevosSprites;
        fotogramasPorSegundo = fps;
        repetir = repetirAnimacion;
        tiempo = 0f;
    }

    private void Update()
    {
        if (sprites == null || sprites.Length == 0) return;

        tiempo += Time.deltaTime * fotogramasPorSegundo;
        int indice = repetir ? (int)tiempo % sprites.Length : Mathf.Min((int)tiempo, sprites.Length - 1);
        renderizador.sprite = sprites[indice];
    }
}
