using UnityEngine;
using UnityEngine.InputSystem; // Puedes mantener esto por si lo necesitas más tarde

public class PlayerController : MonoBehaviour
{
    private bool juegoTerminado = false;

    public void Avanzar()
    {
        if (juegoTerminado) return;

        transform.position += Vector3.right;
        Debug.Log("Player avanzó a la posición: " + transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (juegoTerminado) return;

        if (other.CompareTag("Goal"))
        {
            Debug.Log("¡GANASTE! Tocaste la meta.");
            juegoTerminado = true;
        }
        else if (other.CompareTag("Hazard"))
        {
            Debug.Log("¡PERDISTE! Caíste al abismo.");
            juegoTerminado = true;
            this.enabled = false;
            GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }


    void Update()
    {
        if (juegoTerminado) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.rightArrowKey.wasPressedThisFrame)
        {
            transform.position += Vector3.right;
        }
        if (teclado.leftArrowKey.wasPressedThisFrame)
        {
            transform.position += Vector3.left;
        }
    }
    
}