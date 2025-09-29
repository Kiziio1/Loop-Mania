using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Referencias de Prefabs")]
    public GameObject prefabPiso;
    public GameObject prefabObstaculo;
    public GameObject prefabMeta;
    public GameObject prefabPlayer;

    [Header("Contenedor de Niveles")]
    public Transform contenedorMapa; // Un objeto vacío para mantener el orden

    // Carga y construye un nivel basado en un ScriptableObject
    public void CargarNivel(LevelData nivelACargar)
    {
        // 1. Limpiar el mapa anterior
        foreach (Transform child in contenedorMapa)
        {
            Destroy(child.gameObject);
        }

        // 2. Construir el nuevo mapa
        if (nivelACargar.gridLayout.Length != nivelACargar.ancho * nivelACargar.alto)
        {
            Debug.LogError("¡ERROR! El tamaño del gridLayout no coincide con el ancho y alto del nivel.");
            return;
        }

        for (int y = 0; y < nivelACargar.alto; y++)
        {
            for (int x = 0; x < nivelACargar.ancho; x++)
            {
                TileType tipoDeTile = nivelACargar.gridLayout[y * nivelACargar.ancho + x];
                Vector3 posicion = new Vector3(x, y, 0);

                switch (tipoDeTile)
                {
                    case TileType.Piso:
                        Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa);
                        break;
                    case TileType.Obstaculo:
                        Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa);
                        Instantiate(prefabObstaculo, posicion, Quaternion.identity, contenedorMapa);
                        break;
                    case TileType.Meta:
                         Instantiate(prefabPiso, posicion, Quaternion.identity, contenedorMapa);
                         Instantiate(prefabMeta, posicion, Quaternion.identity, contenedorMapa);
                        break;
                    case TileType.Vacio:
                        break;
                }
            }
        }
        Instantiate(prefabPlayer, (Vector3)(Vector2)nivelACargar.posicionInicioPlayer, Quaternion.identity, contenedorMapa);

        Debug.Log("Cargado Nivel: " + nivelACargar.nombreNivel);
    }
}