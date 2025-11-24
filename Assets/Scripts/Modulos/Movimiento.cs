using UnityEngine;

[System.Serializable]
public class Movimiento
{
    public Vector2Int inicio;
    public Vector2Int destino;
    public PiezaAjedrez ficha; // Cambiado de GameObject a PiezaAjedrez

    public Movimiento(Vector2Int i, Vector2Int d, PiezaAjedrez f) // también PiezaAjedrez
    {
        inicio = i;
        destino = d;
        ficha = f;
    }

    public override string ToString()
    {
        return $"({inicio.x},{inicio.y}) → ({destino.x},{destino.y})";
    }
}
