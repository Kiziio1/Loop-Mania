using UnityEngine;
using UnityEngine.EventSystems;

public class Basurero : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            BloqueDeComando bloque = eventData.pointerDrag.GetComponent<BloqueDeComando>();
            if (bloque != null && !bloque.esOriginalDelBanco)
            {
                Destroy(eventData.pointerDrag);
            }
        }
    }
}