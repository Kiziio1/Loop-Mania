using UnityEngine;
using System.IO; // Necesario para escribir archivos
using System.Text; // Necesario para construir el string eficientemente

public class LogManager : MonoBehaviour
{
    // --- Singleton Pattern ---
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
        
        // Inicializa el constructor de strings
        stringBuilder = new StringBuilder();
    }
    // --- Fin del Singleton Pattern ---

    public string idJugador = "Player_Default"; // El MainMenu lo sobrescribirá
    
    private string logFilePath;
    private StringBuilder stringBuilder;

    void Start()
    {
        // Define la ruta del archivo. Application.dataPath es la carpeta _Data
        // "/../" sube un nivel, quedando al lado del .exe
        logFilePath = Path.Combine(Application.dataPath, "../log_loopmania.csv");

        Debug.Log("Ruta del Log: " + logFilePath);

        // Si el archivo no existe, crea uno nuevo y escribe la cabecera (header)
        if (!File.Exists(logFilePath))
        {
            stringBuilder.AppendLine("Timestamp,ID_Jugador,Nivel,Bloques_Disponibles,Secuencia_Bloques,Gano_Intento,Tiempo_Intento_Seg");
            File.WriteAllText(logFilePath, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }

    public void LogAttempt(string nivel, string bloquesDisponibles, string secuenciaJugador, bool ganoIntento, float tiempoIntento)
    {
        // 1. Obtener la fecha y hora actual
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 2. Limpiar los strings para CSV (evitar que una coma rompa el formato)
        // Reemplazamos comas por puntos y comas.
        string secuenciaLimpia = secuenciaJugador.Replace(',', ';');
        string bloquesLimpios = bloquesDisponibles.Replace(',', ';');

        // 3. Construir la línea del CSV
        stringBuilder.Clear();
        stringBuilder.Append(timestamp).Append(",");
        stringBuilder.Append(idJugador).Append(",");
        stringBuilder.Append(nivel).Append(",");
        stringBuilder.Append(bloquesLimpios).Append(",");
        stringBuilder.Append(secuenciaLimpia).Append(",");
        stringBuilder.Append(ganoIntento.ToString()).Append(",");
        stringBuilder.Append(tiempoIntento.ToString("F2")); // "F2" = 2 decimales

        // 4. Escribir la línea en el archivo
        try
        {
            // Usamos File.AppendAllText para añadir la línea al final sin borrar lo anterior
            File.AppendAllText(logFilePath, stringBuilder.ToString() + "\n");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR AL ESCRIBIR EN EL LOG: " + e.Message);
        }
    }
}