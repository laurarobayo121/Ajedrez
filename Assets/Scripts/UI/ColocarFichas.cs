using System.Collections;
using UnityEngine;

public class ColocarFichas : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public MatrizTablero tablero;
    public Transform contenedorFichas;

    [Header("PREFABS DE FICHAS BLANCAS")]
    public GameObject torreBlanca;
    public GameObject caballoBlanco;
    public GameObject alfilBlanco;
    public GameObject reinaBlanca;
    public GameObject reyBlanco;
    public GameObject peonBlanco;

    [Header("PREFABS DE FICHAS NEGRAS")]
    public GameObject torreNegra;
    public GameObject caballoNegro;
    public GameObject alfilNegro;
    public GameObject reinaNegra;
    public GameObject reyNegro;
    public GameObject peonNegro;

    IEnumerator Start()
    {
        yield return null;
        ColocarPiezasIniciales();
    }

    void ColocarPiezasIniciales()
    {
        // === PIEZAS BLANCAS ===
        // Fila 0: piezas mayores
        InstanciarFicha(torreBlanca, 0, 0, PiezaAjedrez.TipoPieza.Torre, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(caballoBlanco, 0, 1, PiezaAjedrez.TipoPieza.Caballo, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(alfilBlanco, 0, 2, PiezaAjedrez.TipoPieza.Alfil, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(reinaBlanca, 0, 3, PiezaAjedrez.TipoPieza.Reina, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(reyBlanco, 0, 4, PiezaAjedrez.TipoPieza.Rey, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(alfilBlanco, 0, 5, PiezaAjedrez.TipoPieza.Alfil, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(caballoBlanco, 0, 6, PiezaAjedrez.TipoPieza.Caballo, PiezaAjedrez.ColorPieza.Blanco);
        InstanciarFicha(torreBlanca, 0, 7, PiezaAjedrez.TipoPieza.Torre, PiezaAjedrez.ColorPieza.Blanco);

        // Fila 1: peones
        for (int col = 0; col < 8; col++)
        {
            InstanciarFicha(peonBlanco, 1, col, PiezaAjedrez.TipoPieza.Peon, PiezaAjedrez.ColorPieza.Blanco);
        }

        // === PIEZAS NEGRAS ===
        // Fila 7: piezas mayores
        InstanciarFicha(torreNegra, 7, 0, PiezaAjedrez.TipoPieza.Torre, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(caballoNegro, 7, 1, PiezaAjedrez.TipoPieza.Caballo, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(alfilNegro, 7, 2, PiezaAjedrez.TipoPieza.Alfil, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(reinaNegra, 7, 3, PiezaAjedrez.TipoPieza.Reina, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(reyNegro, 7, 4, PiezaAjedrez.TipoPieza.Rey, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(alfilNegro, 7, 5, PiezaAjedrez.TipoPieza.Alfil, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(caballoNegro, 7, 6, PiezaAjedrez.TipoPieza.Caballo, PiezaAjedrez.ColorPieza.Negro);
        InstanciarFicha(torreNegra, 7, 7, PiezaAjedrez.TipoPieza.Torre, PiezaAjedrez.ColorPieza.Negro);

        // Fila 6: peones
        for (int col = 0; col < 8; col++)
        {
            InstanciarFicha(peonNegro, 6, col, PiezaAjedrez.TipoPieza.Peon, PiezaAjedrez.ColorPieza.Negro);
        }
    }

    void InstanciarFicha(GameObject prefab, int fila, int columna, PiezaAjedrez.TipoPieza tipo, PiezaAjedrez.ColorPieza color)
    {
        Vector3 posicion = tablero.ObtenerPosicionCasilla(fila, columna);
        GameObject ficha = Instantiate(prefab, posicion, Quaternion.identity, contenedorFichas);
        ficha.name = $"{prefab.name}_{fila}_{columna}";

        // Añadir y configurar el componente PiezaAjedrez
        PiezaAjedrez piezaScript = ficha.AddComponent<PiezaAjedrez>();
        piezaScript.tipoPieza = tipo;
        piezaScript.colorPieza = color;
        piezaScript.posicionActual = new Vector2Int(fila, columna);

        // Asegurarse de que tenga el componente FichaInteractiva
        if (ficha.GetComponent<FichaInteractiva>() == null)
        {
            ficha.AddComponent<FichaInteractiva>();
        }
    }
}