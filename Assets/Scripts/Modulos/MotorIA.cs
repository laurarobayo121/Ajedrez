using UnityEngine;

public class MotorIA : MonoBehaviour
{
    public MatrizTablero tablero; // referencia opcional si ya tienes clase Tablero

    // Este método lo llama el GestorTurnos
    public void RealizarJugada()
    {
        Debug.Log("IA pensando su jugada...");

        // Simula un tiempo de "pensamiento" antes de mover
        Invoke(nameof(SimularMovimiento), 1.5f);
    }

    private void SimularMovimiento()
    {
        Debug.Log("IA realizó su jugada (simulada)");

        // Aquí más adelante colocarás la lógica real del motor de inferencia
        // Por ejemplo:
        // 1. Calcular mejor movimiento.
        // 2. Mover pieza.
        // 3. Actualizar tablero.
    }
}
