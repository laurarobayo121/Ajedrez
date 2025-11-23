using UnityEngine;
using System.Collections.Generic;

public class BaseConocimiento : MonoBehaviour
{
    private MatrizTablero tablero;

    // Mapa de posiciones lógicas -> GameObject de la pieza
    private Dictionary<Vector2Int, GameObject> piezasPorPosicion = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        tablero = FindObjectOfType<MatrizTablero>();
        ActualizarRegistroPiezas();
    }

    /// <summary>
    /// Verifica si un movimiento es legal según la pieza que se mueve.
    /// Si es legal, actualiza la posición de la pieza y el registro interno.
    /// </summary>
    public bool VerificarMovimientoLegal(Movimiento movimiento)
    {
        // Obtener el script PiezaAjedrez de la ficha movida
        PiezaAjedrez pieza = movimiento.ficha.GetComponent<PiezaAjedrez>();
        if (pieza == null)
        {
            Debug.LogError("[BaseConocimiento] La ficha movida no tiene componente PiezaAjedrez");
            return false;
        }

        // Verificar si el movimiento está en la lista de movimientos legales de la pieza
        bool movimientoLegal = pieza.EsMovimientoLegal(movimiento.destino);

        if (movimientoLegal)
        {
            Debug.Log($"[BaseConocimiento] Movimiento LEGAL de {pieza.tipoPieza} {pieza.colorPieza} desde {movimiento.inicio} hasta {movimiento.destino}");

            // Actualizar la posición lógica de la pieza
            pieza.ActualizarPosicion(movimiento.destino);

            // Volver a escanear todas las piezas y rehacer el diccionario
            ActualizarRegistroPiezas();
        }
        else
        {
            Debug.LogWarning($"[BaseConocimiento] Movimiento ILEGAL de {pieza.tipoPieza} {pieza.colorPieza} desde {movimiento.inicio} hasta {movimiento.destino}");
        }

        return movimientoLegal;
    }

    /// <summary>
    /// Recorre todas las piezas de la escena y actualiza el diccionario
    /// posición -> GameObject de pieza.
    /// </summary>
    private void ActualizarRegistroPiezas()
    {
        piezasPorPosicion.Clear();

        // Buscar todas las piezas en el tablero (en la escena)
        PiezaAjedrez[] todasLasPiezas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (PiezaAjedrez pieza in todasLasPiezas)
        {
            if (!piezasPorPosicion.ContainsKey(pieza.posicionActual))
            {
                piezasPorPosicion.Add(pieza.posicionActual, pieza.gameObject);
            }
        }
    }

    /// <summary>
    /// Método público para refrescar el diccionario desde fuera (lo usa la IA).
    /// </summary>
    public void Refrescar()
    {
        ActualizarRegistroPiezas();
    }

    /// <summary>
    /// Devuelve la pieza (GameObject) que está en una posición lógica dada.
    /// </summary>
    public GameObject ObtenerPiezaEnPosicion(Vector2Int posicion)
    {
        piezasPorPosicion.TryGetValue(posicion, out GameObject pieza);
        return pieza;
    }

    /// <summary>
    /// Indica si hay alguna pieza en esa posición lógica.
    /// </summary>
    public bool HayPiezaEnPosicion(Vector2Int posicion)
    {
        return piezasPorPosicion.ContainsKey(posicion);
    }

    /// <summary>
    /// Devuelve el color de la pieza en esa posición (si existe).
    /// </summary>
    public PiezaAjedrez.ColorPieza ObtenerColorPiezaEnPosicion(Vector2Int posicion)
    {
        if (piezasPorPosicion.TryGetValue(posicion, out GameObject pieza))
        {
            PiezaAjedrez scriptPieza = pieza.GetComponent<PiezaAjedrez>();
            if (scriptPieza != null)
                return scriptPieza.colorPieza;
        }

        // Valor por defecto (por si no hay pieza)
        return PiezaAjedrez.ColorPieza.Blanco;
    }
    
}
