using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController playerController;
    public Transform panelPrograma;
    public void EjecutarPrograma()
    {
        Debug.Log("¡Ejecutando programa!");

        foreach (Transform bloqueTransform in panelPrograma)
        {
            BloqueDeComando comando = bloqueTransform.GetComponent<BloqueDeComando>();
            
            if (comando != null)
            {
                switch (comando.tipoDeComando)
                {
                    case BloqueDeComando.TipoComando.Avanzar:
                        playerController.Avanzar();
                        break;
                    
                    case BloqueDeComando.TipoComando.GirarDerecha:
                        // Aún no tenemos este método, pero aquí iría la llamada.
                        // playerController.GirarDerecha();
                        Debug.Log("Comando Girar Derecha (aún no implementado).");
                        break;
                }
            }
        }
    }
}