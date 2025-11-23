using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BaseConocimiento : MonoBehaviour
{
    [Header("Escenas de resultado")]
    [SerializeField] private string escenaVictoriaBlancas = "Victoria";
    [SerializeField] private string escenaVictoriaNegras  = "GameOver";

    private MatrizTablero tablero;

    // Mapa de posiciones lógicas -> GameObject de la pieza
    private Dictionary<Vector2Int, GameObject> piezasPorPosicion =
        new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        tablero = FindObjectOfType<MatrizTablero>();
        ActualizarRegistroPiezas();
    }

    // ==========================================================
    //          MÉTODO PRINCIPAL: VERIFICAR MOVIMIENTO
    // ==========================================================
    public bool VerificarMovimientoLegal(Movimiento movimiento)
    {
        if (movimiento == null || movimiento.ficha == null)
        {
            Debug.LogError("[BaseConocimiento] Movimiento nulo o sin ficha.");
            return false;
        }

        PiezaAjedrez pieza = movimiento.ficha.GetComponent<PiezaAjedrez>();
        if (pieza == null)
        {
            Debug.LogError("[BaseConocimiento] La ficha movida no tiene componente PiezaAjedrez");
            return false;
        }

        Vector2Int origen  = pieza.posicionActual;
        Vector2Int destino = movimiento.destino;

        // Asegurarse que el mapa está al día
        ActualizarRegistroPiezas();

        // 1) No moverse fuera del tablero
        if (!DentroTablero(destino))
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento fuera del tablero.");
            return false;
        }

        // 2) No quedarse en la misma casilla
        if (origen == destino)
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento a la misma casilla.");
            return false;
        }

        // 3) Si hay pieza en destino del mismo color -> no se puede
        if (HayPiezaEnPosicion(destino))
        {
            var colorDestino = ObtenerColorPiezaEnPosicion(destino);
            if (colorDestino == pieza.colorPieza)
            {
                Debug.LogWarning("[BaseConocimiento] No puedes capturar tu propia pieza.");
                return false;
            }
        }

        // 4) Validar movimiento según tipo de pieza (geometría básica)
        if (!EsMovimientoValidoPorTipo(pieza, origen, destino))
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento no válido para " + pieza.tipoPieza);
            return false;
        }

        // 5) Para piezas deslizantes (torre, alfil, reina): comprobar que no haya nada en medio
        if (EsPiezaDeslizante(pieza.tipoPieza))
        {
            if (HayPiezasEntre(origen, destino))
            {
                Debug.LogWarning("[BaseConocimiento] Hay piezas bloqueando el camino.");
                return false;
            }
        }

        // ----------------------------------------------------------------
        //      Si llegamos hasta aquí: el movimiento es LEGAL
        // ----------------------------------------------------------------
        Debug.Log($"[BaseConocimiento] ✅ Movimiento LEGAL de {pieza.tipoPieza} {pieza.colorPieza} de {origen} a {destino}");

        // 6) Si hay pieza enemiga en destino → capturar
        GameObject piezaEnDestino = ObtenerPiezaEnPosicion(destino);
        if (piezaEnDestino != null)
        {
            PiezaAjedrez capturada = piezaEnDestino.GetComponent<PiezaAjedrez>();
            if (capturada != null)
            {
                Debug.Log($"[BaseConocimiento] ⚔ Captura: {capturada.tipoPieza} {capturada.colorPieza} en {destino}");

                bool reyCapturado = (capturada.tipoPieza == PiezaAjedrez.TipoPieza.Rey);
                PiezaAjedrez.ColorPieza colorRey = capturada.colorPieza;

                Destroy(piezaEnDestino);

                if (reyCapturado)
                {
                    Debug.Log("[BaseConocimiento] 👑 ¡Rey capturado! Fin de partida.");
                    CargarEscenaVictoria(colorRey == PiezaAjedrez.ColorPieza.Blanco
                        ? PiezaAjedrez.ColorPieza.Negro
                        : PiezaAjedrez.ColorPieza.Blanco);
                }
            }
        }

        // 7) Actualizar posición lógica de la pieza movida
        pieza.ActualizarPosicion(destino);

        // 8) Actualizar registro interno
        ActualizarRegistroPiezas();

        return true;
    }

    // ==========================================================
    //             REGLAS POR TIPO DE PIEZA
    // ==========================================================
    private bool EsMovimientoValidoPorTipo(PiezaAjedrez pieza, Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;

        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        Debug.Log($"[MOVIMIENTO] {pieza.tipoPieza} {pieza.colorPieza} de {origen} a {destino} dx={dx}, dy={dy}, |dx|={absDx}, |dy|={absDy}");

        switch (pieza.tipoPieza)
        {
            case PiezaAjedrez.TipoPieza.Peon:
                return EsMovimientoValidoPeon(pieza.colorPieza, origen, destino);

            case PiezaAjedrez.TipoPieza.Torre:
                // horizontal o vertical
                return (dx == 0 || dy == 0);

            case PiezaAjedrez.TipoPieza.Alfil:
                // diagonal
                return absDx == absDy;

            case PiezaAjedrez.TipoPieza.Reina:
                // combinación torre + alfil
                return (dx == 0 || dy == 0 || absDx == absDy);

            case PiezaAjedrez.TipoPieza.Caballo:
                // movimiento en L
                return (absDx == 2 && absDy == 1) || (absDx == 1 && absDy == 2);

            case PiezaAjedrez.TipoPieza.Rey:
                // 1 casilla en cualquier dirección (sin enroque por simplicidad)
                return absDx <= 1 && absDy <= 1;

            default:
                return false;
        }
    }

    // 🔹 Peones con reglas estrictas + logs
    private bool EsMovimientoValidoPeon(PiezaAjedrez.ColorPieza color, Vector2Int origen, Vector2Int destino)
    {
        int dir = (color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : -1;

        int dx = destino.x - origen.x;   // filas (vertical)
        int dy = destino.y - origen.y;   // columnas (horizontal)

        bool hayPiezaDestino = HayPiezaEnPosicion(destino);
        PiezaAjedrez.ColorPieza colorDestino = PiezaAjedrez.ColorPieza.Blanco;
        if (hayPiezaDestino)
            colorDestino = ObtenerColorPiezaEnPosicion(destino);

        Debug.Log($"[PEÓN {color}] origen={origen} destino={destino} dx={dx} dy={dy} hayPiezaDestino={hayPiezaDestino} colorDestino={colorDestino}");

        // Movimiento recto hacia adelante (1 casilla, sin capturar)
        if (dy == 0 && dx == dir)
        {
            if (!hayPiezaDestino)
            {
                Debug.Log("[PEÓN] Avance simple válido");
                return true;
            }
            else
            {
                Debug.Log("[PEÓN] Bloqueado: hay pieza justo delante");
                return false;
            }
        }

        // Doble paso inicial (2 casillas hacia adelante)
        int filaInicial = (color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : 6;

        if (dy == 0 && dx == 2 * dir && origen.x == filaInicial)
        {
            Vector2Int intermedia = new Vector2Int(origen.x + dir, origen.y);

            if (!HayPiezaEnPosicion(intermedia) && !hayPiezaDestino)
            {
                Debug.Log("[PEÓN] Doble paso inicial válido");
                return true;
            }
            else
            {
                Debug.Log("[PEÓN] Doble paso bloqueado (pieza en medio o en destino)");
                return false;
            }
        }

        // Captura en diagonal (1 casilla en diagonal hacia delante, con pieza enemiga)
        if (Mathf.Abs(dy) == 1 && dx == dir)
        {
            if (hayPiezaDestino && colorDestino != color)
            {
                Debug.Log("[PEÓN] Captura diagonal válida");
                return true;
            }
            else
            {
                Debug.Log("[PEÓN] Intento de captura diagonal inválido (no hay enemigo o es del mismo color)");
                return false;
            }
        }

        Debug.Log("[PEÓN] Movimiento inválido (no encaja en ninguna regla)");
        return false;
    }

    private bool EsPiezaDeslizante(PiezaAjedrez.TipoPieza tipo)
    {
        return tipo == PiezaAjedrez.TipoPieza.Torre ||
               tipo == PiezaAjedrez.TipoPieza.Alfil ||
               tipo == PiezaAjedrez.TipoPieza.Reina;
    }

    private bool HayPiezasEntre(Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;

        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        Vector2Int pos = new Vector2Int(origen.x + stepX, origen.y + stepY);

        while (pos != destino)
        {
            if (HayPiezaEnPosicion(pos))
                return true;

            pos = new Vector2Int(pos.x + stepX, pos.y + stepY);
        }

        return false;
    }

    private bool DentroTablero(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 &&
               pos.y >= 0 && pos.y < 8;
    }

    // ==========================================================
    //          REGISTRO DE PIEZAS Y UTILIDADES
    // ==========================================================
    private void ActualizarRegistroPiezas()
    {
        piezasPorPosicion.Clear();

        PiezaAjedrez[] todasLasPiezas =
            Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (PiezaAjedrez pieza in todasLasPiezas)
        {
            if (!piezasPorPosicion.ContainsKey(pieza.posicionActual))
            {
                piezasPorPosicion.Add(pieza.posicionActual, pieza.gameObject);
            }
        }
    }

    public void Refrescar()
    {
        ActualizarRegistroPiezas();
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
            if (scriptPieza != null)
                return scriptPieza.colorPieza;
        }

        return PiezaAjedrez.ColorPieza.Blanco;
    }

    // ==========================================================
    //                   CAMBIO DE ESCENAS
    // ==========================================================
    private void CargarEscenaVictoria(PiezaAjedrez.ColorPieza ganador)
    {
        string escena = (ganador == PiezaAjedrez.ColorPieza.Blanco)
            ? escenaVictoriaBlancas   // "Victoria"
            : escenaVictoriaNegras;   // "GameOver"

        if (!string.IsNullOrEmpty(escena))
        {
            Debug.Log("[BaseConocimiento] Cargando escena de victoria: " + escena);
            SceneManager.LoadScene(escena);
        }
        else
        {
            Debug.LogWarning("[BaseConocimiento] No hay escena configurada para la victoria de " + ganador);
        }
    }
}
