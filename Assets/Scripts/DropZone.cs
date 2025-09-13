using UnityEngine;
using UnityEngine.EventSystems; // Necesario para la interfaz de Drop

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("¡Bloque soltado en el panel!");
        GameObject bloqueArrastrado = eventData.pointerDrag;

        bloqueArrastrado.transform.SetParent(this.transform);
    }
}