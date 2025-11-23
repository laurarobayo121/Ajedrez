using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CambioEscena : MonoBehaviour
{
    public string nombreEscena;
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += CambiarCuandoTermine;
    }

    public void Cambiar()
    {
        SceneManager.LoadScene(nombreEscena);
    }

    private void CambiarCuandoTermine(VideoPlayer vp)
    {
        Cambiar();
    }
}
