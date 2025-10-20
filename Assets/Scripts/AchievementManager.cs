using UnityEngine;
using System.Collections.Generic; 

public class AchievementManager : MonoBehaviour
{
    public const string NIVEL_1_COMPLETADO = "Logro_Nivel_1";
    public const string NIVEL_2_COMPLETADO = "Logro_Nivel_2";
    public const string NIVEL_3_COMPLETADO = "Logro_Nivel_3";
    
    public const string NIVEL_MAS_ALTO = "NivelMasAlto";

    public static AchievementManager Instance;
    public static int NivelACargar = 1;

    void Awake()
    {
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
        PlayerPrefs.SetInt(achievementID, 1);
        PlayerPrefs.Save();
        Debug.Log("Logro desbloqueado: " + achievementID);
    }

    public bool IsAchievementUnlocked(string achievementID)
    {
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

    public void SetHighestLevel(int level)
    {
        int currentHighest = PlayerPrefs.GetInt(NIVEL_MAS_ALTO, 1);

        if (level > currentHighest)
        {
            PlayerPrefs.SetInt(NIVEL_MAS_ALTO, level);
            PlayerPrefs.Save();
            Debug.Log("Nuevo nivel más alto guardado: " + level);
        }
    }

    public int GetHighestLevel()
    {
        return PlayerPrefs.GetInt(NIVEL_MAS_ALTO, 1);
    }

    // Guarda las estrellas para un nivel específico
    public void SetStarsForLevel(int levelIndex, int stars)
    {
        string key = "Nivel_" + levelIndex + "_Estrellas";

        int currentStars = PlayerPrefs.GetInt(key, 0);
        if (stars > currentStars)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    public int GetStarsForLevel(int levelIndex)
    {
        string key = "Nivel_" + levelIndex + "_Estrellas";
        return PlayerPrefs.GetInt(key, 0); 
    }
}