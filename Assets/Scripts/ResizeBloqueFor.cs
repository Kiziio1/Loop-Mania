using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ResizeBloqueFor: MonoBehaviour
{
    public RectTransform headerContenedor;
    public RectTransform contenidoContenedor;
    public float ajusteVerticalExtra = 10f;
    private RectTransform miRectTransform;

    void Awake()
    {
        miRectTransform = GetComponent<RectTransform>();
    }

    public void IniciarActualizacionAltura()
    {
        StopAllCoroutines();
        StartCoroutine(ActualizarAlturaCoroutine());
    }

    private IEnumerator ActualizarAlturaCoroutine()
    {
        yield return new WaitForEndOfFrame();

        if (contenidoContenedor == null || headerContenedor == null) yield break;

        var vlg = contenidoContenedor.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) yield break;

        float alturaContenido = vlg.padding.top + vlg.padding.bottom;

        for (int i = 0; i < contenidoContenedor.childCount; i++)
        {
            var hijoRect = contenidoContenedor.GetChild(i).GetComponent<RectTransform>();
            if (hijoRect != null)
            {
                alturaContenido += hijoRect.sizeDelta.y;
            }
        }

        if (contenidoContenedor.childCount > 1)
        {
            alturaContenido += vlg.spacing * (contenidoContenedor.childCount - 1);
        }

        float alturaFinal = Mathf.Max(headerContenedor.sizeDelta.y, alturaContenido) + ajusteVerticalExtra;
        miRectTransform.sizeDelta = new Vector2(miRectTransform.sizeDelta.x, alturaFinal);

        if (transform.parent != null)
        {
            ResizeBloqueFor resizerPadre = transform.parent.GetComponentInParent<ResizeBloqueFor>();
            if (resizerPadre != null && resizerPadre != this)
            {
                resizerPadre.IniciarActualizacionAltura();
            }
            else
            {
                PanelDinamico panelPadre = transform.parent.GetComponentInParent<PanelDinamico>();
                if (panelPadre != null)
                {
                    panelPadre.IniciarActualizacionAltura();
                }
            }
        }
    }
}