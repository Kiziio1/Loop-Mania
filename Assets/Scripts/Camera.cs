using UnityEngine;

public class CameraFit : MonoBehaviour
{
    // El tamaño de diseño que usaste (probablemente 16:9)
    public float targetAspectRatio = 16f / 9f;
    
    // El tamaño ortográfico que usas en tu editor y se ve bien
    public float initialOrthographicSize = 5f; // AJUSTA ESTO A TU VALOR ACTUAL

    void Start()
    {
        // 1. Calcular el ratio actual de la pantalla (la del usuario)
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 2. Calcular la escala necesaria
        float scaleHeight = windowAspect / targetAspectRatio;

        Camera cam = GetComponent<Camera>();

        // 3. Si la pantalla es más angosta (ej. celular o laptop vieja), aumentamos el zoom out
        if (scaleHeight < 1.0f)
        {
            cam.orthographicSize = initialOrthographicSize / scaleHeight;
        }
        else
        {
            // Si es más ancha, mantenemos la altura (o podrías ajustar también)
            cam.orthographicSize = initialOrthographicSize;
        }
    }
}