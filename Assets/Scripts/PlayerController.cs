using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Sprites de Dirección")]
    public Sprite spriteArriba;
    public Sprite spriteAbajo;
    public Sprite spriteDerecha;
    private SpriteRenderer spriteRenderer;
    private bool juegoTerminado = false;
    public bool estaVivo { get; private set; } = true; 
    public bool estaEnLaMeta { get; private set; } = false;
    private Vector2Int direccionActual = Vector2Int.right; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ActualizarSprite();
        estaVivo = true; 
        estaEnLaMeta = false; 
    }
    public void Avanzar()
    {
        if (juegoTerminado) return;
        transform.position += (Vector3)(Vector2)direccionActual;
        Debug.Log("Player avanzó en dirección " + direccionActual + ". Nueva posición: " + transform.position);
    }

    public void GirarDerecha()
    {
        if (juegoTerminado) return;
        if (direccionActual == Vector2Int.right) direccionActual = Vector2Int.down;
        else if (direccionActual == Vector2Int.down) direccionActual = Vector2Int.left;
        else if (direccionActual == Vector2Int.left) direccionActual = Vector2Int.up;
        else if (direccionActual == Vector2Int.up) direccionActual = Vector2Int.right;

        ActualizarSprite(); 
        Debug.Log("Player giró a la derecha. Nueva dirección: " + direccionActual);
    }

    public void GirarIzquierda()
    {
        if (juegoTerminado) return;
        if (direccionActual == Vector2Int.right) direccionActual = Vector2Int.up;
        else if (direccionActual == Vector2Int.up) direccionActual = Vector2Int.left;
        else if (direccionActual == Vector2Int.left) direccionActual = Vector2Int.down;
        else if (direccionActual == Vector2Int.down) direccionActual = Vector2Int.right;

        ActualizarSprite(); 
        Debug.Log("Player giró a la izquierda. Nueva dirección: " + direccionActual);
    }

    private void ActualizarSprite()
    {
        if (direccionActual == Vector2Int.right)
        {
            spriteRenderer.sprite = spriteDerecha;
            spriteRenderer.flipX = false; 
        }
        else if (direccionActual == Vector2Int.left)
        {
            spriteRenderer.sprite = spriteDerecha; 
            spriteRenderer.flipX = true;   
        }
        else if (direccionActual == Vector2Int.up)
        {
            spriteRenderer.sprite = spriteArriba;
            spriteRenderer.flipX = false;
        }
        else if (direccionActual == Vector2Int.down)
        {
            spriteRenderer.sprite = spriteAbajo;
            spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (juegoTerminado) return; 

        if (other.CompareTag ("Goal"))
        {
            Debug.Log("¡GANASTE EL NIVEL!");
            juegoTerminado = true;
            estaEnLaMeta = true; 
            
            // YA NO LLAMAMOS A MostrarResultadosNivel() DESDE AQUÍ
        }
        else if (other.CompareTag("Hazard"))
        {
            Debug.Log("¡PERDISTE! Caíste al abismo.");
            juegoTerminado = true;
            estaVivo = false; 
            this.enabled = false;
            GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    void Update()
    {
        if (juegoTerminado) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.dKey.wasPressedThisFrame)
        {
            Avanzar();
        }
        if (teclado.eKey.wasPressedThisFrame)
        {
            GirarDerecha();
        }
        if (teclado.qKey.wasPressedThisFrame)
        {
            GirarIzquierda();
        }
    }

    public bool HayObstaculoEnfrente()
    {
        Vector2 direccionActualFloat = (Vector2)direccionActual;

        Vector2 puntoDeInicio = (Vector2)transform.position + (direccionActualFloat * 0.2f);
        float distanciaDelRayo = 0.8f;

        Debug.DrawRay(puntoDeInicio, direccionActualFloat * distanciaDelRayo, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(puntoDeInicio, direccionActualFloat, distanciaDelRayo);

        if (hit.collider != null && (hit.collider.CompareTag("Obstaculo") || hit.collider.CompareTag("Hazard")))
        {
            Debug.DrawRay(puntoDeInicio, direccionActualFloat * distanciaDelRayo, Color.red);
            Debug.Log("¡Obstáculo detectado: " + hit.collider.name + "!");
            return true;
        }

        return false;
    }
    
    public void ForzarFallo()
    {
        if (juegoTerminado) return; 

        Debug.Log("¡PERDISTE! El programa no llegó a la meta.");
        juegoTerminado = true;
        estaVivo = false; 
        this.enabled = false;
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
    
}