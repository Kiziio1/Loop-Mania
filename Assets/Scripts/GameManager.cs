using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Transform panelPrograma;
    public float tiempoDeEsperaEntrePasos = 0.5f;

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
    
    [Header("Referencias a Botones de UI")]
    public GameObject botonBucleFor;
    public GameObject botonCondicionalIf;

    private Transform contenedorMapa;
    private PlayerController playerController;

    void Start()
    {
        if (secuenciaDeNivel != null && secuenciaDeNivel.Count > 0)
        {
            StartCoroutine(CargarNivelCoroutine(secuenciaDeNivel[0]));
        }
    }

    public void ReiniciarNivelActual()
    {
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
    }

    public void EjecutarPrograma()
    {
        if (playerController != null && playerController.gameObject.activeInHierarchy)
        {
            StartCoroutine(EjecutarBloquesCoroutine());
        }
    }

    private IEnumerator EjecutarBloquesCoroutine()
    {
        Debug.Log("Iniciando ejecución...");
        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            if (comando != null)
            {
                yield return StartCoroutine(comando.Ejecutar(playerController, tiempoDeEsperaEntrePasos));
            }
        }
        Debug.Log("¡Programa finalizado!");
    }
}