using UnityEngine;
using System.Collections.Generic; 

public class Entrada : MonoBehaviour
{
    [Header("Referencias")]
    public MatrizTablero tablero;
    public BaseConocimiento baseConocimiento;
    private Vector2Int casillaInicial;
    private Vector2Int casillaDestino;
    private GameObject fichaSeleccionada;
    private bool movimientoEnProgreso = false;
    public Inferencia motorInferencia;  // referencia al script de la IA
    public GestorTurnos gestorTurnos;   // referencia al gestor de turnos
    private bool entradaHabilitada = false;

    // Llamado desde FichaInteractiva cuando el jugador empieza a mover
    public void RegistrarSeleccion(GameObject ficha)
    {
        if (!entradaHabilitada) return;

        fichaSeleccionada = ficha;
        casillaInicial = ObtenerCoordenadasDesdePosicion(ficha.transform.position);
        movimientoEnProgreso = true;
    }

    // Llamado desde FichaInteractiva cuando el jugador suelta la ficha
    public void RegistrarMovimiento(Vector3 posicionFinal)
    {
        if (!movimientoEnProgreso || fichaSeleccionada == null)
            return;

        casillaDestino = ObtenerCoordenadasDesdePosicion(posicionFinal);

        // Validar que el movimiento esté dentro del tablero
        if (!EsMovimientoDentroDelTablero(casillaDestino))
        {
            Debug.Log("Movimiento fuera del tablero. Se cancela.");
            ResetFicha();
            movimientoEnProgreso = false;
            fichaSeleccionada = null;
            return;
        }

        // Crear una jugada estructurada
        Movimiento movimiento = new Movimiento(casillaInicial, casillaDestino, fichaSeleccionada);

        // Enviar a la base de conocimiento
        bool esLegal = baseConocimiento.VerificarMovimientoLegal(movimiento);

        if (esLegal)
        {
            string nombreCasilla = LenguajeAjedrez.ANotacion(casillaDestino.x, casillaDestino.y, tablero.filas);
            Debug.Log("Movimiento legal: " + nombreCasilla);

            // Mover físicamente la ficha al centro de la casilla destino
            fichaSeleccionada.transform.position =
                tablero.ObtenerPosicionCasilla(casillaDestino.x, casillaDestino.y);

            // ✅ Solo aquí avisamos que el jugador terminó su turno
            if (gestorTurnos != null)
            {
                gestorTurnos.JugadaHumanoCompletada();
            }
        }
        else
        {
            Debug.Log("Movimiento ilegal. Revirtiendo.");
            ResetFicha();
        }

        movimientoEnProgreso = false;
        fichaSeleccionada = null;
    }

    private Vector2Int ObtenerCoordenadasDesdePosicion(Vector3 posicion)
    {
        float anchoCasilla = tablero.fondoTablero.sprite.bounds.size.x * tablero.fondoTablero.transform.localScale.x / tablero.columnas;
        float altoCasilla = tablero.fondoTablero.sprite.bounds.size.y * tablero.fondoTablero.transform.localScale.y / tablero.filas;

        Vector3 esquinaInferiorIzquierda = tablero.fondoTablero.bounds.min;

        int columna = Mathf.Clamp(Mathf.FloorToInt((posicion.x - esquinaInferiorIzquierda.x) / anchoCasilla), 0, tablero.columnas - 1);
        int fila = Mathf.Clamp(Mathf.FloorToInt((posicion.y - esquinaInferiorIzquierda.y) / altoCasilla), 0, tablero.filas - 1);

        return new Vector2Int(fila, columna);
    }

    private bool EsMovimientoDentroDelTablero(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < tablero.filas && coords.y >= 0 && coords.y < tablero.columnas;
    }

    private void ResetFicha()
    {
        if (fichaSeleccionada != null)
        {
            fichaSeleccionada.transform.position =
                tablero.ObtenerPosicionCasilla(casillaInicial.x, casillaInicial.y);
        }
    }

    public void HabilitarEntrada(bool habilitar)
    {
        entradaHabilitada = habilitar;

        if (habilitar)
            Debug.Log("Turno del humano: entrada activada.");
        else
            Debug.Log("Turno del humano finalizado. Deshabilitando entrada.");
    } 

    public bool EstaHabilitada()
    {
        return entradaHabilitada;
    }
}
