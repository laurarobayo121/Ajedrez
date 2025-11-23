using UnityEngine;

public class FichaInteractiva : MonoBehaviour
{
    private bool IsDrag = false;
    private Vector3 offset; 
    private Camera camaraPrincipal;
    private Vector3 posicionInicial;

    private Entrada moduloEntrada;
    public GestorTurnos gestorTurnos;

    private PiezaAjedrez datosPieza; // ⭐ referencia al color de la pieza

    void Start()
    {
        camaraPrincipal = Camera.main;
        moduloEntrada = FindObjectOfType<Entrada>();
        gestorTurnos = FindObjectOfType<GestorTurnos>();

        // ⭐ Guardamos referencia a los datos de la pieza
        datosPieza = GetComponent<PiezaAjedrez>();
    }

    void OnMouseDown()
    {
        // ❗ Primero: NO permitir mover piezas negras
        if (datosPieza != null && datosPieza.colorPieza == PiezaAjedrez.ColorPieza.Negro)
        {
            Debug.Log("⚠ No puedes mover piezas negras.");
            return;
        }

        // Verifica si es el turno del jugador humano
        if (moduloEntrada == null || !moduloEntrada.EstaHabilitada())
        {
            Debug.Log("No es el turno del jugador humano.");
            return;
        }

        IsDrag = true;
        posicionInicial = transform.position;
        moduloEntrada.RegistrarSeleccion(gameObject);

        Vector3 mouseWorld = camaraPrincipal.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        offset = transform.position - mouseWorld;
    }

    void OnMouseDrag()
    {
        if (IsDrag)
        {
            Vector3 mouseWorld = camaraPrincipal.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;
            transform.position = mouseWorld + offset;
        }
    }

    void OnMouseUp()
    {
        IsDrag = false;

        // Solo registrar movimiento si la pieza es del jugador
        if (datosPieza != null && datosPieza.colorPieza == PiezaAjedrez.ColorPieza.Blanco)
        {
            moduloEntrada.RegistrarMovimiento(transform.position);
        }

        GameObject casillaCercana = EncontrarCasillaMasCercana();
        if (casillaCercana != null)
        {
            transform.position = casillaCercana.transform.position;
        }
        else
        {
            transform.position = posicionInicial;
        }

        if (gestorTurnos != null)
            gestorTurnos.JugadaHumanoCompletada();
    }

    GameObject EncontrarCasillaMasCercana()
    {
        GameObject[] casillas = GameObject.FindGameObjectsWithTag("Casilla");
        GameObject masCercana = null;
        float menorDistancia = Mathf.Infinity;

        foreach (GameObject c in casillas)
        {
            float distancia = Vector3.Distance(transform.position, c.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                masCercana = c;
            }
        }

        return masCercana;
    }
}
