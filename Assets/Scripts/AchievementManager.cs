using UnityEngine;
using System.Collections.Generic; 

public class AchievementManager : MonoBehaviour
{
    public const string NIVEL_1_COMPLETADO = "Logro_Nivel_1";
    public const string NIVEL_2_COMPLETADO = "Logro_Nivel_2";
    public const string NIVEL_3_COMPLETADO = "Logro_Nivel_3";
    
    // Nueva constante para guardar el progreso
    public const string NIVEL_MAS_ALTO = "NivelMasAlto";

    public static AchievementManager Instance;
    public static int NivelACargar = 1;

    void Awake()
    {
        // Si ya existe una instancia y no soy yo, destruirme.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
  
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void UnlockAchievement(string achievementID)
    {
        // PlayerPrefs solo guarda valores simples. Guardamos un '1' para 'true'.
        PlayerPrefs.SetInt(achievementID, 1);
        PlayerPrefs.Save(); // Guarda los cambios en el disco
        Debug.Log("Logro desbloqueado: " + achievementID);
    }

    public bool IsAchievementUnlocked(string achievementID)
    {
        // GetInt(id, 0) significa: "Busca 'id'. Si no existe, devuelve '0' (falso)".
        return PlayerPrefs.GetInt(achievementID, 0) == 1;
    }

    public void ResetAllAchievements()
    {
        PlayerPrefs.DeleteKey(NIVEL_1_COMPLETADO);
        PlayerPrefs.DeleteKey(NIVEL_2_COMPLETADO);
        PlayerPrefs.DeleteKey(NIVEL_3_COMPLETADO);
        // (Añadir cualquier otro logro que crees)
        PlayerPrefs.Save();
        Debug.Log("Todos los logros reseteados.");
    }

    /**
     * Guarda el nivel más alto alcanzado.
     */
    public void SetHighestLevel(int level)
    {
        // Obtenemos el nivel más alto que ya teníamos guardado
        int currentHighest = PlayerPrefs.GetInt(NIVEL_MAS_ALTO, 1);
        
        // Solo guardamos si el nuevo nivel es MAYOR que el guardado
        if (level > currentHighest)
        {
            PlayerPrefs.SetInt(NIVEL_MAS_ALTO, level);
            PlayerPrefs.Save();
            Debug.Log("Nuevo nivel más alto guardado: " + level);
        }
    }

    /**
     * Obtiene el nivel más alto guardado.
     */
    public int GetHighestLevel()
    {
        // Si no hay nada guardado, devuelve 1 (el primer nivel)
        return PlayerPrefs.GetInt(NIVEL_MAS_ALTO, 1);
    }

    // Guarda las estrellas para un nivel específico
    public void SetStarsForLevel(int levelIndex, int stars)
    {
        string key = "Nivel_" + levelIndex + "_Estrellas";
        
        // Guardamos solo si las nuevas estrellas son MÁS que las que ya teníamos
        int currentStars = PlayerPrefs.GetInt(key, 0);
        if (stars > currentStars)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    // Obtiene las estrellas guardadas para un nivel
    public int GetStarsForLevel(int levelIndex)
    {
        string key = "Nivel_" + levelIndex + "_Estrellas";
        return PlayerPrefs.GetInt(key, 0); // Devuelve 0 si no hay nada guardado
    }
}