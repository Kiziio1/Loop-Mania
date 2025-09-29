using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        GameObject bloqueArrastrado = eventData.pointerDrag;

        if (bloqueArrastrado != null)
        {
            bloqueArrastrado.transform.SetParent(this.transform);
            bloqueArrastrado.transform.SetAsLastSibling();

            ResizeBloqueFor resizerContenedor = GetComponentInParent<ResizeBloqueFor>();
            if (resizerContenedor != null)
            {
                resizerContenedor.IniciarActualizacionAltura();
            }

            PanelDinamico panelDinamico = GetComponent<PanelDinamico>();
            if (panelDinamico != null)
            {
                panelDinamico.IniciarActualizacionAltura();
            }
        }
    }
}