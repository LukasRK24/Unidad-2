using UnityEngine;
using UnityEngine.Tilemaps;

// En Unity 6.6 el Composite Collider no genera geometría al cargar la escena con "Merge" serializado;
// reaplicar la operación en Awake fuerza a fusionar los colliders de los tiles en un solo contorno.
[RequireComponent(typeof(TilemapCollider2D), typeof(CompositeCollider2D))]
public class TilemapCompositeInitializer : MonoBehaviour
{
    private void Awake()
    {
        TilemapCollider2D tiles = GetComponent<TilemapCollider2D>();
        tiles.compositeOperation = Collider2D.CompositeOperation.None;
        tiles.compositeOperation = Collider2D.CompositeOperation.Merge;
    }
}
