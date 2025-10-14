using UnityEngine;

public class FichaInteractiva : MonoBehaviour
{
    private bool IsDrag = false;
    private Vector3 offset; // Diferencia entre el mouse y el centro de la ficha
    private Camera camaraPrincipal;
    private Vector3 posicionInicial;

    private Entrada moduloEntrada;
    public GestorTurnos gestorTurnos;

    void Start()
    {
        camaraPrincipal = Camera.main;
        moduloEntrada = FindObjectOfType<Entrada>();
        gestorTurnos = FindObjectOfType<GestorTurnos>();

        
    }

    void OnMouseDown()
    {
        // Verifica si la entrada está habilitada antes de permitir movimiento
        if (moduloEntrada == null || !moduloEntrada.EstaHabilitada())
        {
            Debug.Log("No es el turno del jugador humano.");
            return;
        }
        
        IsDrag = true;
        posicionInicial = transform.position;
        moduloEntrada.RegistrarSeleccion(gameObject);
        

        // Diferencia entre el punto del mouse y el centro de la ficha
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
        moduloEntrada.RegistrarMovimiento(transform.position);

        // Búsqueda de la casilla más cercana al soltar
        GameObject casillaCercana = EncontrarCasillaMasCercana();
        if (casillaCercana != null)
        {
            transform.position = casillaCercana.transform.position; // Posición centrada de la ficha en la casilla
        }
        else
        {
            // Si no hay casilla cercana, vuelve al punto inicial
            transform.position = posicionInicial;
        }

        if (gestorTurnos != null)
        gestorTurnos.JugadaHumanoCompletada();
    }

    GameObject EncontrarCasillaMasCercana()
    {
        // Busca todas las casillas mediante la etiqueta “Casilla” (en el prefab "CasillaBase")
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
