using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GestorTurnos : MonoBehaviour
{
    public enum Turno { Humano, IA }
    public Turno turnoActual = Turno.Humano;

    [Header("LIMITE DE TIEMPO")]
    public float tiempoPorTurno = 10f;
    private float tiempoRestante;
    private bool temporizadorActivo = false;

    [Header("Referencias")]
    public TMP_Text textoTemporizador;
    public Entrada motorEntrada; // tu clase que controla al jugador
    public Inferencia motorIA;        // el script de la IA

    void Start()
    {
        IniciarPartida();
    }

    void Update()
    {
        if (temporizadorActivo)
        {
            tiempoRestante -= Time.deltaTime;
            if (textoTemporizador != null)
                textoTemporizador.text = Mathf.Ceil(tiempoRestante).ToString();

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                temporizadorActivo = false;
                CambiarATurnoIA();
            }
        }
    }

    public void IniciarPartida()
    {
        turnoActual = Turno.Humano;
        IniciarTemporizadorHumano();
    }

    private void IniciarTemporizadorHumano()
    {
        tiempoRestante = tiempoPorTurno;
        temporizadorActivo = true;
        motorEntrada.HabilitarEntrada(true);
    }

    // Se llama cuando el jugador humano suelta una ficha
    public void JugadaHumanoCompletada()
    {
        if (turnoActual != Turno.Humano) return;

        temporizadorActivo = false;
        motorEntrada.HabilitarEntrada(false);
        CambiarATurnoIA();
    }

    private void CambiarATurnoIA()
    {
        turnoActual = Turno.IA;
        motorEntrada.HabilitarEntrada(false);

        // Ejecuta la jugada de la IA
        motorIA.RealizarJugada();

        // Espera un momento y vuelve al turno humano
        StartCoroutine(EsperarFinIA());
    }

    private IEnumerator EsperarFinIA()
    {
        // Puedes ajustar el tiempo según la duración de la jugada de la IA
        yield return new WaitForSeconds(1.5f);
        turnoActual = Turno.Humano;
        IniciarTemporizadorHumano();
    }
}
