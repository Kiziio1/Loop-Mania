using UnityEngine;
using UnityEditor;

public class LevelEditorWindow : EditorWindow
{
    private static LevelData currentLevel;
    private Vector2 scrollPosition;

    [MenuItem("Window/Loop Mania/Editor de Niveles")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Editor de Niveles");
    }

    public static void OpenWindow(LevelData levelData)
    {
        currentLevel = levelData;
        ShowWindow();
    }

    void OnGUI()
    {
        if (currentLevel == null)
        {
            EditorGUILayout.LabelField("Selecciona un archivo de Nivel para editarlo.", EditorStyles.boldLabel);
            return;
        }

        titleContent = new GUIContent("Editando: " + currentLevel.name);
        
        currentLevel.ancho = EditorGUILayout.IntField("Ancho del Mapa", currentLevel.ancho);
        currentLevel.alto = EditorGUILayout.IntField("Alto del Mapa", currentLevel.alto);

        if (currentLevel.gridLayout == null || currentLevel.gridLayout.Length != currentLevel.ancho * currentLevel.alto)
        {
            currentLevel.gridLayout = new TileType[currentLevel.ancho * currentLevel.alto];
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Haz clic en las casillas para cambiar el tipo de tile:", EditorStyles.miniLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int y = 0; y < currentLevel.alto; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < currentLevel.ancho; x++)
            {
                int index = y * currentLevel.ancho + x;
                string buttonLabel = GetTileLabel(currentLevel.gridLayout[index]);

                if (GUILayout.Button(buttonLabel, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    currentLevel.gridLayout[index] = GetNextTileType(currentLevel.gridLayout[index]);
                    EditorUtility.SetDirty(currentLevel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private string GetTileLabel(TileType tile)
    {
        switch (tile)
        {
            case TileType.Vacio: return " ";
            case TileType.Piso: return "P";
            case TileType.Obstaculo: return "O";
            case TileType.Meta: return "M";
            case TileType.Llave: return "L";
            case TileType.Puerta: return "D";
            default: return "?";
        }
    }

    private TileType GetNextTileType(TileType currentType)
    {
        int next = ((int)currentType + 1) % System.Enum.GetValues(typeof(TileType)).Length;
        return (TileType)next;
    }
}

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelData levelData = (LevelData)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Abrir Editor de Niveles Visual", GUILayout.Height(40)))
        {
            LevelEditorWindow.OpenWindow(levelData);
        }
    }
}