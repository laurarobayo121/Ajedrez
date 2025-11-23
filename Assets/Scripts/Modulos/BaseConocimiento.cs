using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BaseConocimiento : MonoBehaviour
{
    [Header("Escenas de resultado")]
    // Blancas ganan -> Victoria, Negras ganan -> GameOver
    [SerializeField] private string escenaVictoriaBlancas = "Victoria";
    [SerializeField] private string escenaVictoriaNegras = "GameOver";

    private MatrizTablero tablero;

    // Mapa de posiciones lógicas -> GameObject de la pieza (estado actual)
    private Dictionary<Vector2Int, GameObject> piezasPorPosicion =
        new Dictionary<Vector2Int, GameObject>();

    // -------------------------------------------
    // ESTRUCTURA INTERNA PARA SIMULACIÓN
    // -------------------------------------------
    private struct PiezaSim
    {
        public PiezaAjedrez.TipoPieza tipo;
        public PiezaAjedrez.ColorPieza color;

        public PiezaSim(PiezaAjedrez.TipoPieza t, PiezaAjedrez.ColorPieza c)
        {
            tipo = t;
            color = c;
        }
    }

    void Start()
    {
        tablero = FindObjectOfType<MatrizTablero>();
        ActualizarRegistroPiezas();
    }

    // ===========================================
    // MÉTODO PRINCIPAL: VERIFICAR MOVIMIENTO
    // ===========================================
    /// <summary>
    /// Verifica si un movimiento es legal según TODAS las reglas:
    ///  - movimiento del tipo de pieza (geometría)
    ///  - bloqueo por otras piezas
    ///  - no se come piezas propias
    ///  - no deja a tu propio rey en jaque
    /// Si es legal, actualiza posiciones, destruye la pieza comida (si hay)
    /// y evalúa jaque mate/captura de rey para cambiar de escena.
    /// </summary>
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

        Vector2Int origen = pieza.posicionActual;
        Vector2Int destino = movimiento.destino;

        // Asegurar que el diccionario está actualizado
        ActualizarRegistroPiezas();

        // 1) No quedarse en la misma casilla
        if (origen == destino)
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento a la misma casilla.");
            return false;
        }

        // 2) Validar que el destino está dentro del tablero
        if (!DentroTablero(destino))
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento fuera del tablero.");
            return false;
        }

        // 3) No capturar pieza propia
        if (HayPiezaEnPosicion(destino))
        {
            var colorDestino = ObtenerColorPiezaEnPosicion(destino);
            if (colorDestino == pieza.colorPieza)
            {
                Debug.LogWarning("[BaseConocimiento] No puedes capturar tu propia pieza.");
                return false;
            }
        }

        // 4) Reglas específicas de movimiento por tipo de pieza
        if (!EsMovimientoValidoPorTipo(pieza, origen, destino))
        {
            Debug.LogWarning("[BaseConocimiento] Movimiento no válido para esta pieza.");
            return false;
        }

        // 5) Para piezas deslizantes, comprobar que NO hay piezas en medio
        if (EsPiezaDeslizante(pieza.tipoPieza))
        {
            if (HayPiezasEntre(origen, destino))
            {
                Debug.LogWarning("[BaseConocimiento] Hay piezas bloqueando el camino.");
                return false;
            }
        }

        // 6) Comprobar que NO dejas a tu propio rey en jaque (simulación)
        if (MovimientoDejaEnJaquePropioRey(pieza, origen, destino))
        {
            Debug.LogWarning("[BaseConocimiento] No puedes dejar a tu rey en jaque.");
            return false;
        }

        // --------------------------------------------------------------------
        // Hasta aquí el movimiento es LEGAL según las reglas del ajedrez
        // --------------------------------------------------------------------

        Debug.Log($"[BaseConocimiento] Movimiento LEGAL de {pieza.tipoPieza} {pieza.colorPieza} de {origen} a {destino}");

        // 7) Si hay pieza enemiga en el destino, la "comemos"
        GameObject piezaEnDestino = ObtenerPiezaEnPosicion(destino);
        bool capturaRey = false;

        if (piezaEnDestino != null)
        {
            PiezaAjedrez piezaCapturada = piezaEnDestino.GetComponent<PiezaAjedrez>();
            if (piezaCapturada != null &&
                piezaCapturada.tipoPieza == PiezaAjedrez.TipoPieza.Rey)
            {
                capturaRey = true;
            }

            Destroy(piezaEnDestino);
        }

        // 8) Actualizar posición lógica de la pieza movida
        pieza.ActualizarPosicion(destino);

        // 9) Actualizar registro interno
        ActualizarRegistroPiezas();

        // 10) Evaluar condición de victoria:
        PiezaAjedrez.ColorPieza colorQueJuega = pieza.colorPieza;
        PiezaAjedrez.ColorPieza colorOponente =
            (colorQueJuega == PiezaAjedrez.ColorPieza.Blanco)
                ? PiezaAjedrez.ColorPieza.Negro
                : PiezaAjedrez.ColorPieza.Blanco;

        if (capturaRey)
        {
            Debug.Log("[BaseConocimiento] ¡Rey capturado! Fin de la partida.");
            CargarEscenaVictoria(colorQueJuega);
        }
        else
        {
            if (EstaEnJaqueMate(colorOponente))
            {
                Debug.Log("[BaseConocimiento] ¡Jaque mate! Ganan " + colorQueJuega);
                CargarEscenaVictoria(colorQueJuega);
            }
        }

        return true;
    }

    // ===========================================
    // REGLAS POR TIPO DE PIEZA
    // ===========================================
    private bool EsMovimientoValidoPorTipo(PiezaAjedrez pieza, Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;

        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        switch (pieza.tipoPieza)
        {
            case PiezaAjedrez.TipoPieza.Peon:
                return EsMovimientoValidoPeon(pieza.colorPieza, origen, destino);

            case PiezaAjedrez.TipoPieza.Torre:
                // Movimiento horizontal o vertical
                return (dx == 0 || dy == 0);

            case PiezaAjedrez.TipoPieza.Alfil:
                // Movimiento diagonal
                return absDx == absDy;

            case PiezaAjedrez.TipoPieza.Reina:
                // Combinación de torre y alfil
                return (dx == 0 || dy == 0 || absDx == absDy);

            case PiezaAjedrez.TipoPieza.Caballo:
                // L: 2x1 o 1x2
                return (absDx == 2 && absDy == 1) || (absDx == 1 && absDy == 2);

            case PiezaAjedrez.TipoPieza.Rey:
                // Un paso en cualquier dirección (sin enroque por simplicidad)
                return absDx <= 1 && absDy <= 1;

            default:
                return false;
        }
    }

    private bool EsMovimientoValidoPeon(PiezaAjedrez.ColorPieza color, Vector2Int origen, Vector2Int destino)
    {
        int direccion = (color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : -1;
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;

        bool hayPiezaDestino = HayPiezaEnPosicion(destino);
        PiezaAjedrez.ColorPieza colorDestino = PiezaAjedrez.ColorPieza.Blanco;
        if (hayPiezaDestino)
            colorDestino = ObtenerColorPiezaEnPosicion(destino);

        // Movimiento hacia adelante (1 casilla)
        if (dy == 0 && dx == direccion && !hayPiezaDestino)
            return true;

        // Doble paso inicial
        if (dy == 0 && dx == 2 * direccion && !hayPiezaDestino)
        {
            int filaInicial = (color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : 6;
            if (origen.x == filaInicial)
            {
                Vector2Int intermedia = new Vector2Int(origen.x + direccion, origen.y);
                if (!HayPiezaEnPosicion(intermedia))
                    return true;
            }
        }

        // Captura en diagonal
        if (Mathf.Abs(dy) == 1 && dx == direccion && hayPiezaDestino && colorDestino != color)
            return true;

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
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    // ===========================================
    // SIMULACIÓN PARA JAQUE Y JAQUE MATE
    // ===========================================
    private Dictionary<Vector2Int, PiezaSim> ConstruirMapaSimulado()
    {
        Dictionary<Vector2Int, PiezaSim> mapa = new Dictionary<Vector2Int, PiezaSim>();

        PiezaAjedrez[] piezas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);
        foreach (var p in piezas)
        {
            mapa[p.posicionActual] = new PiezaSim(p.tipoPieza, p.colorPieza);
        }

        return mapa;
    }

    private bool MovimientoDejaEnJaquePropioRey(PiezaAjedrez pieza, Vector2Int origen, Vector2Int destino)
    {
        var mapa = ConstruirMapaSimulado();

        // Aplicar movimiento simulado
        if (mapa.ContainsKey(origen))
            mapa.Remove(origen);

        if (mapa.ContainsKey(destino))
            mapa.Remove(destino);

        mapa[destino] = new PiezaSim(pieza.tipoPieza, pieza.colorPieza);

        return EstaEnJaque(pieza.colorPieza, mapa);
    }

    private bool EstaEnJaque(PiezaAjedrez.ColorPieza color, Dictionary<Vector2Int, PiezaSim> mapa)
    {
        // 1) Encontrar al rey
        bool reyEncontrado = false;
        Vector2Int posRey = Vector2Int.zero;

        foreach (var kv in mapa)
        {
            if (kv.Value.color == color &&
                kv.Value.tipo == PiezaAjedrez.TipoPieza.Rey)
            {
                reyEncontrado = true;
                posRey = kv.Key;
                break;
            }
        }

        if (!reyEncontrado)
            return true; // sin rey = perdido

        // 2) Ver si alguna pieza enemiga puede capturar esa casilla
        foreach (var kv in mapa)
        {
            PiezaSim pieza = kv.Value;

            if (pieza.color == color)
                continue;

            Vector2Int origen = kv.Key;
            List<Vector2Int> ataques = GenerarMovimientosSimulados(origen, pieza, mapa, soloAtaquesPeon: true);

            foreach (var d in ataques)
            {
                if (d == posRey)
                    return true;
            }
        }

        return false;
    }

    private bool EstaEnJaqueMate(PiezaAjedrez.ColorPieza color)
    {
        var mapa = ConstruirMapaSimulado();

        // Si no está en jaque, no puede ser jaque mate
        if (!EstaEnJaque(color, mapa))
            return false;

        // Probar todos los movimientos posibles de ese color
        foreach (var kv in mapa)
        {
            PiezaSim pieza = kv.Value;
            if (pieza.color != color)
                continue;

            Vector2Int origen = kv.Key;
            List<Vector2Int> movs = GenerarMovimientosSimulados(origen, pieza, mapa, soloAtaquesPeon: false);

            foreach (var destino in movs)
            {
                var mapaCopia = new Dictionary<Vector2Int, PiezaSim>(mapa);

                if (mapaCopia.ContainsKey(origen))
                    mapaCopia.Remove(origen);

                if (mapaCopia.ContainsKey(destino))
                    mapaCopia.Remove(destino);

                mapaCopia[destino] = pieza;

                if (!EstaEnJaque(color, mapaCopia))
                {
                    return false; // hay al menos una jugada que salva
                }
            }
        }

        // No hay jugada que salve al rey
        return true;
    }

    private List<Vector2Int> GenerarMovimientosSimulados(
        Vector2Int origen,
        PiezaSim pieza,
        Dictionary<Vector2Int, PiezaSim> mapa,
        bool soloAtaquesPeon)
    {
        List<Vector2Int> resultado = new List<Vector2Int>();

        int x = origen.x;
        int y = origen.y;

        switch (pieza.tipo)
        {
            case PiezaAjedrez.TipoPieza.Peon:
                {
                    int dir = (pieza.color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : -1;

                    // Ataques diagonales (para jaque)
                    Vector2Int diagIzq = new Vector2Int(x + dir, y - 1);
                    Vector2Int diagDer = new Vector2Int(x + dir, y + 1);

                    if (DentroTablero(diagIzq))
                    {
                        if (soloAtaquesPeon)
                        {
                            resultado.Add(diagIzq);
                        }
                        else
                        {
                            if (!mapa.ContainsKey(diagIzq) || mapa[diagIzq].color != pieza.color)
                                resultado.Add(diagIzq);
                        }
                    }

                    if (DentroTablero(diagDer))
                    {
                        if (soloAtaquesPeon)
                        {
                            resultado.Add(diagDer);
                        }
                        else
                        {
                            if (!mapa.ContainsKey(diagDer) || mapa[diagDer].color != pieza.color)
                                resultado.Add(diagDer);
                        }
                    }

                    if (!soloAtaquesPeon)
                    {
                        // Movimiento hacia adelante
                        Vector2Int adelante = new Vector2Int(x + dir, y);
                        if (DentroTablero(adelante) && !mapa.ContainsKey(adelante))
                        {
                            resultado.Add(adelante);
                        }

                        // Doble paso inicial
                        int filaInicial = (pieza.color == PiezaAjedrez.ColorPieza.Blanco) ? 1 : 6;
                        if (x == filaInicial)
                        {
                            Vector2Int intermedia = new Vector2Int(x + dir, y);
                            Vector2Int doble = new Vector2Int(x + 2 * dir, y);
                            if (DentroTablero(doble) &&
                                !mapa.ContainsKey(intermedia) &&
                                !mapa.ContainsKey(doble))
                            {
                                resultado.Add(doble);
                            }
                        }
                    }
                }
                break;

            case PiezaAjedrez.TipoPieza.Caballo:
                {
                    Vector2Int[] offsets = new Vector2Int[]
                    {
                        new Vector2Int( 2,  1),
                        new Vector2Int( 2, -1),
                        new Vector2Int(-2,  1),
                        new Vector2Int(-2, -1),
                        new Vector2Int( 1,  2),
                        new Vector2Int( 1, -2),
                        new Vector2Int(-1,  2),
                        new Vector2Int(-1, -2),
                    };

                    foreach (var off in offsets)
                    {
                        Vector2Int d = origen + off;
                        if (!DentroTablero(d))
                            continue;

                        if (!mapa.ContainsKey(d) || mapa[d].color != pieza.color)
                        {
                            resultado.Add(d);
                        }
                    }
                }
                break;

            case PiezaAjedrez.TipoPieza.Rey:
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            Vector2Int d = new Vector2Int(x + dx, y + dy);
                            if (!DentroTablero(d)) continue;

                            if (!mapa.ContainsKey(d) || mapa[d].color != pieza.color)
                            {
                                resultado.Add(d);
                            }
                        }
                    }
                }
                break;

            case PiezaAjedrez.TipoPieza.Torre:
                {
                    Vector2Int[] dirs = new Vector2Int[]
                    {
                        new Vector2Int( 1,  0),
                        new Vector2Int(-1,  0),
                        new Vector2Int( 0,  1),
                        new Vector2Int( 0, -1),
                    };

                    foreach (var dir in dirs)
                    {
                        Vector2Int d = origen + dir;
                        while (DentroTablero(d))
                        {
                            if (!mapa.ContainsKey(d))
                            {
                                resultado.Add(d);
                            }
                            else
                            {
                                if (mapa[d].color != pieza.color)
                                    resultado.Add(d);
                                break;
                            }
                            d += dir;
                        }
                    }
                }
                break;

            case PiezaAjedrez.TipoPieza.Alfil:
                {
                    Vector2Int[] dirs = new Vector2Int[]
                    {
                        new Vector2Int( 1,  1),
                        new Vector2Int( 1, -1),
                        new Vector2Int(-1,  1),
                        new Vector2Int(-1, -1),
                    };

                    foreach (var dir in dirs)
                    {
                        Vector2Int d = origen + dir;
                        while (DentroTablero(d))
                        {
                            if (!mapa.ContainsKey(d))
                            {
                                resultado.Add(d);
                            }
                            else
                            {
                                if (mapa[d].color != pieza.color)
                                    resultado.Add(d);
                                break;
                            }
                            d += dir;
                        }
                    }
                }
                break;

            case PiezaAjedrez.TipoPieza.Reina:
                {
                    Vector2Int[] dirs = new Vector2Int[]
                    {
                        new Vector2Int( 1,  0),
                        new Vector2Int(-1,  0),
                        new Vector2Int( 0,  1),
                        new Vector2Int( 0, -1),
                        new Vector2Int( 1,  1),
                        new Vector2Int( 1, -1),
                        new Vector2Int(-1,  1),
                        new Vector2Int(-1, -1),
                    };

                    foreach (var dir in dirs)
                    {
                        Vector2Int d = origen + dir;
                        while (DentroTablero(d))
                        {
                            if (!mapa.ContainsKey(d))
                            {
                                resultado.Add(d);
                            }
                            else
                            {
                                if (mapa[d].color != pieza.color)
                                    resultado.Add(d);
                                break;
                            }
                            d += dir;
                        }
                    }
                }
                break;
        }

        return resultado;
    }

    // ===========================================
    // REGISTRO DE PIEZAS Y UTILIDADES PÚBLICAS
    // ===========================================
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

    // ===========================================
    // CAMBIO DE ESCENAS
    // ===========================================
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
