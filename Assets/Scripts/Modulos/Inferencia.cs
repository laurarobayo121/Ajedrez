using UnityEngine;
using System.Collections.Generic;

public class Inferencia : MonoBehaviour
{
    [Header("Referencias")]
    public MatrizTablero tablero;
    public BaseConocimiento baseConocimiento;

    [Header("Parámetros de búsqueda")]
    [Tooltip("Profundidad de búsqueda conceptual. 1 o 2 es suficiente para el proyecto.")]
    public int profundidadMax = 2;

    // Valores materiales simples para las piezas (Vp)
    private Dictionary<PiezaAjedrez.TipoPieza, int> valorPieza =
        new Dictionary<PiezaAjedrez.TipoPieza, int>()
    {
        { PiezaAjedrez.TipoPieza.Peon,   1 },
        { PiezaAjedrez.TipoPieza.Caballo,3 },
        { PiezaAjedrez.TipoPieza.Alfil,  3 },
        { PiezaAjedrez.TipoPieza.Torre,  5 },
        { PiezaAjedrez.TipoPieza.Reina,  9 },
        { PiezaAjedrez.TipoPieza.Rey,   100 }
    };

    // ==========================================================
    //              MÉTODO PRINCIPAL LLAMADO POR EL GESTOR
    // ==========================================================
    public void RealizarJugada()
    {
        Debug.Log("🤖 [IA] Pensando jugada (heurística + reglas completas)…");

        if (tablero == null || baseConocimiento == null)
        {
            Debug.LogError("❌ [IA] Falta asignar Tablero o BaseConocimiento en el inspector.");
            return;
        }

        // Refresca el estado interno de la base de conocimiento
        baseConocimiento.Refrescar();

        // 1. Generar candidatos de movimientos para las NEGRAS,
        //    separando movimientos del rey y del resto de piezas.
        List<Movimiento> candidatos = GenerarMovimientosIAConPrioridad();

        if (candidatos.Count == 0)
        {
            Debug.LogWarning("⚠ [IA] No se encontraron movimientos disponibles (posible jaque mate o tablas).");
            return;
        }

        // 2. Evaluar cada movimiento con una heurística h(s)
        Movimiento mejor = ElegirMejorMovimientoPorHeuristica(candidatos);

        if (mejor == null)
        {
            Debug.LogWarning("[IA] No se pudo seleccionar un movimiento (lista vacía o error).");
            return;
        }

        // 3. Intentar aplicar el movimiento en el tablero real.
        AplicarMovimientoConVerificacion(mejor, candidatos);
    }

    // ==========================================================
    //    1. GENERACIÓN DE MOVIMIENTOS CON PRIORIDAD (NO REY)
    // ==========================================================

    private bool DentroTablero(Vector2Int p)
    {
        return p.x >= 0 && p.x < tablero.filas &&
               p.y >= 0 && p.y < tablero.columnas;
    }

