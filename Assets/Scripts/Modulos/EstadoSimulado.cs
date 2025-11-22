using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un estado interno del tablero, independiente de los GameObjects de Unity.
/// Se usa para que la IA pueda "simular" movimientos sin afectar la escena real.
/// </summary>
public class EstadoSimulado
{
    // Diccionarios que guardan qué hay en cada casilla
    public Dictionary<Vector2Int, PiezaAjedrez.ColorPieza> colores;
    public Dictionary<Vector2Int, PiezaAjedrez.TipoPieza> tipos;

    public EstadoSimulado()
    {
        colores = new Dictionary<Vector2Int, PiezaAjedrez.ColorPieza>();
        tipos   = new Dictionary<Vector2Int, PiezaAjedrez.TipoPieza>();
    }

    /// <summary>
    /// Construye el estado simulado leyendo todas las piezas reales de la escena.
    /// </summary>
    public static EstadoSimulado DesdeEscena()
    {
        EstadoSimulado estado = new EstadoSimulado();

        PiezaAjedrez[] piezas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (var p in piezas)
        {
            Vector2Int pos = p.posicionActual;
            estado.colores[pos] = p.colorPieza;
            estado.tipos[pos]   = p.tipoPieza;
        }

        return estado;
    }

    /// <summary>
    /// Crea una copia profunda del estado para usar en Minimax / Alfa-Beta.
    /// </summary>
    public EstadoSimulado Clone()
    {
        EstadoSimulado copia = new EstadoSimulado();

        foreach (var kv in colores)
            copia.colores[kv.Key] = kv.Value;

        foreach (var kv in tipos)
            copia.tipos[kv.Key] = kv.Value;

        return copia;
    }

    /// <summary>
    /// Aplica un movimiento en el estado interno:
    /// mueve la pieza de 'origen' a 'destino', capturando si es necesario.
    /// </summary>
    public void AplicarMovimiento(Vector2Int origen, Vector2Int destino)
    {
        if (!tipos.ContainsKey(origen)) return;

        // Captura si hay pieza en destino
        if (tipos.ContainsKey(destino))
        {
            tipos.Remove(destino);
            colores.Remove(destino);
        }

        // Mover la pieza
        tipos[destino]   = tipos[origen];
        colores[destino] = colores[origen];

        tipos.Remove(origen);
        colores.Remove(origen);
    }

    public bool HayPieza(Vector2Int pos)
    {
        return tipos.ContainsKey(pos);
    }
}
