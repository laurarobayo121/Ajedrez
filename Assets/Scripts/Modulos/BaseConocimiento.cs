using UnityEngine;
using System.Collections.Generic;

public class BaseConocimiento : MonoBehaviour
{
    private MatrizTablero tablero;
    private Dictionary<Vector2Int, GameObject> piezasPorPosicion = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        tablero = FindObjectOfType<MatrizTablero>();
        ActualizarRegistroPiezas();
    }

    public bool VerificarMovimientoLegal(Movimiento movimiento)
    {
        // Obtener el script PiezaAjedrez de la ficha movida
        PiezaAjedrez pieza = movimiento.ficha.GetComponent<PiezaAjedrez>();
        if (pieza == null)
        {
            Debug.LogError("La ficha movida no tiene componente PiezaAjedrez");
            return false;
        }

        // Verificar si el movimiento está en la lista de movimientos legales de la pieza
        bool movimientoLegal = pieza.EsMovimientoLegal(movimiento.destino);

        if (movimientoLegal)
        {
            Debug.Log($"Movimiento LEGAL de {pieza.tipoPieza} {pieza.colorPieza} desde {movimiento.inicio} hasta {movimiento.destino}");
            
            // Actualizar la posición de la pieza
            pieza.ActualizarPosicion(movimiento.destino);
            
            // Actualizar el registro de piezas
            ActualizarRegistroPiezas();
        }
        else
        {
            Debug.LogWarning($"Movimiento ILEGAL de {pieza.tipoPieza} {pieza.colorPieza} desde {movimiento.inicio} hasta {movimiento.destino}");
        }

        return movimientoLegal;
    }

    private void ActualizarRegistroPiezas()
    {
        piezasPorPosicion.Clear();
        
        // Buscar todas las piezas en el tablero
        PiezaAjedrez[] todasLasPiezas = FindObjectsOfType<PiezaAjedrez>();
        foreach (PiezaAjedrez pieza in todasLasPiezas)
        {
            if (!piezasPorPosicion.ContainsKey(pieza.posicionActual))
            {
                piezasPorPosicion.Add(pieza.posicionActual, pieza.gameObject);
            }
        }
    }

    public GameObject ObtenerPiezaEnPosicion(Vector2Int posicion)
    {
        piezasPorPosicion.TryGetValue(posicion, out GameObject pieza);
        return pieza;
    }

    public bool HayPiezaEnPosicion(Vector2Int posicion)
    {
        return piezasPorPosicion.ContainsKey(posicion);
    }

    // ESTA ES LA LÍNEA CORREGIDA - usa PiezaAjedrez.ColorPieza
    public PiezaAjedrez.ColorPieza ObtenerColorPiezaEnPosicion(Vector2Int posicion)
    {
        if (piezasPorPosicion.TryGetValue(posicion, out GameObject pieza))
        {
            PiezaAjedrez scriptPieza = pieza.GetComponent<PiezaAjedrez>();
            return scriptPieza.colorPieza;
        }
        return PiezaAjedrez.ColorPieza.Blanco; // Valor por defecto
    }
}