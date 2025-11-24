using UnityEngine;

public class Entrada : MonoBehaviour
{
    [Header("Referencias")]
    public MatrizTablero tablero;
    public BaseConocimiento baseConocimiento;
    public Inferencia motorInferencia;   // IA
    public GestorTurnos gestorTurnos;    // Control de turnos

    private Vector2Int casillaInicial;
    private Vector2Int casillaDestino;
    private GameObject fichaSeleccionada;
    private bool movimientoEnProgreso = false;
    private bool entradaHabilitada = false;

    // ==========================================================
    //           SELECCIÓN DE FICHA (START DRAG)
    // ==========================================================
    public void RegistrarSeleccion(GameObject ficha)
    {
        if (!entradaHabilitada) return;

        fichaSeleccionada = ficha;
        casillaInicial = ObtenerCoordenadasDesdePosicion(ficha.transform.position);
        movimientoEnProgreso = true;
    }

    // ==========================================================
    //           SOLTAR FICHA (END DRAG)
    // ==========================================================
    public void RegistrarMovimiento(Vector3 posicionFinal)
    {
        if (!movimientoEnProgreso || fichaSeleccionada == null)
            return;

        casillaDestino = ObtenerCoordenadasDesdePosicion(posicionFinal);

        // Validar que el movimiento esté dentro del tablero
        if (!EsMovimientoDentroDelTablero(casillaDestino))
        {
            Debug.Log("❌ Movimiento fuera del tablero. Se regresa.");
            ResetFicha();
            Finalizar();
            return;
        }

        PiezaAjedrez piezaScript = fichaSeleccionada.GetComponent<PiezaAjedrez>();
        if (piezaScript == null)
        {
            Debug.LogError("❌ La ficha seleccionada no tiene el componente PiezaAjedrez.");
            ResetFicha();
            Finalizar();
            return;
        }

        // Crear el movimiento con PiezaAjedrez
        Movimiento movimiento = new Movimiento(casillaInicial, casillaDestino, piezaScript);

        // Verificar movimiento legal en BaseConocimiento
        bool esLegal = baseConocimiento.VerificarMovimientoLegal(movimiento);

        if (esLegal)
        {
            string casilla = LenguajeAjedrez.ANotacion(casillaDestino.x, casillaDestino.y, tablero.filas);
            Debug.Log($"✔ Movimiento legal hacia {casilla}");

            // Mover la ficha visualmente
            fichaSeleccionada.transform.position =
                tablero.ObtenerPosicionCasilla(casillaDestino.x, casillaDestino.y);

            // Avisar al Gestor de Turnos
            if (gestorTurnos != null)
                gestorTurnos.JugadaHumanoCompletada();
        }
        else
        {
            Debug.Log("❌ Movimiento ilegal. Se revierte.");
            ResetFicha();
        }

        Finalizar();
    }

    private void Finalizar()
    {
        movimientoEnProgreso = false;
        fichaSeleccionada = null;
    }

    // ==========================================================
    //              CALCULAR COORDENADAS DEL TABLERO
    // ==========================================================
    private Vector2Int ObtenerCoordenadasDesdePosicion(Vector3 posicion)
    {
        float anchoCasilla = tablero.fondoTablero.sprite.bounds.size.x *
                             tablero.fondoTablero.transform.localScale.x /
                             tablero.columnas;

        float altoCasilla = tablero.fondoTablero.sprite.bounds.size.y *
                             tablero.fondoTablero.transform.localScale.y /
                             tablero.filas;

        Vector3 esquinaInferiorIzquierda = tablero.fondoTablero.bounds.min;

        int columna = Mathf.Clamp(
            Mathf.FloorToInt((posicion.x - esquinaInferiorIzquierda.x) / anchoCasilla),
            0, tablero.columnas - 1
        );

        int fila = Mathf.Clamp(
            Mathf.FloorToInt((posicion.y - esquinaInferiorIzquierda.y) / altoCasilla),
            0, tablero.filas - 1
        );

        return new Vector2Int(fila, columna);
    }

    // ==========================================================
    //                     VALIDACIONES
    // ==========================================================
    private bool EsMovimientoDentroDelTablero(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < tablero.filas &&
               coords.y >= 0 && coords.y < tablero.columnas;
    }

    private void ResetFicha()
    {
        if (fichaSeleccionada != null)
        {
            fichaSeleccionada.transform.position =
                tablero.ObtenerPosicionCasilla(casillaInicial.x, casillaInicial.y);
        }
    }

    // ==========================================================
    //             CONTROL DE ENTRADA DEL JUGADOR
    // ==========================================================
    public void HabilitarEntrada(bool habilitar)
    {
        entradaHabilitada = habilitar;

        if (habilitar)
            Debug.Log("🎮 Turno del humano: entrada ACTIVADA.");
        else
            Debug.Log("🛑 Turno del humano: entrada DESACTIVADA.");
    }

    public bool EstaHabilitada()
    {
        return entradaHabilitada;
    }
}
