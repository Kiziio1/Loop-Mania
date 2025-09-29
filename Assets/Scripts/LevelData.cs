using UnityEngine;

public enum TileType
{
    Vacio,
    Piso,
    Obstaculo,
    Meta,
    Llave,  
    Puerta 
}

[CreateAssetMenu(fileName = "Nuevo Nivel", menuName = "Loop Mania/Nivel")]
public class LevelData : ScriptableObject
{
    [Header("Configuración del Nivel")]
    public int nivelId;
    public string nombreNivel;

    [Header("Diseño del Grid (Mapa)")]
    public int ancho = 10;
    public int alto = 10;
    public TileType[] gridLayout;

    [Header("Posiciones Iniciales")]
    public Vector2Int posicionInicioPlayer;

    [Header("Bloques Permitidos")]
    public bool permitirBucleFor = false;
    public bool permitirCondicionalIf = false;
}