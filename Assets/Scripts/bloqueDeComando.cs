using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class BloqueDeComando : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public enum TipoComando { Avanzar, GirarDerecha, GirarIzquierda, BucleFor, CondicionalSi }
    public TipoComando tipoDeComando;
    public bool esOriginalDelBanco = false;

    [Header("Para Bucle For")]
    public int repeticionesBucle = 3;
    public Transform contenedorDeBloquesAnidados;
    public TMP_Text textoRepeticiones;
    public Button botonIncrementar;
    public Button botonDecrementar;

    private Transform padreOriginal;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        ActualizarUITexto();

        if (tipoDeComando == TipoComando.BucleFor)
        {
            if (botonIncrementar != null)
            {
                botonIncrementar.onClick.RemoveAllListeners();
                botonIncrementar.onClick.AddListener(IncrementarRepeticiones);
            }
            if (botonDecrementar != null)
            {
                botonDecrementar.onClick.RemoveAllListeners();
                botonDecrementar.onClick.AddListener(DecrementarRepeticiones);
            }
        }
    }

    public void IncrementarRepeticiones()
    {
        repeticionesBucle++;
        ActualizarUITexto();
    }

    public void DecrementarRepeticiones()
    {
        if (repeticionesBucle > 1)
        {
            repeticionesBucle--;
            ActualizarUITexto();
        }
    }

    private void ActualizarUITexto()
    {
        if (tipoDeComando == TipoComando.BucleFor && textoRepeticiones != null)
        {
            textoRepeticiones.text = repeticionesBucle.ToString();
        }
    }
    
    public IEnumerator Ejecutar(PlayerController player, float tiempoDeEspera)
    {
        switch (tipoDeComando)
        {
            case TipoComando.Avanzar:
                player.Avanzar();
                yield return new WaitForSeconds(tiempoDeEspera);
                break;
            case TipoComando.GirarDerecha:
                player.GirarDerecha();
                yield return new WaitForSeconds(tiempoDeEspera);
                break;
            case TipoComando.GirarIzquierda:
                player.GirarIzquierda();
                yield return new WaitForSeconds(tiempoDeEspera);
                break;
            case TipoComando.BucleFor:
                if (contenedorDeBloquesAnidados != null)
                {
                    for (int i = 0; i < repeticionesBucle; i++)
                    {
                        foreach (Transform bloqueAnidado in contenedorDeBloquesAnidados)
                        {
                            BloqueDeComando comando = bloqueAnidado.GetComponent<BloqueDeComando>();
                            if (comando != null)
                            {
                                yield return StartCoroutine(comando.Ejecutar(player, tiempoDeEspera));
                            }
                        }
                    }
                }
                break;
            case TipoComando.CondicionalSi:
                if (player.HayObstaculoEnfrente())
                {
                    if (contenedorDeBloquesAnidados != null)
                    {
                        foreach (Transform bloqueAnidado in contenedorDeBloquesAnidados)
                        {
                            BloqueDeComando comando = bloqueAnidado.GetComponent<BloqueDeComando>();
                            if (comando != null)
                            {
                                yield return StartCoroutine(comando.Ejecutar(player, tiempoDeEspera));
                            }
                        }
                    }
                }
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (esOriginalDelBanco)
        {
            GameObject clon = Instantiate(gameObject, transform.root);
            clon.transform.position = eventData.position;
            BloqueDeComando clonScript = clon.GetComponent<BloqueDeComando>();
            clonScript.esOriginalDelBanco = false;
            clonScript.padreOriginal = this.transform.parent;
            clonScript.canvasGroup = clon.GetComponent<CanvasGroup>();
            clonScript.canvasGroup.blocksRaycasts = false;
            eventData.pointerDrag = clon;
            return;
        }

        padreOriginal = transform.parent;
        if (padreOriginal != null)
        {
            PanelDinamico panel = padreOriginal.GetComponent<PanelDinamico>();
            if (panel != null)
            {
                panel.IniciarActualizacionAltura();
            }
        }
        transform.SetParent(transform.root);
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!esOriginalDelBanco)
        {
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (esOriginalDelBanco) return;

        canvasGroup.alpha = 1f;

        if (transform.parent == transform.root)
        {
            transform.SetParent(padreOriginal);
        }

        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (tipoDeComando != TipoComando.BucleFor && tipoDeComando != TipoComando.CondicionalSi) return;

        GameObject bloqueArrastrado = eventData.pointerDrag;
        if (bloqueArrastrado != null)
        {
            bloqueArrastrado.transform.SetParent(contenedorDeBloquesAnidados);
            ResizeBloqueFor resizer = GetComponent<ResizeBloqueFor>();
            if (resizer != null)
            {
                resizer.IniciarActualizacionAltura();
            }
        }
    }
}