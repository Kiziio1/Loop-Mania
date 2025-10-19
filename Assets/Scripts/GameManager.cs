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
    

    void Start()
    {
        if (flechaEjecucion != null)
        {
            colorOriginalFlecha = flechaEjecucion.color;
            OcultarFlecha(); 
        }

        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }

        nivelActualIndex = AchievementManager.NivelACargar - 1;

        if (nivelActualIndex < 0 || nivelActualIndex >= secuenciaDeNivel.Count)
        {
            Debug.LogWarning("Nivel a cargar inválido, cargando nivel 1 por defecto.");
            nivelActualIndex = 0;
            AchievementManager.NivelACargar = 1; 
        }

        if (secuenciaDeNivel != null && secuenciaDeNivel.Count > 0)
        {
            // Carga el nivel seleccionado en lugar de siempre [0]
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex]));
        }
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return; 

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            EjecutarPrograma();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            ReiniciarNivelActual();
        }
    }

    public void ReiniciarNivelActual()
    {
        StopAllCoroutines(); 
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }

        if (nivelActualIndex < secuenciaDeNivel.Count)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[nivelActualIndex]));
        }
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

    private IEnumerator CargarNivelCoroutine(LevelData nivelACargar)
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
        if (panelPrograma != null)
        {
            foreach (Transform bloque in panelPrograma)
            {
                Destroy(bloque.gameObject);
            }
        }

        if (botonBucleFor != null) { botonBucleFor.SetActive(nivelACargar.permitirBucleFor); }
        if (botonCondicionalIf != null) { botonCondicionalIf.SetActive(nivelACargar.permitirCondicionalIf); }

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

        Vector3 posPlayer = new Vector3(origenDelMapa.x + nivelACargar.posicionInicioPlayer.x, origenDelMapa.y - nivelACargar.posicionInicioPlayer.y, 0);
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
                playerController.ForzarFallo();
                SetFlechaEstadoFallo(); 
                gano = false; 
            }
            else
            {
                gano = true; 
            }
        }
        
        // Llama al LogManager con el resultado final del intento
        RegistrarIntentoLog(gano);
        
        // **AQUÍ ESTÁ EL CAMBIO**
        // Si el intento fue exitoso, AHORA mostramos los resultados
        if (gano)
        {
            MostrarResultadosNivel();
        }

        estaEjecutando = false; 
        activeAutoScrollCoroutine = null;
    }

    public void SetBloqueActual(Transform bloqueTransform)
    {
        if (flechaEjecucion == null) return;

        flechaEjecucion.gameObject.SetActive(true);
        flechaEjecucion.transform.position = new Vector3(
            bloqueTransform.position.x - 100f, 
            bloqueTransform.position.y,
            bloqueTransform.position.z
        );
        
        flechaEjecucion.transform.SetAsLastSibling();
        ultimoBloqueEjecutado = bloqueTransform;

        // --- INICIO DEL AUTO-SCROLL (ARREGLADO) ---
        if (panelScrollRect != null)
        {
            // **AQUÍ ESTÁ EL ARREGLO**
            // Detiene la corutina anterior SI existe
            if (activeAutoScrollCoroutine != null)
            {
                StopCoroutine(activeAutoScrollCoroutine);
            }
            // Inicia la nueva corutina y GUARDA la referencia
            activeAutoScrollCoroutine = StartCoroutine(AutoScrollCoroutine(bloqueTransform.GetComponent<RectTransform>()));
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
        // ELIMINAMOS StopAllCoroutines(); 
        estaEjecutando = false; 
        OcultarFlecha();

        LevelData nivelActual = secuenciaDeNivel[nivelActualIndex];
        int optimo = nivelActual.movimientosOptimos;

        int movimientosJugador = 0;
        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            if (comando != null)
            {
                movimientosJugador += comando.GetBlockCount();
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
        }

        if (panelResultados != null)
        {
            panelResultados.SetActive(true);
        }
    }

    // Este método lo llamará el botón "Siguiente Nivel"
    public void BotonSiguienteNivelClick()
    {
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }
        CargarSiguienteNivel();
    }

    private IEnumerator AutoScrollCoroutine(RectTransform targetBlock)
    {
        // Actualiza el layout para tener las posiciones correctas
        Canvas.ForceUpdateCanvases();

        // Altura total del contenido (panelPrograma)
        float contentHeight = panelPrograma.rect.height;
        // Altura del área visible (el viewport)
        float viewportHeight = panelScrollRect.viewport.rect.height;

        // Si el contenido cabe en la pantalla, no hay nada que scrollear
        if (contentHeight <= viewportHeight)
        {
            yield break;
        }

        // --- Calcular la posición ---
        // El 'anchoredPosition.y' de un bloque es negativo (ej. -500)
        // Lo convertimos a un valor positivo (500)
        float blockYPos = Mathf.Abs(targetBlock.anchoredPosition.y);

        // El punto máximo que el scroll puede alcanzar
        float maxScrollableY = contentHeight - viewportHeight;

        // Queremos centrar el bloque en la pantalla.
        // Calculamos dónde debe estar el borde superior del contenido.
        // **AQUÍ ESTABA EL TYPO (decía 'blockY' en lugar de 'blockYPos')**
        float targetContentY = blockYPos - (viewportHeight / 2.0f);

        // Aseguramos que no se pase de los límites (arriba o abajo)
        targetContentY = Mathf.Clamp(targetContentY, 0, maxScrollableY);

        // Convertimos esa posición Y a un valor "normalizado" (0.0 = abajo, 1.0 = arriba)
        float targetNormalizedY = 1.0f - (targetContentY / maxScrollableY);

        // --- Animar el scroll ---
        float duration = 0.25f; // Duración de la animación (en segundos)
        float timer = 0;
        float startNormalizedY = panelScrollRect.verticalNormalizedPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Interpola suavemente desde la posición actual a la objetivo
            float newNormalizedY = Mathf.Lerp(startNormalizedY, targetNormalizedY, timer / duration);
            panelScrollRect.verticalNormalizedPosition = newNormalizedY;
            yield return null;
        }

        // Asegura que termine en la posición exacta
        panelScrollRect.verticalNormalizedPosition = targetNormalizedY;
    }
    
    // Reúne los datos y llama al LogManager
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

        // **AQUÍ ESTÁ EL CAMBIO**
        // Calculamos el tiempo transcurrido desde que se cargó el nivel
        float tiempo = Time.time - tiempoInicioNivel;

        LogManager.Instance.LogAttempt(
            nivel,
            sbDisponibles.ToString(),
            sbSecuencia.ToString(),
            ganoIntento,
            tiempo
        );
    }
}