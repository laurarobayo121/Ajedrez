using UnityEngine;

public class MutearMusica : MonoBehaviour
{
    public AudioSource musica;

    private bool muteado = false;

    public void ToggleMute()
    {
        muteado = !muteado;
        musica.mute = muteado;
    }
}
