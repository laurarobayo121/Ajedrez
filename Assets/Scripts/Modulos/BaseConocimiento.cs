using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BaseConocimiento : MonoBehaviour
{
    [SerializeField] private string escenaVictoriaBlancas = "Victoria";
    [SerializeField] private string escenaVictoriaNegras = "GameOver";

    private Dictionary<Vector2Int, PiezaAjedrez> piezasPorPosicion = new Dictionary<Vector2Int, PiezaAjedrez>();

    void Start() { ActualizarRegistroPiezas(); }

    public void Refrescar() { ActualizarRegistroPiezas(); }

    public void ActualizarRegistroPiezas()
    {
        piezasPorPosicion.Clear();
        PiezaAjedrez[] todas = Object.FindObjectsByType<PiezaAjedrez>(FindObjectsSortMode.None);
        foreach (var p in todas)
            piezasPorPosicion[p.posicionActual] = p;
    }

    public bool HayPiezaEnPosicion(Vector2Int pos) => piezasPorPosicion.ContainsKey(pos);

    public PiezaAjedrez.ColorPieza ObtenerColorPiezaEnPosicion(Vector2Int pos)
    {
        if (piezasPorPosicion.TryGetValue(pos, out PiezaAjedrez p))
            return p.colorPieza;
        return PiezaAjedrez.ColorPieza.Blanco;
    }

    public bool VerificarMovimientoLegal(Movimiento mov)
    {
        if (mov == null || mov.ficha == null) return false;

        PiezaAjedrez pieza = mov.ficha.GetComponent<PiezaAjedrez>();
        Vector2Int origen = pieza.posicionActual;
        Vector2Int destino = mov.destino;

        ActualizarRegistroPiezas();

        if (!DentroTablero(destino) || origen == destino) return false;
        if (HayPiezaEnPosicion(destino) && piezasPorPosicion[destino].colorPieza == pieza.colorPieza)
            return false;
        if (!EsMovimientoValidoPorTipo(pieza, origen, destino)) return false;
        if (EsPiezaDeslizante(pieza.tipoPieza) && HayPiezasEntre(origen, destino)) return false;

        // Captura enemiga
        if (piezasPorPosicion.TryGetValue(destino, out PiezaAjedrez capturada))
        {
            if (capturada.colorPieza != pieza.colorPieza)
            {
                Destroy(capturada.gameObject);
                if (capturada.tipoPieza == PiezaAjedrez.TipoPieza.Rey)
                    CargarEscenaVictoria(pieza.colorPieza);
            }
        }

        // Mover ficha
        pieza.ActualizarPosicion(destino);
        ActualizarRegistroPiezas();
        return true;
    }

    private bool DentroTablero(Vector2Int pos) => pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;

    private bool EsPiezaDeslizante(PiezaAjedrez.TipoPieza t) => t == PiezaAjedrez.TipoPieza.Torre || t == PiezaAjedrez.TipoPieza.Alfil || t == PiezaAjedrez.TipoPieza.Reina;

    private bool HayPiezasEntre(Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
        Vector2Int pos = new Vector2Int(origen.x + stepX, origen.y + stepY);

        while (pos != destino)
        {
            if (HayPiezaEnPosicion(pos)) return true;
            pos.x += stepX; pos.y += stepY;
        }
        return false;
    }

    private bool EsMovimientoValidoPorTipo(PiezaAjedrez pieza, Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;
        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        switch (pieza.tipoPieza)
        {
            case PiezaAjedrez.TipoPieza.Peon:
                int dir = (pieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco) ? 1 : -1;
                if (dy == 0 && dx == dir) return !HayPiezaEnPosicion(destino);
                if (dy == 0 && dx == 2 * dir && ((origen.x == 1 && pieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco) || (origen.x == 6 && pieza.colorPieza == PiezaAjedrez.ColorPieza.Negro)))
                    return !HayPiezaEnPosicion(new Vector2Int(origen.x + dir, origen.y)) && !HayPiezaEnPosicion(destino);
                if (Mathf.Abs(dy) == 1 && dx == dir) return HayPiezaEnPosicion(destino) && ObtenerColorPiezaEnPosicion(destino) != pieza.colorPieza;
                return false;
            case PiezaAjedrez.TipoPieza.Torre: return dx == 0 || dy == 0;|
            case PiezaAjedrez.TipoPieza.Alfil: return absDx == absDy;
            case PiezaAjedrez.TipoPieza.Reina: return dx == 0 || dy == 0 || absDx == absDy;
            case PiezaAjedrez.TipoPieza.Caballo: return (absDx == 2 && absDy == 1) || (absDx == 1 && absDy == 2);
            case PiezaAjedrez.TipoPieza.Rey: return absDx <= 1 && absDy <= 1;
            default: return false;
        }
    }

    private void CargarEscenaVictoria(PiezaAjedrez.ColorPieza ganador)
    {
        string escena = ganador == PiezaAjedrez.ColorPieza.Blanco ? escenaVictoriaBlancas : escenaVictoriaNegras;
        SceneManager.LoadScene(escena);
    }
}
