using System.Collections;
using UnityEngine;

public class ColocarFichas : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public MatrizTablero tablero;  // Asignar el tablero
    public Transform contenedorFichas; // Un objeto vacío que almacena todas las fichas

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
        // Esperar un frame para asegurarse que MatrizTablero haya corrido Start()
        yield return null;
        ColocarPiezasIniciales();
    }

    void ColocarPiezasIniciales()
    {
        // === FILAS BLANCAS ===
        // Fila 0: piezas mayores
        InstanciarFicha(torreBlanca, 0, 0);
        InstanciarFicha(caballoBlanco, 0, 1);
        InstanciarFicha(alfilBlanco, 0, 2);
        InstanciarFicha(reinaBlanca, 0, 3);
        InstanciarFicha(reyBlanco, 0, 4);
        InstanciarFicha(alfilBlanco, 0, 5);
        InstanciarFicha(caballoBlanco, 0, 6);
        InstanciarFicha(torreBlanca, 0, 7);

        // Fila 1: peones
        for (int col = 0; col < 8; col++)
        {
            InstanciarFicha(peonBlanco, 1, col);
        }

        // === FILAS NEGRAS ===
        // Fila 7: piezas mayores
        InstanciarFicha(torreNegra, 7, 0);
        InstanciarFicha(caballoNegro, 7, 1);
        InstanciarFicha(alfilNegro, 7, 2);
        InstanciarFicha(reinaNegra, 7, 3);
        InstanciarFicha(reyNegro, 7, 4);
        InstanciarFicha(alfilNegro, 7, 5);
        InstanciarFicha(caballoNegro, 7, 6);
        InstanciarFicha(torreNegra, 7, 7);

        // Fila 6: peones
        for (int col = 0; col < 8; col++)
        {
            InstanciarFicha(peonNegro, 6, col);
        }
    }

    void InstanciarFicha(GameObject prefab, int fila, int columna)
    {
        Vector3 posicion = tablero.ObtenerPosicionCasilla(fila, columna);
        GameObject ficha = Instantiate(prefab, posicion, Quaternion.identity, contenedorFichas);
        ficha.name = $"{prefab.name}_{fila}_{columna}";
    }
}
