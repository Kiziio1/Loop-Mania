using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

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

    // =================================================================================
    // SISTEMA DE TUTORIALES (HÍBRIDO)
    // =================================================================================
    [Header("Tutorial UI General")]
    public GameObject panelTutorialMaestro;
    public GameObject imagenAvatarBruja;
    public TextMeshProUGUI textoTutorial;
    public Button botonTutorialSiguiente; 

    // --- Estructuras para el NUEVO tutorial interactivo (Nivel 1) ---
    [System.Serializable]
    public struct PasoTutorialData
    {
        [TextArea(2,3)] public string textoKael; 
        public RectTransform objetoAResaltar;    
        public TipoAccionTutorial accionRequerida; 
        
        [Header("Opciones Especiales")]
        public bool resaltarSoloUnSegundo; 
        public bool ocultarTextoParaAccion; 
    }

    public enum TipoAccionTutorial
    {
        ClickParaAvanzar, 
        PresionarTab,     
        ArrastrarHechizos, 
        Ejecutar,         
        FalloForzado,      
        PresionarT,
        Nada               
    }

    [Header("Nuevo Sistema Tutorial (Nivel 1)")]
    public GameObject panelOscuroTutorial; 
    public List<PasoTutorialData> tutorialNivel1_Pasos; 
    private int pasoInteractivoIndex = 0;
    private int pasoGuardadoParaReinicio = -1; 
    private PasoTutorialData pasoActualData;
    private Canvas canvasResaltadoActual; 
    private bool esTutorialInteractivo = false; 
    private bool esperandoInicioAccion = false; 

    // --- Listas para el VIEJO tutorial (Nivel 6 y 11) ---
    private List<string> tutorialActivoTexto; 
    private int pasoTextoIndex;
    
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
        "Esto puedes combinarlo con todos los anteriores y crear combinaciones."
    };
    
    [Header("Audio de Nivel")]
    public AudioClip musicaPensando;
    public AudioClip musicaEjecutando;
    public AudioClip musicaVictoria;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    [System.Obsolete]
    void Start()
    {
        if (panelHechizos != null) panelHechizos.SetActive(false);

        if (panelOscuroTutorial != null) panelOscuroTutorial.SetActive(false);

        if (flechaEjecucion != null)
        {
            colorOriginalFlecha = flechaEjecucion.color;
            OcultarFlecha();
        }

        if (panelResultados != null) panelResultados.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        nivelActualIndex = AchievementManager.NivelACargar - 1;

        if (nivelActualIndex < 0 || nivelActualIndex >= secuenciaDeNivel.Count)
        {
            Debug.LogWarning("Nivel a cargar inválido, cargando nivel 1 por defecto.");
            nivelActualIndex = 0;
            AchievementManager.NivelACargar = 1;
        }

        panelTutorialMaestro.SetActive(false);

        if (botonTutorialSiguiente != null) 
        {
            botonTutorialSiguiente.onClick.RemoveAllListeners();
            botonTutorialSiguiente.onClick.AddListener(ClickBotonTutorial);
        }
        
        if (secuenciaDeNivel != null && secuenciaDeNivel.Count > 0)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex], true));
        }
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // TUTORIAL INTERACTIVO
        if (esTutorialInteractivo)
        {
            if (esperandoInicioAccion) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                 if (TransitionManager.Instance != null) TransitionManager.Instance.LoadSceneWithFade("MainMenu");
            }

            switch (pasoActualData.accionRequerida)
            {
                case TipoAccionTutorial.PresionarTab:
                    if (keyboard.tabKey.wasPressedThisFrame)
                    {
                        if (panelHechizos != null) panelHechizos.SetActive(!panelHechizos.activeSelf);
                        AvanzarPasoInteractivo();
                    }
                    return; 

                case TipoAccionTutorial.PresionarT:
                    if (keyboard.tKey.wasPressedThisFrame)
                    {
                        pasoGuardadoParaReinicio = pasoInteractivoIndex + 1;

                        ReiniciarIntento();

                    }
                    return;

                case TipoAccionTutorial.Ejecutar:
                    if (keyboard.spaceKey.wasPressedThisFrame && !estaEjecutando)
                    {
                         EjecutarPrograma();
                    }
                    return;

                case TipoAccionTutorial.ArrastrarHechizos:
                    if (keyboard.spaceKey.wasPressedThisFrame) return;

                    if (GetBloquesColocadosCount() >= 3) 
                    {
                        AvanzarPasoInteractivo();
                    }
                    return; 
                
                case TipoAccionTutorial.ClickParaAvanzar:
                    break;

                default:
                    if (pasoActualData.accionRequerida == TipoAccionTutorial.FalloForzado) return;
                    break;
            }
        }

        // INPUTS NORMALES (Juego estándar)
        if (!esTutorialInteractivo)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (TransitionManager.Instance != null) TransitionManager.Instance.LoadSceneWithFade("MainMenu");
            }

            if (keyboard.spaceKey.wasPressedThisFrame) EjecutarPrograma();
            if (keyboard.tKey.wasPressedThisFrame) ReiniciarIntento();
            if (keyboard.rKey.wasPressedThisFrame) ReiniciarNivelActual();
            
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
                if (hudCanvasGroup != null) hudCanvasGroup.alpha = hudTransparente ? 0.4f : 1.0f;
            }
        }
    }

    // --- Métodos Públicos para UI y Lógica ---

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
            TransitionManager.Instance.LoadSceneWithFade("MainMenu");
        }
    }

    public void ReiniciarIntento()
    {
        StopAllCoroutines();
        if (panelResultados != null) panelResultados.SetActive(false);

        if (nivelActualIndex < secuenciaDeNivel.Count)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex], false));
        }
    }

    // --- Corrutina de Carga (Modificada para detectar tutoriales) ---

    private IEnumerator CargarNivelCoroutine(LevelData nivelACargar, bool limpiarPanel = true)
    {
        estaEjecutando = false;
        activeAutoScrollCoroutine = null;

        if (playerController != null) Destroy(playerController.gameObject);
        if (contenedorMapa != null) Destroy(contenedorMapa.gameObject);
        if (limpiarPanel && panelPrograma != null)
        {
            foreach (Transform bloque in panelPrograma) Destroy(bloque.gameObject);
        }

        if (botonBucleFor != null) botonBucleFor.SetActive(nivelACargar.permitirBucleFor);
        if (botonCondicionalIf != null) botonCondicionalIf.SetActive(nivelACargar.permitirCondicionalIf);

        if (audioSource != null && musicaPensando != null)
        {
            audioSource.Stop();
            audioSource.clip = musicaPensando;
            audioSource.loop = true;
            audioSource.Play();
        }

        // --- SISTEMA DE TUTORIAL: SELECCIÓN ---
        esTutorialInteractivo = false; // Reset
        
        if (panelTutorialMaestro != null)
        {
            // NIVEL 1: NUEVO SISTEMA INTERACTIVO
            if (nivelActualIndex == 0 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_1_VISTO))
            {
                if (pasoGuardadoParaReinicio != -1)
                {
                    IniciarTutorialInteractivo(pasoGuardadoParaReinicio);
                    pasoGuardadoParaReinicio = -1; // 
                }
                else
                {
                    IniciarTutorialInteractivo(0);
                }
            }
            // NIVEL 6: SISTEMA VIEJO (TEXTO)
            else if (nivelActualIndex == 5 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_6_VISTO))
            {
                IniciarTutorialTexto(tutorialNivel6);
            }
            // NIVEL 11: SISTEMA VIEJO (TEXTO)
            else if (nivelActualIndex == 10 && !AchievementManager.Instance.IsAchievementUnlocked(AchievementManager.TUTORIAL_11_VISTO))
            {
                IniciarTutorialTexto(tutorialNivel11);
            }
            else
            {
                panelTutorialMaestro.SetActive(false);
                if (panelOscuroTutorial != null) panelOscuroTutorial.SetActive(false);
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

        if (sliderVelocidad != null) sliderVelocidad.value = 0.5f;

        OcultarFlecha();
        tiempoInicioNivel = Time.time;
    }

    public void EjecutarPrograma()
    {
        if (panelHechizos != null && panelHechizos.activeSelf) panelHechizos.SetActive(false);
        if (estaEjecutando) return;
        if (playerController == null || !playerController.estaVivo) return;

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

        // --- INTERVENCION TUTORIAL INTERACTIVO ---
        if (esTutorialInteractivo)
        {
            if (pasoActualData.accionRequerida == TipoAccionTutorial.Ejecutar && !gano)
            {
                yield return new WaitForSeconds(1f);
                AvanzarPasoInteractivo();
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

    // =================================================================================
    // MÉTODOS DEL NUEVO SISTEMA DE TUTORIAL (INTERACTIVO)
    // =================================================================================
    
    private void IniciarTutorialInteractivo(int pasoInicial = 0)
    {
        if (tutorialNivel1_Pasos == null || tutorialNivel1_Pasos.Count == 0) return;

        esTutorialInteractivo = true;
        pasoInteractivoIndex = pasoInicial; 
        
        panelTutorialMaestro.SetActive(true);
        panelOscuroTutorial.SetActive(true); 
        textoTutorial.gameObject.SetActive(true);
        
        ProcesarPasoInteractivo();
    }

    private void ProcesarPasoInteractivo()
    {
        if (pasoInteractivoIndex >= tutorialNivel1_Pasos.Count)
        {
            FinalizarTutorialInteractivo();
            return;
        }

        pasoActualData = tutorialNivel1_Pasos[pasoInteractivoIndex];
        esperandoInicioAccion = false; 

        panelTutorialMaestro.SetActive(true);
        textoTutorial.text = pasoActualData.textoKael;

        if (pasoActualData.ocultarTextoParaAccion)
        {
            esperandoInicioAccion = true;
            ResaltarObjeto(null); 
            if (imagenAvatarBruja != null) imagenAvatarBruja.SetActive(true); 
        }
        else
        {
            ResaltarObjeto(pasoActualData.objetoAResaltar);
            
            if (imagenAvatarBruja != null)
            {
                bool hayResaltado = pasoActualData.objetoAResaltar != null;
                imagenAvatarBruja.SetActive(!hayResaltado);
            }

            if (pasoActualData.resaltarSoloUnSegundo && pasoActualData.objetoAResaltar != null)
            {
                StartCoroutine(RutinaResaltarTemporal());
            }
        }

        if (botonTutorialSiguiente != null)
        {
            bool interactuable = (pasoActualData.accionRequerida == TipoAccionTutorial.ClickParaAvanzar) 
                                 || esperandoInicioAccion;
            
            botonTutorialSiguiente.interactable = interactuable;
            botonTutorialSiguiente.GetComponent<Image>().raycastTarget = interactuable;
        }
    }

    private void AvanzarPasoInteractivo()
    {
        pasoInteractivoIndex++;
        ProcesarPasoInteractivo();
    }

    private void FinalizarTutorialInteractivo()
    {
        esTutorialInteractivo = false;
        panelTutorialMaestro.SetActive(false);
        panelOscuroTutorial.SetActive(false);
        ResaltarObjeto(null); 
        AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_1_VISTO);
    }

    private void ResaltarObjeto(RectTransform objetivo)
    {
        if (canvasResaltadoActual != null)
        {
            GraphicRaycaster gr = canvasResaltadoActual.GetComponent<GraphicRaycaster>();
            if (gr != null) Destroy(gr);

            Destroy(canvasResaltadoActual);
            canvasResaltadoActual = null;
        }

        if (objetivo == null) return;

        Canvas c = objetivo.gameObject.GetComponent<Canvas>();
        if (c == null) c = objetivo.gameObject.AddComponent<Canvas>();
        
        c.overrideSorting = true;
        c.sortingOrder = 30; 

        if (objetivo.gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            objetivo.gameObject.AddComponent<GraphicRaycaster>();
        }

        canvasResaltadoActual = c;
    }

    public bool EstaEnTutorial()
    {
        return esTutorialInteractivo;
    }
    
    public bool PasoActualRequiereAvanzar()
    {
        return esTutorialInteractivo && pasoActualData.accionRequerida == TipoAccionTutorial.ArrastrarHechizos;
    }

    // =================================================================================
    // MÉTODOS DEL VIEJO SISTEMA DE TUTORIAL (SOLO TEXTO - NIVEL 6 y 11)
    // =================================================================================

    private void IniciarTutorialTexto(List<string> pasos)
    {
        esTutorialInteractivo = false;
        panelTutorialMaestro.SetActive(true);
        textoTutorial.gameObject.SetActive(true);
        if (panelOscuroTutorial != null) panelOscuroTutorial.SetActive(false); // Sin oscuridad
        
        tutorialActivoTexto = pasos;
        pasoTextoIndex = 0;
        MostrarSiguientePasoTexto();
    }

    public void MostrarSiguientePasoTexto()
    {
        if (pasoTextoIndex >= tutorialActivoTexto.Count)
        {
            panelTutorialMaestro.SetActive(false);
            if (nivelActualIndex == 5) AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_6_VISTO);
            if (nivelActualIndex == 10) AchievementManager.Instance.UnlockAchievement(AchievementManager.TUTORIAL_11_VISTO);
            return;
        }

        textoTutorial.text = tutorialActivoTexto[pasoTextoIndex];
        pasoTextoIndex++;
        
        if (botonTutorialSiguiente != null) botonTutorialSiguiente.interactable = true;
    }

    public void ClickBotonTutorial()
    {
        if (esTutorialInteractivo)
        {
            if (esperandoInicioAccion)
            {
                panelTutorialMaestro.SetActive(false);

                ResaltarObjeto(pasoActualData.objetoAResaltar);

                esperandoInicioAccion = false;
                
                return;
            }

            if (pasoActualData.accionRequerida == TipoAccionTutorial.ClickParaAvanzar)
            {
                AvanzarPasoInteractivo();
            }
        }
        else
        {
            MostrarSiguientePasoTexto();
        }
    }

    // =================================================================================
    // RESTO DE MÉTODOS UI Y VISUALES
    // =================================================================================

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

        if (!isVisible) flechaEjecucion.gameObject.SetActive(false);
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
            if (activeAutoScrollCoroutine != null) StopCoroutine(activeAutoScrollCoroutine);
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

        if (esTutorialInteractivo)
        {
            FinalizarTutorialInteractivo();
        }

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
                if (comando.tipoDeComando == BloqueDeComando.TipoComando.BucleFor) usoBucleFor = true;
                if (comando.tipoDeComando == BloqueDeComando.TipoComando.CondicionalSi) usoCondicionalSi = true;
            }
        }

        int estrellasGanadas;
        if (movimientosJugador <= optimo) estrellasGanadas = 3;
        else if (movimientosJugador <= Mathf.CeilToInt(optimo + (optimo * 0.25f) + 3)) estrellasGanadas = 2;
        else estrellasGanadas = 1;

        if (textoMovimientosJugador != null) textoMovimientosJugador.text = "Tus Movimientos: " + movimientosJugador;
        if (textoMovimientosOptimos != null) textoMovimientosOptimos.text = "Óptimo: " + optimo;

        if (estrella1 != null) estrella1.SetActive(estrellasGanadas >= 1);
        if (estrella2 != null) estrella2.SetActive(estrellasGanadas >= 2);
        if (estrella3 != null) estrella3.SetActive(estrellasGanadas == 3);

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.SetStarsForLevel(nivelActualIndex + 1, estrellasGanadas);
            AchievementManager.Instance.SetHighestLevel(nivelActualIndex + 2);
            CheckearLogros(nivelActualIndex + 1, estrellasGanadas, usoBucleFor, usoCondicionalSi);
        }

        if (panelResultados != null) panelResultados.SetActive(true);
    }

    public void BotonSiguienteNivelClick()
    {
        if (panelResultados != null) panelResultados.SetActive(false);

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

        if (contentHeight <= viewportHeight) yield break;

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
            panelScrollRect.verticalNormalizedPosition = Mathf.Lerp(startNormalizedY, targetNormalizedY, timer / duration);
            yield return null;
        }
        panelScrollRect.verticalNormalizedPosition = targetNormalizedY;
    }

    private void RegistrarIntentoLog(bool ganoIntento)
    {
        if (LogManager.Instance == null) return;

        LevelData nivelActual = secuenciaDeNivel[nivelActualIndex];
        StringBuilder sbDisponibles = new StringBuilder("Avanzar, GirarDerecha, GirarIzquierda");
        if (nivelActual.permitirBucleFor) sbDisponibles.Append(", BucleFor"); 
        if (nivelActual.permitirCondicionalIf) sbDisponibles.Append(", CondicionalSi"); 

        StringBuilder sbSecuencia = new StringBuilder();
        foreach (Transform bloque in panelPrograma)
        {
            BloqueDeComando cmd = bloque.GetComponent<BloqueDeComando>();
            if (cmd != null) sbSecuencia.Append(cmd.GetSequenceString()).Append(", ");
        }

        LogManager.Instance.LogAttempt(nivelActual.nombreNivel, sbDisponibles.ToString(), sbSecuencia.ToString(), ganoIntento, Time.time - tiempoInicioNivel);
    }

    private void CheckearLogros(int nivelActualID, int estrellasGanadas, bool usoBucleFor, bool usoCondicionalSi)
    {
        if (AchievementManager.Instance == null) return;
        float tiempoTotal = Time.time - tiempoInicioNivel;

        if (tiempoTotal < 10f) AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_SPEEDRUNNER);
        if (usoBucleFor) AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_MAESTRO_BUCLE);
        if (usoCondicionalSi) AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_ARQUITECTO);
        
        if (nivelActualID == 5)
        {
             bool todosNiveles = true;
             for(int i=1; i<=5; i++) if(AchievementManager.Instance.GetStarsForLevel(i) == 0) todosNiveles = false;
             
             if(todosNiveles) AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_PENSADOR_LOGICO);
        }

        if (estrellasGanadas == 3 && AchievementManager.Instance.GetTotalLevelsWithThreeStars() >= 10)
        {
             AchievementManager.Instance.UnlockAchievement(AchievementManager.LOGRO_OPTIMIZADOR);
        }
    }

    private IEnumerator RutinaResaltarTemporal()
    {
        yield return new WaitForSeconds(1.0f); 
        
        ResaltarObjeto(null);

        if (imagenAvatarBruja != null) imagenAvatarBruja.SetActive(true);
    }

    public int GetBloquesColocadosCount()
    {
        if (panelPrograma == null) return 0;
        return panelPrograma.GetComponentsInChildren<BloqueDeComando>().Length;
    }

    public bool PasoActualEsArrastrar()
    {
        return esTutorialInteractivo && pasoActualData.accionRequerida == TipoAccionTutorial.ArrastrarHechizos;
    }
}