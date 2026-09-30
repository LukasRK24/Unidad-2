using UnityEngine;

// Todo lo que pueda recibir danio (jugador, enemigos) usa esta interfaz
public interface IRecibeDanio
{
    void RecibirDanio(int cantidad, Vector2 puntoGolpe);
}
