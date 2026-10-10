using UnityEngine;

// Da sensación de profundidad (3D) moviendo el fondo más lento que la cámara principal
public class Parallax : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float efectoParallax = 0.5f;
    private Transform camara;
    private Vector3 posicionAnteriorCamara;

    private void Start()
    {
        if (Camera.main != null)
        {
            camara = Camera.main.transform;
            posicionAnteriorCamara = camara.position;
        }
    }

    private void LateUpdate()
    {
        if (camara == null) return;
        
        Vector3 deltaMovimiento = camara.position - posicionAnteriorCamara;
        transform.position += new Vector3(deltaMovimiento.x * efectoParallax, deltaMovimiento.y * efectoParallax, 0);
        posicionAnteriorCamara = camara.position;
    }
}
