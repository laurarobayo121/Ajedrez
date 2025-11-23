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

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoHumano;
    public AudioClip sonidoIA;


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

    private void ReiniciarTemporizador()
    {
        tiempoRestante = tiempoPorTurno;
        if (textoTemporizador != null)
            textoTemporizador.text = Mathf.Ceil(tiempoRestante).ToString();
    }


    public void IniciarPartida()
    { 
        Debug.Log("🔵 Inicia partida — Turno del HUMANO");
        turnoActual = Turno.Humano;
        IniciarTemporizadorHumano();
        
    }

    private void IniciarTemporizadorHumano()
    { 
        Debug.Log("🟢 Turno del humano iniciado. Temporizador activado.");
        ReproducirSonidoHumano();
        tiempoRestante = tiempoPorTurno;
        temporizadorActivo = true;
        motorEntrada.HabilitarEntrada(true);
    }

    private void IniciarTemporizadorIA()
    {
        Debug.Log("🤖 Temporizador de la IA reiniciado (aunque no cuente regresivamente).");
        ReproducirSonidoIA();
        ReiniciarTemporizador(); // Deja el tiempo en 10 siempre
    }

    // Se llama cuando el jugador humano suelta una ficha
    public void JugadaHumanoCompletada()
    {
        if (turnoActual != Turno.Humano) return;

        Debug.Log("🟡 Movimiento del humano completado. Cambiando a la IA...");

        temporizadorActivo = false;
        motorEntrada.HabilitarEntrada(false);
        CambiarATurnoIA();
    }

    private void CambiarATurnoIA()
    { 
        Debug.Log("🔴 Turno de la IA iniciado.");
        turnoActual = Turno.IA;
        temporizadorActivo = true;
        motorEntrada.HabilitarEntrada(false);

        ReiniciarTemporizador();
        IniciarTemporizadorIA();


        // Ejecuta la jugada de la IA
        motorIA.RealizarJugada();

        // Espera un momento y vuelve al turno humano
        StartCoroutine(EsperarFinIA());
    }

    private IEnumerator EsperarFinIA()
    {
        // Puedes ajustar el tiempo según la duración de la jugada de la IA
        yield return new WaitForSeconds(1.5f);
        temporizadorActivo = false;
        Debug.Log("🟣 La IA terminó su jugada. Regresando al turno humano.");
        turnoActual = Turno.Humano;
        IniciarTemporizadorHumano();
    }

    private void ReproducirSonidoHumano()
    {
        if (audioSource != null && sonidoHumano != null)
            audioSource.PlayOneShot(sonidoHumano);
    }

    private void ReproducirSonidoIA()
    {
        if (audioSource != null && sonidoIA != null)
            audioSource.PlayOneShot(sonidoIA);
    }


}
