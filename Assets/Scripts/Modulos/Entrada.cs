using UnityEngine;

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
        if (!entradaHabilitada)
            return;  // 🔒 BLOQUEA LA ENTRADA CUANDO ES TURNO DE LA IA

        fichaSeleccionada = ficha;
        casillaInicial = ObtenerCoordenadasDesdePosicion(ficha.transform.position);
        movimientoEnProgreso = true;
    }


    // Llamado desde FichaInteractiva cuando el jugador suelta la ficha
    public void RegistrarMovimiento(Vector3 posicionFinal)
    {
        if (!entradaHabilitada)
            return;  // 🔒 BLOQUEA LA ENTRADA CUANDO ES TURNO DE LA IA

        if (!movimientoEnProgreso || fichaSeleccionada == null)
            return;

        casillaDestino = ObtenerCoordenadasDesdePosicion(posicionFinal);

        if (!EsMovimientoDentroDelTablero(casillaDestino))
        {
            ResetFicha();
            return;
        }

        Movimiento movimiento = new Movimiento(casillaInicial, casillaDestino, fichaSeleccionada);

        bool esLegal = baseConocimiento.VerificarMovimientoLegal(movimiento);

        if (esLegal)
        {
            Debug.Log("Movimiento legal");
            // Solo notificar si es turno del jugador
            if (gestorTurnos.turnoActual == GestorTurnos.Turno.Humano)
            {
                gestorTurnos.JugadaHumanoCompletada();
            }
        }
        else
        {
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
            fichaSeleccionada.transform.position = tablero.ObtenerPosicionCasilla(casillaInicial.x, casillaInicial.y);
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

// Clase auxiliar para representar un movimiento
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
