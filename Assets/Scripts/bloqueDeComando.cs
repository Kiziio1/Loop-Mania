using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BloqueDeComando : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum TipoComando { Avanzar, GirarDerecha }
    public TipoComando tipoDeComando;

    public bool esOriginalDelBanco = false;

    private Transform padreOriginal;
    private CanvasGroup canvasGroup;
    private Vector3 posicionOriginal;

    void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        padreOriginal = transform.parent;
        posicionOriginal = transform.position; 
        canvasGroup.blocksRaycasts = false;

        if (esOriginalDelBanco)
        {
            GameObject clon = Instantiate(this.gameObject, padreOriginal);
            clon.GetComponent<BloqueDeComando>().esOriginalDelBanco = false;
    
            eventData.pointerDrag = clon;
        }
        else 
        {
            transform.SetParent(transform.root);
            canvasGroup.alpha = 0.6f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (esOriginalDelBanco)
        {
            transform.position = posicionOriginal;
            return;
        }
        transform.position = Mouse.current.position.ReadValue();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (esOriginalDelBanco)
        {
            canvasGroup.blocksRaycasts = true;
            return;
        }

        if (transform.parent == transform.root)
        {
            transform.SetParent(padreOriginal);
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}