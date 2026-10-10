using UnityEngine;

// Todo lo que pueda recibir danio (jugador, enemigos, objetos) usa esta interfaz
public interface IRecibeDanio
{
    bool EstaMuerto { get; }
    void RecibirDanio(int cantidad, Vector2 puntoGolpe);
    void Matar();
}