    private List<Movimiento> GenerarMovimientosIAConPrioridad()
    {
        List<Movimiento> capturasNoRey      = new List<Movimiento>();
        List<Movimiento> movsNoCapturaNoRey = new List<Movimiento>();

        List<Movimiento> capturasRey        = new List<Movimiento>();
        List<Movimiento> movsNoCapturaRey   = new List<Movimiento>();

        PiezaAjedrez[] piezas =
            Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (var pieza in piezas)
        {
            if (pieza.colorPieza != PiezaAjedrez.ColorPieza.Negro)
                continue; // IA controla negras

            bool esRey = pieza.tipoPieza == PiezaAjedrez.TipoPieza.Rey;

            if (esRey)
            {
                GenerarMovimientosRey(pieza, capturasRey, movsNoCapturaRey);
            }
            else
            {
                switch (pieza.tipoPieza)
                {
                    case PiezaAjedrez.TipoPieza.Peon:
                        GenerarMovimientosPeon(pieza, capturasNoRey, movsNoCapturaNoRey);
                        break;
                    case PiezaAjedrez.TipoPieza.Caballo:
                        GenerarMovimientosCaballo(pieza, capturasNoRey, movsNoCapturaNoRey);
                        break;
                    case PiezaAjedrez.TipoPieza.Alfil:
                        GenerarMovimientosDeslizantes(
                            pieza,
                            new Vector2Int[] {
                                new Vector2Int(1,1), new Vector2Int(1,-1),
                                new Vector2Int(-1,1), new Vector2Int(-1,-1)
                            },
                            capturasNoRey, movsNoCapturaNoRey);
                        break;
                    case PiezaAjedrez.TipoPieza.Torre:
                        GenerarMovimientosDeslizantes(
                            pieza,
                            new Vector2Int[] {
                                new Vector2Int(1,0), new Vector2Int(-1,0),
                                new Vector2Int(0,1), new Vector2Int(0,-1)
                            },
                            capturasNoRey, movsNoCapturaNoRey);
                        break;
                    case PiezaAjedrez.TipoPieza.Reina:
                        GenerarMovimientosDeslizantes(
                            pieza,
                            new Vector2Int[] {
                                new Vector2Int(1,0), new Vector2Int(-1,0),
                                new Vector2Int(0,1), new Vector2Int(0,-1),
                                new Vector2Int(1,1), new Vector2Int(1,-1),
                                new Vector2Int(-1,1), new Vector2Int(-1,-1)
                            },
                            capturasNoRey, movsNoCapturaNoRey);
                        break;
                }
            }
        }

        List<Movimiento> resultado = new List<Movimiento>();

        // PRIORIDAD:
        // 1) capturas de piezas que no son rey
        // 2) movimientos normales de piezas que no son rey
        // 3) capturas con el rey
        // 4) movimientos normales del rey
        if (capturasNoRey.Count > 0)
        {
            Debug.Log("[IA] 📌 Hay capturas de piezas (sin usar rey).");
            resultado.AddRange(capturasNoRey);
        }
        else if (movsNoCapturaNoRey.Count > 0)
        {
            Debug.Log("[IA] 📌 No hay capturas, pero hay movimientos de otras piezas.");
            resultado.AddRange(movsNoCapturaNoRey);
        }
        else if (capturasRey.Count > 0)
        {
            Debug.Log("[IA] ⚠ Solo el rey puede capturar → se permite.");
            resultado.AddRange(capturasRey);
        }
        else
        {
            Debug.Log("[IA] ⚠ Solo el rey puede moverse → movimientos del rey.");
            resultado.AddRange(movsNoCapturaRey);
        }

        return resultado;
    }

    // ---------- Generadores por tipo de pieza ----------

    private void GenerarMovimientosPeon(
        PiezaAjedrez pieza,
        List<Movimiento> capturas,
        List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;
        int dir = (pieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco) ? 1 : -1;

        // una casilla hacia adelante
        Vector2Int one = new Vector2Int(pos.x + dir, pos.y);
        if (DentroTablero(one) && !baseConocimiento.HayPiezaEnPosicion(one))
        {
            normales.Add(new Movimiento(pos, one, pieza.gameObject));

            // doble paso desde fila inicial
            bool enInicioBlanco = pieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco && pos.x == 1;
            bool enInicioNegro  = pieza.colorPieza == PiezaAjedrez.ColorPieza.Negro  && pos.x == 6;

            if (enInicioBlanco || enInicioNegro)
            {
                Vector2Int two = new Vector2Int(pos.x + 2 * dir, pos.y);
                if (DentroTablero(two) &&
                    !baseConocimiento.HayPiezaEnPosicion(two))
                {
                    normales.Add(new Movimiento(pos, two, pieza.gameObject));
                }
            }
        }

        // capturas diagonales
        Vector2Int diagL = new Vector2Int(pos.x + dir, pos.y - 1);
        Vector2Int diagR = new Vector2Int(pos.x + dir, pos.y + 1);

        if (DentroTablero(diagL) &&
            baseConocimiento.HayPiezaEnPosicion(diagL) &&
            baseConocimiento.ObtenerColorPiezaEnPosicion(diagL) != pieza.colorPieza)
        {
            capturas.Add(new Movimiento(pos, diagL, pieza.gameObject));
        }

        if (DentroTablero(diagR) &&
            baseConocimiento.HayPiezaEnPosicion(diagR) &&
            baseConocimiento.ObtenerColorPiezaEnPosicion(diagR) != pieza.colorPieza)
        {
            capturas.Add(new Movimiento(pos, diagR, pieza.gameObject));
        }
    }

