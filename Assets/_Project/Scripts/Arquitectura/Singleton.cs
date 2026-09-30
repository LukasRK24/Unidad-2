using UnityEngine;

// Patron Singleton generico: un solo controlador por sistema y se puede usar desde cualquier script
// sin arrastrar referencias en el Inspector
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instancia { get; private set; }

    protected virtual void Awake()
    {
        // Si ya existe otra copia en la escena, esta se destruye
        if (Instancia != null && Instancia != this as T)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (Instancia == this as T)
        {
            Instancia = null;
        }
    }
}
