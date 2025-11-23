using UnityEngine;

public class MatrizTablero : MonoBehaviour
{
    [Header("CONFIGURACIÓN DEL TABLERO")]
    public int filas = 8;
    public int columnas = 8;
    public GameObject prefabCasilla;     // Prefab de la casilla
    public float tamañoCasilla = 1f;     // No lo usamos directamente porque calculamos según el sprite
    public Transform contenedorCasillas; // Padre donde van todas las casillas
    public SpriteRenderer fondoTablero;  // Sprite del tablero

    private GameObject[,] casillas;

    void Start()
    {
        casillas = new GameObject[filas, columnas];
        GenerarCasillas();
    }

    void GenerarCasillas()
    {
        // Tamaño visible del sprite del tablero
        float ancho = fondoTablero.sprite.bounds.size.x * fondoTablero.transform.localScale.x;
        float alto  = fondoTablero.sprite.bounds.size.y * fondoTablero.transform.localScale.y;

        float anchoCasilla = ancho / columnas;
        float altoCasilla  = alto  / filas;

        // Esquina inferior izquierda del tablero en mundo
        Vector3 esquinaInferiorIzquierda = fondoTablero.bounds.min;

        for (int fila = 0; fila < filas; fila++)
        {
            for (int col = 0; col < columnas; col++)
            {
                // Posición central de cada casilla
                Vector3 posicion = new Vector3(
                    esquinaInferiorIzquierda.x + (col + 0.5f) * anchoCasilla,
                    esquinaInferiorIzquierda.y + (fila + 0.5f) * altoCasilla,
                    fondoTablero.transform.position.z - 0.1f
                );

                GameObject casilla = Instantiate(prefabCasilla, posicion, Quaternion.identity, contenedorCasillas);
               
                // Info básica
                casilla.transform.localScale = new Vector3(anchoCasilla, altoCasilla, 1f);

                CasillaInfo info = casilla.AddComponent<CasillaInfo>();
                info.fila = fila;
                info.columna = col;
                info.notacion = LenguajeAjedrez.ANotacion(fila, col, filas);

                casilla.name = $"Casilla_{info.notacion}";

                casillas[fila, col] = casilla;
            }
        }
    }

    /// <summary>
    /// Devuelve la posición en mundo del centro de una casilla.
    /// (La usa Entrada, etc.)
    /// </summary>
    public Vector3 ObtenerPosicionCasilla(int fila, int columna)
    {
        return casillas[fila, columna].transform.position;
    }

    /// <summary>
    /// Devuelve el GameObject de la casilla [fila, columna].
    /// (La usa Inferencia para colocar visualmente las piezas de la IA.)
    /// </summary>
    public GameObject GetCasilla(int fila, int columna)
    {
        if (fila < 0 || fila >= filas || columna < 0 || columna >= columnas)
        {
            Debug.LogError($"[MatrizTablero] Casilla fuera de rango: ({fila}, {columna})");
            return null;
        }

        return casillas[fila, columna];
    }
}