    private void GenerarMovimientosCaballo(
        PiezaAjedrez pieza,
        List<Movimiento> capturas,
        List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;
        int[] dx = { 2, 2, -2, -2, 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1, 2, -2, 2, -2 };

        for (int i = 0; i < 8; i++)
        {
            Vector2Int d = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (!DentroTablero(d)) continue;

            if (baseConocimiento.HayPiezaEnPosicion(d))
            {
                if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) ==
                    pieza.colorPieza)
                    continue; // propia

                capturas.Add(new Movimiento(pos, d, pieza.gameObject));
            }
            else
            {
                normales.Add(new Movimiento(pos, d, pieza.gameObject));
            }
        }
    }

    private void GenerarMovimientosDeslizantes(
        PiezaAjedrez pieza,
        Vector2Int[] direcciones,
        List<Movimiento> capturas,
        List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;

        foreach (var dir in direcciones)
        {
            for (int paso = 1; paso < 8; paso++)
            {
                Vector2Int d = new Vector2Int(pos.x + dir.x * paso, pos.y + dir.y * paso);
                if (!DentroTablero(d)) break;

                if (baseConocimiento.HayPiezaEnPosicion(d))
                {
                    if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) != pieza.colorPieza)
                    {
                        capturas.Add(new Movimiento(pos, d, pieza.gameObject));
                    }
                    break; // propio o enemigo, en ambos casos se corta la dirección
                }
                else
                {
                    normales.Add(new Movimiento(pos, d, pieza.gameObject));
                }
            }
        }
    }

    private void GenerarMovimientosRey(
        PiezaAjedrez pieza,
        List<Movimiento> capturas,
        List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;
        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(1,-1),
            new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        foreach (var dir in dirs)
        {
            Vector2Int d = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            if (!DentroTablero(d)) continue;

            if (baseConocimiento.HayPiezaEnPosicion(d))
            {
                if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) ==
                    pieza.colorPieza)
                    continue;

                capturas.Add(new Movimiento(pos, d, pieza.gameObject));
            }
            else
            {
                normales.Add(new Movimiento(pos, d, pieza.gameObject));
            }
        }
    }

    // ==========================================================
    //          2. EVALUACIÓN HEURÍSTICA TIPO MINIMAX
    // ==========================================================

    private class PiezaVirtual
    {
        public PiezaAjedrez.TipoPieza tipo;
        public PiezaAjedrez.ColorPieza color;
        public Vector2Int pos;

        public PiezaVirtual(PiezaAjedrez.TipoPieza t, PiezaAjedrez.ColorPieza c, Vector2Int p)
        {
            tipo = t; color = c; pos = p;
        }
    }

    private List<PiezaVirtual> ConstruirEstadoActualVirtual()
    {
        List<PiezaVirtual> estado = new List<PiezaVirtual>();
        PiezaAjedrez[] piezas =
            Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (var p in piezas)
        {
            estado.Add(new PiezaVirtual(p.tipoPieza, p.colorPieza, p.posicionActual));
        }

        return estado;
    }

    private void AplicarMovimientoVirtual(List<PiezaVirtual> estado, Movimiento m)
    {
        // eliminar pieza blanca capturada en el destino
        for (int i = estado.Count - 1; i >= 0; i--)
        {
            var pv = estado[i];
            if (pv.pos == m.destino &&
                pv.color == PiezaAjedrez.ColorPieza.Blanco)
            {
                estado.RemoveAt(i);
            }
        }

        // mover pieza negra desde inicio a destino
        foreach (var pv in estado)
        {
            if (pv.color == PiezaAjedrez.ColorPieza.Negro &&
                pv.pos == m.inicio)
            {
                pv.pos = m.destino;
                break;
            }
        }
    }

    private float EvaluarEstadoVirtual(List<PiezaVirtual> estado)
    {
        float alpha = 0.6f;
        float beta  = 0.25f;
        float gamma = 0.15f;

        float Vp = CalcularValorMaterial(estado);
        float Cc = CalcularControlCentro(estado);
        float Sk = CalcularSeguridadRey(estado);

        return alpha * Vp + beta * Cc + gamma * Sk;
    }

    private float CalcularValorMaterial(List<PiezaVirtual> estado)
    {
        float valorNegras = 0f;
        float valorBlancas = 0f;

        foreach (var p in estado)
        {
            if (!valorPieza.TryGetValue(p.tipo, out int v))
                continue;

            if (p.color == PiezaAjedrez.ColorPieza.Negro)
                valorNegras += v;
            else
                valorBlancas += v;
        }

        return valorNegras - valorBlancas;
    }

    private float CalcularControlCentro(List<PiezaVirtual> estado)
    {
        float score = 0f;
        Vector2 centro = new Vector2(3.5f, 3.5f);

        foreach (var p in estado)
        {
            float dist = Vector2.Distance(
                new Vector2(p.pos.x, p.pos.y),
                centro
            );

            float aporte = (8f - dist);

            if (p.color == PiezaAjedrez.ColorPieza.Negro)
                score += aporte;
            else
                score -= aporte;
        }

        return score;
    }

    private float CalcularSeguridadRey(List<PiezaVirtual> estado)
    {
        PiezaVirtual reyNegro = null;
        List<PiezaVirtual> piezasBlancas = new List<PiezaVirtual>();

        foreach (var p in estado)
        {
            if (p.tipo == PiezaAjedrez.TipoPieza.Rey &&
                p.color == PiezaAjedrez.ColorPieza.Negro)
            {
                reyNegro = p;
            }
            else if (p.color == PiezaAjedrez.ColorPieza.Blanco)
            {
                piezasBlancas.Add(p);
            }
        }

        if (reyNegro == null)
            return 0f;

        float penalizacion = 0f;

        foreach (var blanca in piezasBlancas)
        {
            float dist = Vector2.Distance(
                new Vector2(blanca.pos.x, blanca.pos.y),
                new Vector2(reyNegro.pos.x, reyNegro.pos.y)
            );

            if (dist <= 2f)
            {
                penalizacion += (3f - dist);
            }
        }

        return -penalizacion;
    }

    private Movimiento ElegirMejorMovimientoPorHeuristica(List<Movimiento> candidatos)
    {
        if (candidatos == null || candidatos.Count == 0)
            return null;

        float mejorValor = float.NegativeInfinity;
        Movimiento mejor = null;

        foreach (var mov in candidatos)
        {
            List<PiezaVirtual> estado = ConstruirEstadoActualVirtual();
            AplicarMovimientoVirtual(estado, mov);

            float valor = EvaluarEstadoVirtual(estado);

            // Penalizar un poquito si mueve el rey
            PiezaAjedrez pieza = mov.ficha.GetComponent<PiezaAjedrez>();
            if (pieza != null && pieza.tipoPieza == PiezaAjedrez.TipoPieza.Rey)
            {
                valor -= 0.5f;
            }

            if (valor > mejorValor)
            {
                mejorValor = valor;
                mejor = mov;
            }
        }

        Debug.Log($"[IA] 🔍 Mejor valor heurístico encontrado: {mejorValor}");
        return mejor;
    }

    // ==========================================================
    //           3. APLICAR MOVIMIENTO EN EL TABLERO REAL
    // ==========================================================
    private void AplicarMovimientoConVerificacion(Movimiento movimientoInicial,
                                                  List<Movimiento> todos)
    {
        List<Movimiento> intentos = new List<Movimiento>();
        intentos.Add(movimientoInicial);

        foreach (var m in todos)
        {
            if (m != movimientoInicial)
                intentos.Add(m);
        }

        foreach (var mov in intentos)
        {
            bool esLegal = baseConocimiento.VerificarMovimientoLegal(mov);

            if (esLegal)
            {
                mov.ficha.transform.position =
                    tablero.ObtenerPosicionCasilla(mov.destino.x, mov.destino.y);

                Debug.Log($"[IA] ✅ Movimiento aplicado: {mov.ficha.name} de {mov.inicio} → {mov.destino}");
                return;
            }
            else
            {
                Debug.LogWarning($"[IA] Movimiento descartado por BaseConocimiento: {mov.ficha.name} de {mov.inicio} → {mov.destino}");
            }
        }

        Debug.LogWarning("[IA] Ninguno de los movimientos candidatos fue aceptado como legal.");
    }
}
