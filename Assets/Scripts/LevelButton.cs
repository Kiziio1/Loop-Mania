using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    // Asigna esto en el inspector. Ej: 1, 2, 3...
    public int nivelIndex; 

    public void CargarNivel()
    {
        // 1. Le dice al manager qué nivel debe cargar
        AchievementManager.NivelACargar = nivelIndex;

        // 2. Carga la escena del juego
        SceneManager.LoadScene("GameLoop");
    }
}