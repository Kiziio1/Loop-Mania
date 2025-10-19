using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles Principales")]
    public GameObject panelMenuPrincipal; 
    public GameObject panelLogros;
    public GameObject panelSelectorNiveles;
    public Button botonContinuar;
    public GameObject panelLogin;
    public TMP_InputField inputFieldJugador;
    public Button botonComenzarLogin;

    [Header("UI de Logros")]
    public GameObject checkmarkNivel1; 
    public GameObject checkmarkNivel2; 
    public GameObject checkmarkNivel3; 

    [Header("Botones de Niveles")]
    public Button botonNivel1; 
    public Button botonNivel2; 
    public Button botonNivel3; 
    
    private const string PLAYER_PREF_ID_JUGADOR = "IDJugador";
    
    void Start()
    {
        if (panelLogros != null) panelLogros.SetActive(false);
        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(false);

        if (PlayerPrefs.HasKey(PLAYER_PREF_ID_JUGADOR))
        {
            string idGuardado = PlayerPrefs.GetString(PLAYER_PREF_ID_JUGADOR);
            inputFieldJugador.text = idGuardado;
            if (LogManager.Instance != null) LogManager.Instance.idJugador = idGuardado;
            
            panelLogin.SetActive(false);
            panelMenuPrincipal.SetActive(true);
            ActualizarBotonContinuar();
        }
        else
        {
            panelLogin.SetActive(true);
            panelMenuPrincipal.SetActive(false);
        }

        if (botonComenzarLogin != null)
            botonComenzarLogin.onClick.AddListener(ComenzarJuego);
        if (inputFieldJugador != null)
            inputFieldJugador.onSubmit.AddListener((value) => { ComenzarJuego(); });
    }

    public void ComenzarJuego()
    {
        string id = "Player_Default";
        if (inputFieldJugador != null && !string.IsNullOrEmpty(inputFieldJugador.text))
        {
            id = inputFieldJugador.text;
        }

        PlayerPrefs.SetString(PLAYER_PREF_ID_JUGADOR, id);
        if (LogManager.Instance != null) LogManager.Instance.idJugador = id;

        panelLogin.SetActive(false);
        panelMenuPrincipal.SetActive(true);
        ActualizarBotonContinuar();
    }

    private void GuardarIDJugador()
    {
        string id = "Player_Default";
        if (inputFieldJugador != null && !string.IsNullOrEmpty(inputFieldJugador.text))
        {
            id = inputFieldJugador.text;
        }

        if (LogManager.Instance != null)
        {
            LogManager.Instance.idJugador = id;
        }
    }
    
    private void ActualizarBotonContinuar()
    {
        if (botonContinuar != null && AchievementManager.Instance != null)
        {
            int nivelMasAlto = AchievementManager.Instance.GetHighestLevel();
            botonContinuar.interactable = (nivelMasAlto > 1);
        }
    }

    public void Jugar()
    {
        GuardarIDJugador(); 
        AchievementManager.NivelACargar = 1;
        SceneManager.LoadScene("GameLoop"); 
    }

    public void Continuar()
    {
        GuardarIDJugador(); 

        int nivelACargar = 1;
        if (AchievementManager.Instance != null)
        {
            nivelACargar = AchievementManager.Instance.GetHighestLevel();
        }
        AchievementManager.NivelACargar = nivelACargar;
        SceneManager.LoadScene("GameLoop");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void AbrirLogros()
    {
        if (AchievementManager.Instance != null)
        {
            if (checkmarkNivel1 != null) checkmarkNivel1.SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.NIVEL_1_COMPLETADO));
            if (checkmarkNivel2 != null) checkmarkNivel2.SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.NIVEL_2_COMPLETADO));
            if (checkmarkNivel3 != null) checkmarkNivel3.SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.NIVEL_3_COMPLETADO));
        }

        if (panelLogros != null) panelLogros.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false); 
    }

    public void CerrarLogros()
    {
        if (panelLogros != null) panelLogros.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true); 
    }

    public void AbrirSelectorNiveles()
    {
        int nivelMasAlto = 1;
        if (AchievementManager.Instance != null)
        {
            nivelMasAlto = AchievementManager.Instance.GetHighestLevel();
        }

        if (botonNivel1 != null) botonNivel1.interactable = (1 <= nivelMasAlto);
        if (botonNivel2 != null) botonNivel2.interactable = (2 <= nivelMasAlto);
        if (botonNivel3 != null) botonNivel3.interactable = (3 <= nivelMasAlto);

        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false); 
    }

    public void CerrarSelectorNiveles()
    {
        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true); 
    }
}