using System.Collections.Generic;
using UnityEngine;

public class Inferencia : MonoBehaviour
{
    [Header("Referencias")]
    public MatrizTablero tablero;
    public BaseConocimiento baseConocimiento;

    // ==========================================================
    // MÉTODO PÚBLICO QUE LLAMA EL GESTOR DE TURNOS
    // ==========================================================
    public void RealizarJugada()
    {
        if (tablero == null || baseConocimiento == null)
        {
            Debug.LogError("[IA] Falta asignar Tablero o BaseConocimiento.");
            return;
        }

        Debug.Log("[IA] 🤖 Pensando jugada...");

        // Refresca el registro de piezas
        baseConocimiento.Refrescar();

        // Genera movimientos posibles de piezas negras
        List<Movimiento> movimientosCandidatos = GenerarMovimientosNegras();

        if (movimientosCandidatos.Count == 0)
        {
            Debug.LogWarning("[IA] No hay movimientos posibles para la IA.");
            return;
        }

        // Elegimos el primer movimiento legal (IA voraz simple)
        foreach (Movimiento mov in movimientosCandidatos)
        {
            if (baseConocimiento.VerificarMovimientoLegal(mov))
            {
                AplicarMovimiento(mov);
                return;
            }
        }

        Debug.LogWarning("[IA] Ningún movimiento candidato fue legal.");
    }

    // ==========================================================
    // Genera movimientos de todas las piezas negras
    // ==========================================================
    private List<Movimiento> GenerarMovimientosNegras()
    {
        List<Movimiento> listaMovimientos = new List<Movimiento>();
        PiezaAjedrez[] piezas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);

        foreach (var pieza in piezas)
        {
            if (pieza.colorPieza != PiezaAjedrez.ColorPieza.Negro)
                continue;

            List<Movimiento> capturas = new List<Movimiento>();
            List<Movimiento> normales = new List<Movimiento>();

            switch (pieza.tipoPieza)
            {
                case PiezaAjedrez.TipoPieza.Peon:
                    GenerarMovimientosPeon(pieza, capturas, normales);
                    break;
                case PiezaAjedrez.TipoPieza.Caballo:
                    GenerarMovimientosCaballo(pieza, capturas, normales);
                    break;
                case PiezaAjedrez.TipoPieza.Torre:
                    GenerarMovimientosDeslizantes(pieza, new Vector2Int[] {
                        new Vector2Int(1,0), new Vector2Int(-1,0),
                        new Vector2Int(0,1), new Vector2Int(0,-1)
                    }, capturas, normales);
                    break;
                case PiezaAjedrez.TipoPieza.Alfil:
                    GenerarMovimientosDeslizantes(pieza, new Vector2Int[] {
                        new Vector2Int(1,1), new Vector2Int(1,-1),
                        new Vector2Int(-1,1), new Vector2Int(-1,-1)
                    }, capturas, normales);
                    break;
                case PiezaAjedrez.TipoPieza.Reina:
                    GenerarMovimientosDeslizantes(pieza, new Vector2Int[] {
                        new Vector2Int(1,0), new Vector2Int(-1,0),
                        new Vector2Int(0,1), new Vector2Int(0,-1),
                        new Vector2Int(1,1), new Vector2Int(1,-1),
                        new Vector2Int(-1,1), new Vector2Int(-1,-1)
                    }, capturas, normales);
                    break;
                case PiezaAjedrez.TipoPieza.Rey:
                    GenerarMovimientosRey(pieza, capturas, normales);
                    break;
            }

            // Prioridad: capturas > movimientos normales
            if (capturas.Count > 0)
                listaMovimientos.AddRange(capturas);
            else
                listaMovimientos.AddRange(normales);
        }

        return listaMovimientos;
    }

    // ==========================================================
    // Mueve la ficha en el tablero real
    // ==========================================================
    private void AplicarMovimiento(Movimiento mov)
    {
        mov.ficha.transform.position = tablero.ObtenerPosicionCasilla(mov.destino.x, mov.destino.y);
        mov.ficha.ActualizarPosicion(mov.destino);

        baseConocimiento.Refrescar();
        Debug.Log($"[IA] Movimiento aplicado: {mov.ficha.tipoPieza} de {mov.inicio} → {mov.destino}");
    }

    // ==========================================================
    // Generadores de movimientos por pieza
    // ==========================================================
    private void GenerarMovimientosPeon(PiezaAjedrez pieza, List<Movimiento> capturas, List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;
        int dir = -1; // negras van hacia arriba

        Vector2Int adelante = new Vector2Int(pos.x + dir, pos.y);
        if (DentroTablero(adelante) && !baseConocimiento.HayPiezaEnPosicion(adelante))
            normales.Add(new Movimiento(pos, adelante, pieza));

        Vector2Int diagL = new Vector2Int(pos.x + dir, pos.y - 1);
        Vector2Int diagR = new Vector2Int(pos.x + dir, pos.y + 1);

        if (DentroTablero(diagL) && baseConocimiento.HayPiezaEnPosicion(diagL) &&
            baseConocimiento.ObtenerColorPiezaEnPosicion(diagL) != pieza.colorPieza)
            capturas.Add(new Movimiento(pos, diagL, pieza));

        if (DentroTablero(diagR) && baseConocimiento.HayPiezaEnPosicion(diagR) &&
            baseConocimiento.ObtenerColorPiezaEnPosicion(diagR) != pieza.colorPieza)
            capturas.Add(new Movimiento(pos, diagR, pieza));
    }

    private void GenerarMovimientosCaballo(PiezaAjedrez pieza, List<Movimiento> capturas, List<Movimiento> normales)
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
                if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) != pieza.colorPieza)
                    capturas.Add(new Movimiento(pos, d, pieza));
            }
            else
                normales.Add(new Movimiento(pos, d, pieza));
        }
    }

    private void GenerarMovimientosDeslizantes(PiezaAjedrez pieza, Vector2Int[] dirs, List<Movimiento> capturas, List<Movimiento> normales)
    {
        Vector2Int pos = pieza.posicionActual;

        foreach (var dir in dirs)
        {
            for (int paso = 1; paso < 8; paso++)
            {
                Vector2Int d = new Vector2Int(pos.x + dir.x * paso, pos.y + dir.y * paso);
                if (!DentroTablero(d)) break;

                if (baseConocimiento.HayPiezaEnPosicion(d))
                {
                    if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) != pieza.colorPieza)
                        capturas.Add(new Movimiento(pos, d, pieza));
                    break;
                }
                else
                    normales.Add(new Movimiento(pos, d, pieza));
            }
        }
    }

    private void GenerarMovimientosRey(PiezaAjedrez pieza, List<Movimiento> capturas, List<Movimiento> normales)
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
                if (baseConocimiento.ObtenerColorPiezaEnPosicion(d) != pieza.colorPieza)
                    capturas.Add(new Movimiento(pos, d, pieza));
            }
            else
                normales.Add(new Movimiento(pos, d, pieza));
        }
    }

    private bool DentroTablero(Vector2Int p)
    {
        return p.x >= 0 && p.x < 8 && p.y >= 0 && p.y < 8;
    }
}
