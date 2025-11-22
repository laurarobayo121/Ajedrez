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
        // Obtener el script PiezaAjedrez
        PiezaAjedrez pieza = movimiento.ficha.GetComponent<PiezaAjedrez>();
        if (pieza == null)
        {
            Debug.LogError("La ficha movida no tiene componente PiezaAjedrez");
            return false;
        }

        bool movimientoLegal = pieza.EsMovimientoLegal(movimiento.destino);

        if (movimientoLegal)
        {
            Debug.Log($"Movimiento LEGAL de {pieza.tipoPieza} {pieza.colorPieza} desde {movimiento.inicio} hasta {movimiento.destino}");

            // 1️⃣ ¿Hay una pieza en el destino?
            GameObject piezaDestinoGO = ObtenerPiezaEnPosicion(movimiento.destino);

            if (piezaDestinoGO != null)
            {
                PiezaAjedrez piezaDestino = piezaDestinoGO.GetComponent<PiezaAjedrez>();

                // 2️⃣ SOLO destruir si es del otro color
                if (piezaDestino != null && piezaDestino.colorPieza != pieza.colorPieza)
                {
                    Debug.Log($"CAPTURA: {pieza.tipoPieza} ({pieza.colorPieza}) come a {piezaDestino.tipoPieza} ({piezaDestino.colorPieza})");
                    Destroy(piezaDestinoGO);
                }
            }

            // 3️⃣ actualizar posición lógica
            pieza.ActualizarPosicion(movimiento.destino);

            // 4️⃣ actualizar el registro interno
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

        PiezaAjedrez[] todasLasPiezas = FindObjectsOfType<PiezaAjedrez>();
        foreach (PiezaAjedrez pieza in todasLasPiezas)
        {
            piezasPorPosicion[pieza.posicionActual] = pieza.gameObject;
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

    public PiezaAjedrez.ColorPieza ObtenerColorPiezaEnPosicion(Vector2Int posicion)
    {
        if (piezasPorPosicion.TryGetValue(posicion, out GameObject pieza))
        {
            PiezaAjedrez scriptPieza = pieza.GetComponent<PiezaAjedrez>();
            return scriptPieza.colorPieza;
        }
        return PiezaAjedrez.ColorPieza.Blanco;
    }
}
