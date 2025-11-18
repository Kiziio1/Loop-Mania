using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class GameManager : MonoBehaviour
{
    public RectTransform panelPrograma;
    public GameObject panelHechizos;
    public CanvasGroup hudCanvasGroup;
    private bool hudTransparente = false;

    [Header("Configuración de Niveles")]
    public List<LevelData> secuenciaDeNivel;
    private int nivelActualIndex = 0;

    [Header("Coordenadas del Mapa")]
    public Vector2 origenDelMapa = new Vector2(-8.36f, 4.48f);

    [Header("Referencias de Prefabs")]
    public GameObject prefabPiso;
    public GameObject prefabObstaculo;
    public GameObject prefabMeta;
    public GameObject prefabPlayer;
    public GameObject prefabLlave;
    public GameObject prefabPuerta;
    public GameObject prefabAbismo;
    public GameObject botonBucleFor;
    public GameObject botonCondicionalIf;
    public Slider sliderVelocidad;
    public Image flechaEjecucion;
    public GameObject panelResultados;
    public ScrollRect panelScrollRect;
    
    [Header("UI Panel Resultados")]
    public TextMeshProUGUI textoMovimientosJugador;
    public TextMeshProUGUI textoMovimientosOptimos;
    public GameObject estrella1;
    public GameObject estrella2;
    public GameObject estrella3; 
    private Transform ultimoBloqueEjecutado;
    private Color colorOriginalFlecha;
    private Transform contenedorMapa;
    private PlayerController playerController;
    private bool estaEjecutando = false;
    private Coroutine activeAutoScrollCoroutine;
    private float tiempoInicioNivel;

    [Header("Tutorial Secuencial")]
    public GameObject panelTutorialMaestro;
    public TextMeshProUGUI textoTutorial;
    public Button botonTutorialSiguiente;
    private List<string> tutorialActivo;
    private int pasoTutorialActual;
    private List<string> tutorialNivel1 = new List<string> {
        "Ya veo que lograste entrar a la torre sin mayores problemas, no pierdas el tiempo.",
        "No puedo arriesgarme a que falles ahora, así que te hare un pequeño repaso de lo basico.",
        "Cuentas con un grimorio magico, el cual te permite realizar hechizos.",
        "A tu izquierda está tu Grimorio. A la derecha, tu Página de Hechizos.",
        "Pulsa (TAB) para abrir y cerrar tu Página de Hechizos. Allí encontrarás tus hechizos básicos y un Sello de Anulación (X) para borrar.",
        "Arrastra hechizos de la Página al Grimorio para construir tu secuencia.",
        "Pulsa (Espacio) o el boton reproducir para ejecutar tu magia. El Slider de abajo controla la velocidad a la que se realizn los hechizos.",
        "Si fallas o si lo deseas, pulsa (R) para reiniciar el nivel, puedes oprimir (T) para hacer un reinicio que mantenga los hechizos que haz usado. Ahora, alcanza la meta."
    };
    private List<string> tutorialNivel6 = new List<string> {
    "¡Buen trabajo! Veo que dominas lo básico.",
    "Ahora te enseñaré un hechizo más poderoso: el Bucle For.",
    "Este hechizo te permite repetir una secuencia de hechizos varias veces.",
    "Arrastra los hechizos que quieras repetir *dentro* del bloque BucleFor.",
    "Úsalo sabiamente para ahorrar hechizos."
    };
    private List<string> tutorialNivel11 = new List<string> {
    "Impresionante. Has llegado lejos.",
    "Es hora del último hechizo: el Condicional If.",
    "Este hechizo observa si hay un obstáculo enfrente. Si lo hay, ejecutará los hechizos que tenga dentro.",
    "Si no hay obstáculo, simplemente no hara nada.",
    "Esto puedes combinarlo con todos los anteriores y crear combinaciones, las necesitaras para los niveles que se aproximan. Buena suerte."
    };
    
    [Header("Audio de Nivel")]
    public AudioClip musicaPensando;
    public AudioClip musicaEjecutando;
    public AudioClip musicaVictoria;
    private AudioSource audioSource;

    [System.Obsolete]
    void Start()
    {
        if (panelHechizos != null)
        {
            panelHechizos.SetActive(false);
        }

        if (flechaEjecucion != null)
        {
            colorOriginalFlecha = flechaEjecucion.color;
            OcultarFlecha();
        }

        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        nivelActualIndex = AchievementManager.NivelACargar - 1;

        if (nivelActualIndex < 0 || nivelActualIndex >= secuenciaDeNivel.Count)
        {
            Debug.LogWarning("Nivel a cargar inválido, cargando nivel 1 por defecto.");
            nivelActualIndex = 0;
            AchievementManager.NivelACargar = 1;
        }

        panelTutorialMaestro.SetActive(false);

        if (botonTutorialSiguiente != null) botonTutorialSiguiente.onClick.AddListener(MostrarSiguientePasoTutorial);
        
        if (secuenciaDeNivel != null && secuenciaDeNivel.Count > 0)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex], true));
        }
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadSceneWithFade("MainMenu");
            }
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            EjecutarPrograma();
        }

        if (keyboard.tKey.wasPressedThisFrame)
        {
            ReiniciarIntento();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            ReiniciarNivelActual();
        }
        
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (panelHechizos != null && !estaEjecutando)
            {
                panelHechizos.SetActive(!panelHechizos.activeSelf);
            }
        }
        
        if (keyboard.hKey.wasPressedThisFrame)
        {
            hudTransparente = !hudTransparente;
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = hudTransparente ? 0.4f : 1.0f;
            }
        }
    }

    public void ReiniciarNivelActual()
    {
        TransitionManager.Instance.LoadSceneWithFade("GameLoop");
    }

    public void CargarSiguienteNivel()
    {
        nivelActualIndex++;
        if (nivelActualIndex < secuenciaDeNivel.Count)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex]));
        }
        else
        {
            Debug.Log("¡FELICIDADES! ¡HAS COMPLETADO TODOS LOS NIVELES!");
        }
    }

    public void ReiniciarIntento()
    {
        StopAllCoroutines();
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }

        if (nivelActualIndex < secuenciaDeNivel.Count)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex], false));
        }
    }

    private IEnumerator CargarNivelCoroutine(LevelData nivelACargar, bool limpiarPanel = true)
    {
        estaEjecutando = false;
        activeAutoScrollCoroutine = null;

        if (playerController != null)
        {
            Destroy(playerController.gameObject);
        }

        if (contenedorMapa != null)
        {
            Destroy(contenedorMapa.gameObject);
        }

        if (limpiarPanel && panelPrograma != null)
        {
            foreach (Transform bloque in panelPrograma)
            {
                Destroy(bloque.gameObject);
            }
        }

        if (botonBucleFor != null) { botonBucleFor.SetActive(nivelACargar.permitirBucleFor); }
        if (botonCondicionalIf != null) { botonCondicionalIf.SetActive(nivelACargar.permitirCondicionalIf); }

        if (audioSource != null && musicaPensando != null)
        {
            audioSource.Stop();
            audioSource.clip = musicaPensando;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (panelTutorialMaestro != null)
        {
            if (nivelActualIndex == 0 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_1_VISTO))
            {
                IniciarTutorial(tutorialNivel1);
            }
            else if (nivelActualIndex == 5 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_6_VISTO))
            {
                IniciarTutorial(tutorialNivel6);
            }
            else if (nivelActualIndex == 10 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_11_VISTO))
            {
                IniciarTutorial(tutorialNivel11);
            }
            else
            {
                panelTutorialMaestro.SetActive(false);
            }
        }

        yield return null;

        contenedorMapa = new GameObject("ContenedorMapa").transform;
        for (int y = 0; y < nivelACargar.alto; y++)
        {
            for (int x = 0; x < nivelACargar.ancho; x++)
            {
                Vector3 posicion = new Vector3(origenDelMapa.x + x, origenDelMapa.y - y, 0);
                TileType tipoDeTile = nivelACargar.gridLayout[y * nivelACargar.ancho + x];
                switch (tipoDeTile)
                {
                    case TileType.Piso: Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa); break;
                    case TileType.Obstaculo: Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa); Instantiate(prefabObstaculo, posicion, Quaternion.identity, contenedorMapa); break;
                    case TileType.Meta: Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa); Instantiate(prefabMeta, posicion, Quaternion.identity, contenedorMapa); break;
                    case TileType.Llave: Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa); Instantiate(prefabLlave, posicion, Quaternion.identity, contenedorMapa); break;
                    case TileType.Puerta: Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa); Instantiate(prefabPuerta, posicion, Quaternion.identity, contenedorMapa); break;
                    case TileType.Vacio: Instantiate(prefabAbismo, posicion, Quaternion.identity, contenedorMapa); break;
                }
            }
        }

        Vector3 posPlayer = new Vector3(
            origenDelMapa.x + nivelACargar.posicionInicioPlayer.x,
            origenDelMapa.y - nivelACargar.posicionInicioPlayer.y + 0.25f,
            0
        );
        GameObject playerInstance = Instantiate(prefabPlayer, posPlayer, Quaternion.identity);
        playerController = playerInstance.GetComponent<PlayerController>();

        Debug.Log("Cargado Nivel: " + nivelACargar.nombreNivel);

        if (sliderVelocidad != null)
        {
            sliderVelocidad.value = 0.5f;
        }

        OcultarFlecha();
        tiempoInicioNivel = Time.time;
    }

    public void EjecutarPrograma()
    {
        if (panelHechizos != null && panelHechizos.activeSelf)
        {
            panelHechizos.SetActive(false);
        }

        if (estaEjecutando)
        {
            Debug.Log("Ya hay una ejecución en curso.");
            return;
        }

        if (playerController == null || !playerController.estaVivo)
        {
            Debug.Log("No se puede ejecutar: el jugador no está listo o ya ha perdido.");
            return;
        }

        if (audioSource != null && musicaEjecutando != null)
        {
            audioSource.Stop();
            audioSource.clip = musicaEjecutando;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (playerController != null && playerController.gameObject.activeInHierarchy)
        {
            StartCoroutine(EjecutarBloquesCoroutine());
        }
    }

    private IEnumerator EjecutarBloquesCoroutine()
    {
        Debug.Log("Iniciando ejecución...");
        estaEjecutando = true;
        bool gano = false;

        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            if (comando != null)
            {
                yield return StartCoroutine(comando.Ejecutar(this, playerController));
                if (playerController == null || !playerController.estaVivo)
                {
                    gano = false;
                    break;
                }
            }
        }

        Debug.Log("¡Programa finalizado!");

        if (playerController != null && playerController.estaVivo)
        {
            if (!playerController.estaEnLaMeta)
            {
                SetFlechaEstadoFallo();
                playerController.ForzarFallo();
                gano = false;
            }
            else
            {
                gano = true;
            }
        }

        RegistrarIntentoLog(gano);

        if (gano)
        {
            MostrarResultadosNivel();
        }
        else if (audioSource != null && musicaPensando != null)
        {
            audioSource.Stop();
            
        }

        estaEjecutando = false;
        activeAutoScrollCoroutine = null;
    }

    public void SetBloqueActual(Transform bloqueTransform)
    {
        if (flechaEjecucion == null) return;

        RectTransform viewportRect = panelScrollRect.viewport;
        RectTransform blockRect = bloqueTransform.GetComponent<RectTransform>();

        Vector3[] blockCorners = new Vector3[4];
        blockRect.GetWorldCorners(blockCorners);
        float blockTopY = blockCorners[1].y;
        float blockBottomY = blockCorners[0].y;

        Vector3[] viewportCorners = new Vector3[4];
        viewportRect.GetWorldCorners(viewportCorners);
        float viewportTopY = viewportCorners[1].y;
        float viewportBottomY = viewportCorners[0].y;

        bool isVisible = (blockTopY < viewportTopY) && (blockBottomY > viewportBottomY);

        if (!isVisible)
        {
            flechaEjecucion.gameObject.SetActive(false);
        }
        else
        {
            flechaEjecucion.gameObject.SetActive(true);
            flechaEjecucion.transform.position = new Vector3(
                bloqueTransform.position.x - 170f,
                bloqueTransform.position.y,
                bloqueTransform.position.z
            );
            flechaEjecucion.transform.SetAsLastSibling();
        }
        
        ultimoBloqueEjecutado = bloqueTransform;

        if (panelScrollRect != null)
        {
            if (activeAutoScrollCoroutine != null)
            {
                StopCoroutine(activeAutoScrollCoroutine);
            }
            activeAutoScrollCoroutine = StartCoroutine(AutoScrollCoroutine(blockRect));
        }
    }
    public void OcultarFlecha()
    {
        if (flechaEjecucion == null) return;

        flechaEjecucion.gameObject.SetActive(false);
        flechaEjecucion.color = colorOriginalFlecha; 
        ultimoBloqueEjecutado = null;
    }

    public void SetFlechaEstadoFallo()
    {
        if (flechaEjecucion == null || ultimoBloqueEjecutado == null) return;

        SetBloqueActual(ultimoBloqueEjecutado);

        flechaEjecucion.color = Color.red;
    }

    public void MostrarResultadosNivel()
    {
        estaEjecutando = false;
        OcultarFlecha();

        if (audioSource != null && musicaVictoria != null)
        {
            audioSource.Stop();
            audioSource.clip = musicaVictoria;
            audioSource.loop = false;
            audioSource.Play();
        }

        LevelData nivelActual = secuenciaDeNivel[nivelActualIndex];
        int optimo = nivelActual.movimientosOptimos;
        int movimientosJugador = 0;

        bool usoBucleFor = false;
        bool usoCondicionalSi = false;

        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            if (comando != null)
            {
                movimientosJugador += comando.GetBlockCount();
                
                if (comando.tipoDeComando == BloqueDeComando.TipoComando.BucleFor)
                {
                    usoBucleFor = true;
                }
                if (comando.tipoDeComando == BloqueDeComando.TipoComando.CondicionalSi)
                {
                    usoCondicionalSi = true;
                }
            }
        }

        int limite3Estrellas = optimo;
        int limite2Estrellas = Mathf.CeilToInt(optimo + (optimo * 0.25f) + 3);
        int estrellasGanadas = 0;

        if (movimientosJugador <= limite3Estrellas)
        {
            estrellasGanadas = 3;
        }
        else if (movimientosJugador <= limite2Estrellas)
        {
            estrellasGanadas = 2;
        }
        else
        {
            estrellasGanadas = 1;
        }

        if (textoMovimientosJugador != null)
            textoMovimientosJugador.text = "Tus Movimientos: " + movimientosJugador;
        if (textoMovimientosOptimos != null)
            textoMovimientosOptimos.text = "Óptimo: " + optimo;

        if (estrella1 != null) estrella1.SetActive(estrellasGanadas >= 1);
        if (estrella2 != null) estrella2.SetActive(estrellasGanadas >= 2);
        if (estrella3 != null) estrella3.SetActive(estrellasGanadas == 3);

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.SetStarsForLevel(nivelActualIndex + 1, estrellasGanadas);
            int nivelDesbloqueado = nivelActualIndex + 2;
            AchievementManager.Instance.SetHighestLevel(nivelDesbloqueado);
            
            CheckearLogros(nivelActualIndex + 1, estrellasGanadas, usoBucleFor, usoCondicionalSi);
        }

        if (panelResultados != null)
        {
            panelResultados.SetActive(true);
        }
    }

    public void BotonSiguienteNivelClick()
    {
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }

        nivelActualIndex++; 
        
        if (nivelActualIndex < secuenciaDeNivel.Count)
        {
            AchievementManager.NivelACargar = nivelActualIndex + 1;
            TransitionManager.Instance.LoadSceneWithFade("GameLoop");
        }
        else
        {
            Debug.Log("¡FELICIDADES! ¡HAS COMPLETADO TODOS LOS NIVELES!");
            TransitionManager.Instance.LoadSceneWithFade("MainMenu");
        }
    }

    private IEnumerator AutoScrollCoroutine(RectTransform targetBlock)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = panelPrograma.rect.height;
        float viewportHeight = panelScrollRect.viewport.rect.height;

        if (contentHeight <= viewportHeight)
        {
            yield break;
        }

        float blockYPos = Mathf.Abs(targetBlock.anchoredPosition.y);

        float maxScrollableY = contentHeight - viewportHeight;

        float targetContentY = blockYPos - (viewportHeight / 2.0f);

        targetContentY = Mathf.Clamp(targetContentY, 0, maxScrollableY);

        float targetNormalizedY = 1.0f - (targetContentY / maxScrollableY);

        float duration = 0.25f; 
        float timer = 0;
        float startNormalizedY = panelScrollRect.verticalNormalizedPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newNormalizedY = Mathf.Lerp(startNormalizedY, targetNormalizedY, timer / duration);
            panelScrollRect.verticalNormalizedPosition = newNormalizedY;
            yield return null;
        }

        panelScrollRect.verticalNormalizedPosition = targetNormalizedY;
    }
    private void RegistrarIntentoLog(bool ganoIntento)
    {
        if (LogManager.Instance == null) return;

        LevelData nivelActual = secuenciaDeNivel[nivelActualIndex];
        string nivel = nivelActual.nombreNivel;

        StringBuilder sbDisponibles = new StringBuilder("Avanzar, GirarDerecha, GirarIzquierda");
        if (nivelActual.permitirBucleFor) { sbDisponibles.Append(", BucleFor"); }
        if (nivelActual.permitirCondicionalIf) { sbDisponibles.Append(", CondicionalSi"); }

        StringBuilder sbSecuencia = new StringBuilder();
        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            if (comando != null)
            {
                sbSecuencia.Append(comando.GetSequenceString()).Append(", ");
            }
        }
        float tiempo = Time.time - tiempoInicioNivel;

        LogManager.Instance.LogAttempt(
            nivel,
            sbDisponibles.ToString(),
            sbSecuencia.ToString(),
            ganoIntento,
            tiempo
        );
    }

    private void IniciarTutorial(List<string> pasos)
    {
        if (panelTutorialMaestro == null || textoTutorial == null) return;

        if (pasos == tutorialNivel1)
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_1_VISTO);
        }
        else if (pasos == tutorialNivel6)
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_6_VISTO);
        }
        else if (pasos == tutorialNivel11)
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_11_VISTO);
        }

        panelTutorialMaestro.SetActive(true);
        textoTutorial.gameObject.SetActive(true);

        tutorialActivo = pasos;
        pasoTutorialActual = 0;
        MostrarSiguientePasoTutorial();
    }

    public void MostrarSiguientePasoTutorial()
    {
        if (pasoTutorialActual >= tutorialActivo.Count)
        {
            panelTutorialMaestro.SetActive(false);
            return;
        }

        string paso = tutorialActivo[pasoTutorialActual];
        textoTutorial.gameObject.SetActive(true);
        textoTutorial.text = paso;
        pasoTutorialActual++;
    }

    private void CheckearLogros(int nivelActualID, int estrellasGanadas, bool usoBucleFor, bool usoCondicionalSi)
    {
        if (AchievementManager.Instance == null) return;

        float tiempoTotal = Time.time - tiempoInicioNivel;

        if (tiempoTotal < 10f && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_SPEEDRUNNER))
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_SPEEDRUNNER);
            Debug.Log("LOGRO DESBLOQUEADO: Speedrunner");
        }

        if (usoBucleFor && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_MAESTRO_BUCLE))
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_MAESTRO_BUCLE);
            Debug.Log("LOGRO DESBLOQUEADO: Maestro del Bucle");
        }

        if (usoCondicionalSi && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_ARQUITECTO))
        {
            AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_ARQUITECTO);
            Debug.Log("LOGRO DESBLOQUEADO: Arquitecto de Decisiones");
        }
        
        if (nivelActualID == 5)
        {
            bool nivel1 = AchievementManager.Instance.GetStarsForLevel(1) > 0;
            bool nivel2 = AchievementManager.Instance.GetStarsForLevel(2) > 0;
            bool nivel3 = AchievementManager.Instance.GetStarsForLevel(3) > 0;
            bool nivel4 = AchievementManager.Instance.GetStarsForLevel(4) > 0;
            bool nivel5 = AchievementManager.Instance.GetStarsForLevel(5) > 0;

            if (nivel1 && nivel2 && nivel3 && nivel4 && nivel5 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_PENSADOR_LOGICO))
            {
                AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_PENSADOR_LOGICO);
                Debug.Log("LOGRO DESBLOQUEADO: Pensador Lógico");
            }
        }

        if (estrellasGanadas == 3)
        {
            int total3Estrellas = AchievementManager.Instance.GetTotalLevelsWithThreeStars();
            if (total3Estrellas >= 10 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.LOGRO_OPTIMIZADOR))
            {
                AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_OPTIMIZADOR);
                Debug.Log("LOGRO DESBLOQUEADO: Optimizador");
            }
        }
    }
}