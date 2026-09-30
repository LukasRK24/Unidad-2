using UnityEngine;
using UnityEngine.Tilemaps;

// En Unity 6.6 el Composite Collider no genera la colision al cargar la escena si "Merge" ya viene guardado.
// Volver a aplicar la operacion en Awake obliga a unir los colliders de los tiles en un solo contorno
[RequireComponent(typeof(TilemapCollider2D), typeof(CompositeCollider2D))]
public class InicializadorColisionTilemap : MonoBehaviour
{
    private void Awake()
    {
        TilemapCollider2D tiles = GetComponent<TilemapCollider2D>();
        tiles.compositeOperation = Collider2D.CompositeOperation.None;
        tiles.compositeOperation = Collider2D.CompositeOperation.Merge;
    }
}
