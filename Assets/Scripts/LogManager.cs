using UnityEngine;
using System.IO; 
using System.Text; 

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        stringBuilder = new StringBuilder();
    }


    public string idJugador = "Player_Default";
    
    private string logFilePath;
    private StringBuilder stringBuilder;

    void Start()
    {
        logFilePath = Path.Combine(Application.dataPath, "../log_loopmania.csv");

        Debug.Log("Ruta del Log: " + logFilePath);

        if (!File.Exists(logFilePath))
        {
            stringBuilder.AppendLine("Timestamp,ID_Jugador,Nivel,Bloques_Disponibles,Secuencia_Bloques,Gano_Intento,Tiempo_Intento_Seg");
            File.WriteAllText(logFilePath, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }

    public void LogAttempt(string nivel, string bloquesDisponibles, string secuenciaJugador, bool ganoIntento, float tiempoIntento)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string secuenciaLimpia = secuenciaJugador.Replace(',', ';');
        string bloquesLimpios = bloquesDisponibles.Replace(',', ';');

        stringBuilder.Clear();
        stringBuilder.Append(timestamp).Append(",");
        stringBuilder.Append(idJugador).Append(",");
        stringBuilder.Append(nivel).Append(",");
        stringBuilder.Append(bloquesLimpios).Append(",");
        stringBuilder.Append(secuenciaLimpia).Append(",");
        stringBuilder.Append(ganoIntento.ToString()).Append(",");
        stringBuilder.Append(tiempoIntento.ToString("F2")); 

        try
        {
            File.AppendAllText(logFilePath, stringBuilder.ToString() + "\n");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR AL ESCRIBIR EN EL LOG: " + e.Message);
        }
    }
}