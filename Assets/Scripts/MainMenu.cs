using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic; 
using UnityEngine.InputSystem;

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
    public List<GameObject> checkmarksLogros;

    [Header("Botones de Niveles")]
    public List<LevelButton> botonesNivel; 
    public GameObject starPrefab;
    private const string PLAYER_PREF_ID_JUGADOR = "IDJugador";

    [Header("Paneles Ajustes")]
    public GameObject panelAjustes;
    public Slider sliderVolumen;
    public Toggle togglePantallaCompleta;
    private const string PREF_VOLUMEN = "VolumenAudio";
    private const string PREF_PANTALLA_COMPLETA = "PantallaCompleta";

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip musicaMenu;
    public AudioClip sonidoClickBoton;
    public AudioClip sonidoBotonAtras;
    
    void Start()
    {
        if (panelLogros != null) panelLogros.SetActive(false);
        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);

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

        if (sliderVolumen != null)
        {
            float volumenGuardado = PlayerPrefs.GetFloat(PREF_VOLUMEN, 0.75f);
            sliderVolumen.value = volumenGuardado;
            AudioListener.volume = volumenGuardado;
        }

        if (togglePantallaCompleta != null)
        {
            bool pantallaCompletaGuardada = PlayerPrefs.GetInt(PREF_PANTALLA_COMPLETA, 1) == 1;
            togglePantallaCompleta.isOn = pantallaCompletaGuardada;
            Screen.fullScreen = pantallaCompletaGuardada;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && musicaMenu != null && !audioSource.isPlaying)
        {
            audioSource.clip = musicaMenu;
            audioSource.loop = true;
            audioSource.Play();
        }
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
        TransitionManager.Instance.LoadSceneWithFade("GameLoop"); 
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
        TransitionManager.Instance.LoadSceneWithFade("GameLoop");
    }

    public void Salir()
    {
        TransitionManager.Instance.QuitGame();
    }

    public void AbrirLogros()
    {
        if (AchievementManager.Instance != null)
        {
            if (checkmarksLogros.Count > 0 && checkmarksLogros[0] != null)
                checkmarksLogros[0].SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_PENSADOR_LOGICO));
            
            if (checkmarksLogros.Count > 1 && checkmarksLogros[1] != null)
                checkmarksLogros[1].SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_MAESTRO_BUCLE));

            if (checkmarksLogros.Count > 2 && checkmarksLogros[2] != null)
                checkmarksLogros[2].SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_ARQUITECTO));

            if (checkmarksLogros.Count > 3 && checkmarksLogros[3] != null)
                checkmarksLogros[3].SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_SPEEDRUNNER));
            
            if (checkmarksLogros.Count > 4 && checkmarksLogros[4] != null)
                checkmarksLogros[4].SetActive(AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_OPTIMIZADOR));
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

        for (int i = 0; i < botonesNivel.Count; i++)
        {
            if (botonesNivel[i] != null)
            {
                int levelNum = i + 1;
                bool isLocked = levelNum > nivelMasAlto;
                int stars = 0;
                
                if (AchievementManager.Instance != null)
                {
                    stars = AchievementManager.Instance.GetStarsForLevel(levelNum);
                }
                
                botonesNivel[i].Setup(levelNum, stars, isLocked, starPrefab);
            }
        }

        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false); 
    }

    public void CerrarSelectorNiveles()
    {
        if (panelSelectorNiveles != null) panelSelectorNiveles.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame && Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftShiftKey.isPressed)
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ResetAllAchievements();
            }

            PlayerPrefs.DeleteKey("IDJugador");
            PlayerPrefs.Save();
            
            Debug.LogWarning("PROGRESO BORRADO (PLAYERPREFS) CON F12");
            TransitionManager.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
    }
    
    public void AbrirAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
    }

    public void CerrarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }

    public void SetVolumen(float volumen)
    {
        AudioListener.volume = volumen;
        PlayerPrefs.SetFloat(PREF_VOLUMEN, volumen);
        PlayerPrefs.Save();
    }

    public void SetPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        PlayerPrefs.SetInt(PREF_PANTALLA_COMPLETA, esCompleta ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlayClickSound()
    {
        if (audioSource != null && sonidoClickBoton != null)
        {
            audioSource.PlayOneShot(sonidoClickBoton);
        }
    }
    public void PlayBackSound()
    {
        if (audioSource != null && sonidoBotonAtras != null)
        {
            audioSource.PlayOneShot(sonidoBotonAtras);
        }
    }
}