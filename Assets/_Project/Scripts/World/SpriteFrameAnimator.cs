using UnityEngine;

// Animación simple por fotogramas para objetos sin Animator (frutas, enemigos).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private bool loop = true;

    private SpriteRenderer spriteRenderer;
    private float elapsed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(Sprite[] newFrames, float fps, bool shouldLoop)
    {
        frames = newFrames;
        framesPerSecond = fps;
        loop = shouldLoop;
        elapsed = 0f;
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        elapsed += Time.deltaTime * framesPerSecond;
        int index = loop ? (int)elapsed % frames.Length : Mathf.Min((int)elapsed, frames.Length - 1);
        spriteRenderer.sprite = frames[index];
    }
}
