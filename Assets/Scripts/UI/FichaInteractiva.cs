using UnityEngine;

public class FichaInteractiva : MonoBehaviour
{
    private bool IsDrag = false;
    private Vector3 offset; // Diferencia entre el mouse y el centro de la ficha
    private Camera camaraPrincipal;
    private Vector3 posicionInicial;

    private Entrada moduloEntrada;

    void Start()
    {
        camaraPrincipal = Camera.main;
        moduloEntrada = FindObjectOfType<Entrada>();
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
        if (!IsDrag) return;

        IsDrag = false;

        // ⚠️ SOLO avisamos a Entrada dónde soltamos la ficha.
        // Entrada decide si el movimiento es legal y recoloca la pieza.
        if (moduloEntrada != null)
        {
            moduloEntrada.RegistrarMovimiento(transform.position);
        }

        // ❌ IMPORTANTE:
        // YA NO buscamos casilla cercana NI movemos la ficha aquí.
        // Si el movimiento es legal: Entrada la mueve al centro de la casilla destino.
        // Si es ilegal: Entrada la regresa a posicionInicial.
    }
}
