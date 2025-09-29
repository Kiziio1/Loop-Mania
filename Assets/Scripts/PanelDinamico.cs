using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class PanelDinamico : MonoBehaviour
{
    [Header("Configuración del Panel")]
    public float alturaMinima = 763.64f;

    [Tooltip("Añade un valor aquí para darle más espacio al final del scroll.")]
    public float ajusteVerticalExtra = 50f;

    private RectTransform miRectTransform;
    private VerticalLayoutGroup vlg;

    void Awake()
    {
        miRectTransform = GetComponent<RectTransform>();
        vlg = GetComponent<VerticalLayoutGroup>();
        IniciarActualizacionAltura();
    }

    public void IniciarActualizacionAltura()
    {
        StopAllCoroutines();
        StartCoroutine(ActualizarAlturaCoroutine());
    }

    private IEnumerator ActualizarAlturaCoroutine()
    {
        yield return null;

        if (vlg == null) yield break;

        float alturaContenido = vlg.padding.top + vlg.padding.bottom;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform hijo = transform.GetChild(i).GetComponent<RectTransform>();
            if (hijo != null)
            {
                alturaContenido += hijo.sizeDelta.y;
            }
        }

        if (transform.childCount > 1)
        {
            alturaContenido += vlg.spacing * (transform.childCount - 1);
        }

        float alturaFinal = Mathf.Max(alturaMinima, alturaContenido + ajusteVerticalExtra);
        
        miRectTransform.sizeDelta = new Vector2(miRectTransform.sizeDelta.x, alturaFinal);
    }
}