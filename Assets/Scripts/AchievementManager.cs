using UnityEngine;
using System.Collections.Generic; 

public class AchievementManager : MonoBehaviour
{
    public const string NIVEL_1_COMPLETADO = "Logro_Nivel_1";
    public const string NIVEL_2_COMPLETADO = "Logro_Nivel_2";
    public const string NIVEL_3_COMPLETADO = "Logro_Nivel_3";
    public const string NIVEL_MAS_ALTO = "NivelMasAlto";
    public const string TUTORIAL_1_VISTO = "Tutorial_1_Visto";
    public const string TUTORIAL_6_VISTO = "Tutorial_6_Visto";
    public const string TUTORIAL_11_VISTO = "Tutorial_11_Visto";
    public const string LOGRO_SPEEDRUNNER = "Logro_Speedrunner";
    public const string LOGRO_PENSADOR_LOGICO = "Logro_PensadorLogico";
    public const string LOGRO_MAESTRO_BUCLE = "Logro_MaestroBucle";
    public const string LOGRO_ARQUITECTO = "Logro_Arquitecto";
    public const string LOGRO_OPTIMIZADOR = "Logro_Optimizador";
    public const string LOGRO_TENAZ = "Logro_SolucionadorTenaz";

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

        PlayerPrefs.DeleteKey(LOGRO_SPEEDRUNNER);
        PlayerPrefs.DeleteKey(LOGRO_PENSADOR_LOGICO);
        PlayerPrefs.DeleteKey(LOGRO_MAESTRO_BUCLE);
        PlayerPrefs.DeleteKey(LOGRO_ARQUITECTO);
        PlayerPrefs.DeleteKey(LOGRO_OPTIMIZADOR);
        PlayerPrefs.DeleteKey(LOGRO_TENAZ);

        PlayerPrefs.DeleteKey(TUTORIAL_1_VISTO);
        PlayerPrefs.DeleteKey(TUTORIAL_6_VISTO);
        PlayerPrefs.DeleteKey(TUTORIAL_11_VISTO);

        int currentHighest = GetHighestLevel();
        for (int i = 1; i <= currentHighest; i++)
        {
            string key = "Nivel_" + i + "_Estrellas";
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.DeleteKey(NIVEL_MAS_ALTO);
        
        PlayerPrefs.Save();
        Debug.Log("Todos los logros y progreso (estrellas/nivel alto) reseteados.");
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
    
    public int GetTotalLevelsWithThreeStars()
    {
        int count = 0;
        int maxLevel = GetHighestLevel();
        
        for (int i = 1; i < maxLevel; i++)
        {
            if (GetStarsForLevel(i) == 3)
            {
                count++;
            }
        }
        return count;
    }
}