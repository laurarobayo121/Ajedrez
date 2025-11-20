using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena : MonoBehaviour
{
    public string nombreEscena;

    public void Cambiar()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}
