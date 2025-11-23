using UnityEngine;

[System.Serializable]
public class Movimiento
{
    public Vector2Int inicio;
    public Vector2Int destino;
    public GameObject ficha;

    public Movimiento(Vector2Int i, Vector2Int d, GameObject f)
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
