using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorPartida : MonoBehaviour
{
    public string escenaVictoria = "Victoria";
    public string escenaGameOver = "GameOver";

    // Se llamará cuando una pieza capture a otra
    public void NotificarCaptura(PiezaAjedrez piezaCapturada)
    {
        if (piezaCapturada == null) return;

        if (piezaCapturada.esRey)
        {
            if (piezaCapturada.esIA)
            {
                Debug.Log("👑💥 Rey de la IA capturado — ¡Victoria!");
                SceneManager.LoadScene(escenaVictoria);
            }
            else
            {
                Debug.Log("👑💀 Rey del jugador capturado — Game Over");
                SceneManager.LoadScene(escenaGameOver);
            }
        }
    }
}
