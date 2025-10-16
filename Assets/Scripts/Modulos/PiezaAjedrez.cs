using UnityEngine;
using System.Collections.Generic;

public class PiezaAjedrez : MonoBehaviour
{
    public enum TipoPieza { Peon, Torre, Caballo, Alfil, Reina, Rey }
    public enum ColorPieza { Blanco, Negro }

    [Header("Configuración de Pieza")]
    public TipoPieza tipoPieza;
    public ColorPieza colorPieza;
    public Vector2Int posicionActual;

    private Entrada moduloEntrada;
    private BaseConocimiento baseConocimiento;

    void Start()
    {
        moduloEntrada = FindObjectOfType<Entrada>();
        baseConocimiento = FindObjectOfType<BaseConocimiento>();
        
        // Obtener posición inicial desde el nombre del objeto
        ObtenerPosicionDesdeNombre();
    }

    void ObtenerPosicionDesdeNombre()
    {
        string[] partes = gameObject.name.Split('_');
        if (partes.Length >= 3)
        {
            int fila = int.Parse(partes[partes.Length - 2]);
            int columna = int.Parse(partes[partes.Length - 1]);
            posicionActual = new Vector2Int(fila, columna);
        }
    }

    public void ActualizarPosicion(Vector2Int nuevaPosicion)
    {
        posicionActual = nuevaPosicion;
    }

    public List<Vector2Int> ObtenerMovimientosLegales()
    {
        switch (tipoPieza)
        {
            case TipoPieza.Peon:
                return ObtenerMovimientosPeon();
            case TipoPieza.Torre:
                return ObtenerMovimientosTorre();
            case TipoPieza.Caballo:
                return ObtenerMovimientosCaballo();
            case TipoPieza.Alfil:
                return ObtenerMovimientosAlfil();
            case TipoPieza.Reina:
                return ObtenerMovimientosReina();
            case TipoPieza.Rey:
                return ObtenerMovimientosRey();
            default:
                return new List<Vector2Int>();
        }
    }

    private List<Vector2Int> ObtenerMovimientosPeon()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();
        int direccion = (colorPieza == ColorPieza.Blanco) ? 1 : -1;

        // Movimiento hacia adelante (1 casilla)
        Vector2Int movimientoAdelante = new Vector2Int(posicionActual.x + direccion, posicionActual.y);
        if (EsCasillaValida(movimientoAdelante))
        {
            movimientos.Add(movimientoAdelante);
        }

        // Movimiento inicial de 2 casillas
        if ((colorPieza == ColorPieza.Blanco && posicionActual.x == 1) || 
            (colorPieza == ColorPieza.Negro && posicionActual.x == 6))
        {
            Vector2Int movimientoDoble = new Vector2Int(posicionActual.x + (2 * direccion), posicionActual.y);
            if (EsCasillaValida(movimientoDoble))
            {
                movimientos.Add(movimientoDoble);
            }
        }

        // Capturas en diagonal
        Vector2Int capturaIzquierda = new Vector2Int(posicionActual.x + direccion, posicionActual.y - 1);
        Vector2Int capturaDerecha = new Vector2Int(posicionActual.x + direccion, posicionActual.y + 1);

        if (EsCasillaValida(capturaIzquierda))
            movimientos.Add(capturaIzquierda);
        if (EsCasillaValida(capturaDerecha))
            movimientos.Add(capturaDerecha);

        return movimientos;
    }

    private List<Vector2Int> ObtenerMovimientosTorre()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();

        // Movimiento horizontal y vertical
        int[] direccionesX = { 1, -1, 0, 0 };
        int[] direccionesY = { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            for (int distancia = 1; distancia < 8; distancia++)
            {
                Vector2Int nuevaPos = new Vector2Int(
                    posicionActual.x + (direccionesX[i] * distancia),
                    posicionActual.y + (direccionesY[i] * distancia)
                );

                if (!EsCasillaValida(nuevaPos)) break;

                movimientos.Add(nuevaPos);
            }
        }

        return movimientos;
    }

    private List<Vector2Int> ObtenerMovimientosCaballo()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();

        // Todos los movimientos en L posibles
        int[] movimientosX = { 2, 2, -2, -2, 1, 1, -1, -1 };
        int[] movimientosY = { 1, -1, 1, -1, 2, -2, 2, -2 };

        for (int i = 0; i < 8; i++)
        {
            Vector2Int nuevaPos = new Vector2Int(
                posicionActual.x + movimientosX[i],
                posicionActual.y + movimientosY[i]
            );

            if (EsCasillaValida(nuevaPos))
            {
                movimientos.Add(nuevaPos);
            }
        }

        return movimientos;
    }

    private List<Vector2Int> ObtenerMovimientosAlfil()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();

        // Movimiento diagonal
        int[] direccionesX = { 1, 1, -1, -1 };
        int[] direccionesY = { 1, -1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            for (int distancia = 1; distancia < 8; distancia++)
            {
                Vector2Int nuevaPos = new Vector2Int(
                    posicionActual.x + (direccionesX[i] * distancia),
                    posicionActual.y + (direccionesY[i] * distancia)
                );

                if (!EsCasillaValida(nuevaPos)) break;

                movimientos.Add(nuevaPos);
            }
        }

        return movimientos;
    }

    private List<Vector2Int> ObtenerMovimientosReina()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();

        // Combinación de torre y alfil
        movimientos.AddRange(ObtenerMovimientosTorre());
        movimientos.AddRange(ObtenerMovimientosAlfil());

        return movimientos;
    }

    private List<Vector2Int> ObtenerMovimientosRey()
    {
        List<Vector2Int> movimientos = new List<Vector2Int>();

        // Todas las direcciones adyacentes
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Saltar la posición actual

                Vector2Int nuevaPos = new Vector2Int(posicionActual.x + x, posicionActual.y + y);

                if (EsCasillaValida(nuevaPos))
                {
                    movimientos.Add(nuevaPos);
                }
            }
        }

        return movimientos;
    }

    private bool EsCasillaValida(Vector2Int posicion)
    {
        return posicion.x >= 0 && posicion.x < 8 && posicion.y >= 0 && posicion.y < 8;
    }

    public bool EsMovimientoLegal(Vector2Int destino)
    {
        List<Vector2Int> movimientosLegales = ObtenerMovimientosLegales();
        return movimientosLegales.Contains(destino);
    }
}