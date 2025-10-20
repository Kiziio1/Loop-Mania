using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI levelText;
    public Transform starContainer; 
    public Button myButton;

    [Header("Configuración")]
    public int nivelIndex; 

    private GameObject starPrefab;

    public void Setup(int levelNum, int stars, bool isLocked, GameObject prefabEstrella)
    {
        nivelIndex = levelNum;
        levelText.text = nivelIndex.ToString();
        this.starPrefab = prefabEstrella;

        myButton.interactable = !isLocked;

        foreach (Transform child in starContainer)
        {
            Destroy(child.gameObject);
        }

        if (!isLocked && stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                
                GameObject starInstance = Instantiate(starPrefab);

                starInstance.transform.SetParent(starContainer);

                starInstance.transform.localScale = Vector3.one; 
            }
        }
    }

    public void CargarNivel()
    {
        AchievementManager.NivelACargar = nivelIndex;
        SceneManager.LoadScene("GameLoop");
    }
}