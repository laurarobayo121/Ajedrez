using UnityEngine;
using System.Collections.Generic;

public class Inferencia : MonoBehaviour
{
    [Header("Referencias")]
    public MatrizTablero tablero;
    public BaseConocimiento baseConocimiento;

    [Header("Parámetros IA")]
    [Tooltip("Si es true, la IA intenta capturar siempre que pueda.")]
    public bool priorizarCapturas = true;

    // =====================================================================================
    //  MÉTODO PRINCIPAL LLAMADO POR GESTORTURNOS
    // =====================================================================================
    public void RealizarJugada()
    {
        if (tablero == null || baseConocimiento == null)
        {
            Debug.LogError("[IA] ERROR → Faltan referencias (tablero o baseConocimiento) en el Inspector.");
            return;
        }

        Debug.Log("[IA] Pensando jugada (versión simple, estable)…");

        // ----------------------------------------------------------------------
        // 1. Recolectar TODAS las piezas de la escena
        // ----------------------------------------------------------------------
        PiezaAjedrez[] todas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);
        List<Movimiento> candidatos = new List<Movimiento>();

        foreach (var p in todas)
        {
            if (p.colorPieza != PiezaAjedrez.ColorPieza.Negro)
                continue;

            // ------------------------------------------------------------------
            // 2. Obtener movimientos legales de esta pieza
            // ------------------------------------------------------------------
            List<Vector2Int> movs = p.ObtenerMovimientosLegales();

            foreach (var destino in movs)
            {
                // Seguridad: evitar coordenadas fuera de rango
                if (destino.x < 0 || destino.x >= 8 || destino.y < 0 || destino.y >= 8)
                    continue;

                candidatos.Add(new Movimiento(p.posicionActual, destino, p.gameObject));
            }
        }

        if (candidatos.Count == 0)
        {
            Debug.Log("[IA] No tiene movimientos disponibles (posible jaque mate o bloqueo).");
            return;
        }

        // ----------------------------------------------------------------------
        // 3. Elegir movimiento (priorizando capturas si es posible)
        // ----------------------------------------------------------------------
        Movimiento elegido = ElegirMovimiento(candidatos);

        // ----------------------------------------------------------------------
        // 4. Aplicar movimiento en escena (mover GameObject y actualizar coords)
        // ----------------------------------------------------------------------
        AplicarMovimientoEnEscena(elegido);

        Debug.Log($"[IA] Mueve {elegido.ficha.name} de {elegido.inicio} a {elegido.destino}");
    }

    // =====================================================================================
    //  ELEGIR MOVIMIENTO
    // =====================================================================================
    private Movimiento ElegirMovimiento(List<Movimiento> candidatos)
    {
        // Si queremos capturar, buscar primero jugadas donde la casilla destino tenga una pieza BLANCA
        if (priorizarCapturas)
        {
            foreach (var mov in candidatos)
            {
                GameObject piezaEnDestino = baseConocimiento.ObtenerPiezaEnPosicion(mov.destino);

                if (piezaEnDestino != null)
                {
                    PiezaAjedrez pieza = piezaEnDestino.GetComponent<PiezaAjedrez>();

                    if (pieza != null && pieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco)
                    {
                        Debug.Log("[IA] Jugada de captura detectada → la elige.");
                        return mov;
                    }
                }
            }
        }

        // Si no hay capturas, elegimos una al azar (IA sencilla)
        int index = Random.Range(0, candidatos.Count);
        return candidatos[index];
    }

    // =====================================================================================
    //  APLICAR MOVIMIENTO EN ESCENA
    // =====================================================================================
    private void AplicarMovimientoEnEscena(Movimiento mov)
    {
        if (mov.ficha == null)
        {
            Debug.LogError("[IA] ERROR → Movimiento sin ficha asociada.");
            return;
        }

        // ----------------------------------------------------------------------
        // 1. Comprobar si hay pieza en destino (captura)
        // ----------------------------------------------------------------------
        GameObject piezaEnDestino = baseConocimiento.ObtenerPiezaEnPosicion(mov.destino);

        if (piezaEnDestino != null && piezaEnDestino != mov.ficha)
        {
            Debug.Log($"[IA] Captura → {piezaEnDestino.name}");
            Destroy(piezaEnDestino);
        }

        // ----------------------------------------------------------------------
        // 2. Mover GameObject a la casilla destino VISUALMENTE
        // ----------------------------------------------------------------------
        Vector3 posCasilla = tablero.ObtenerPosicionCasilla(mov.destino.x, mov.destino.y);
        mov.ficha.transform.position = posCasilla;

        // ----------------------------------------------------------------------
        // 3. Actualizar coordenadas internas de la pieza
        // ----------------------------------------------------------------------
        PiezaAjedrez piezaScript = mov.ficha.GetComponent<PiezaAjedrez>();
        if (piezaScript != null)
        {
            piezaScript.ActualizarPosicion(mov.destino);
        }

        // ----------------------------------------------------------------------
        // 4. Refrescar BaseConocimiento (para que conozca las nuevas posiciones)
        // ----------------------------------------------------------------------
        baseConocimiento.Refrescar();

        
    }
}
