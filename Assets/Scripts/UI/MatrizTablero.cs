using UnityEngine;

public class MatrizTablero : MonoBehaviour
{
    [Header("CONFIGURACIÓN DEL TABLERO")]
    public int filas = 8;
    public int columnas = 8;
    public GameObject prefabCasilla;   
    public float tamañoCasilla = 1f;
    public Transform contenedorCasillas;
    public SpriteRenderer fondoTablero;

    private GameObject[,] casillas;

    void Start()
    {
        casillas = new GameObject[filas, columnas];
        GenerarCasillas();
    }

    void GenerarCasillas()
    {
        // Tamaño visible total del fondo del tablero
        float ancho = fondoTablero.sprite.bounds.size.x * fondoTablero.transform.localScale.x;
        float alto = fondoTablero.sprite.bounds.size.y * fondoTablero.transform.localScale.y;

        float anchoCasilla = ancho / columnas;
        float altoCasilla = alto / filas;

        // Encuentra la esquina inferior izquierda del fondo
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
                casilla.name = $"Casilla_{fila}_{col}";

                // Ajuste de tamaño de cada casilla según el fondo real del tablero
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

    public Vector3 ObtenerPosicionCasilla(int fila, int columna)
    {
        return casillas[fila, columna].transform.position;
    }
}
