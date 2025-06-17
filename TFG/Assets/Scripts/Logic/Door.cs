using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Destino de teletransporte")]
    public Transform teleportDestination;

    [Header("Referencia de la cámara")]
    public Transform cameraTransform;

    [Header("Offset entre el jugador y la cámara (para que quede a la izquierda)")]
    public Vector2 cameraOffset = new Vector2(4f, 0f);

    [Header("Etiqueta del jugador")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos que sea el jugador
        if (other.CompareTag(playerTag))
        {
            if (teleportDestination == null)
            {
                Debug.LogWarning("TeleportDoor: No se ha asignado el destino.");
                return;
            }

            // Mover al jugador
            other.transform.position = teleportDestination.position;

            // Mover la cámara si se ha asignado
            if (cameraTransform != null)
            {
                Vector3 newCameraPos = teleportDestination.position + (Vector3)cameraOffset;
                newCameraPos.z = cameraTransform.position.z; // Mantener z original de la cámara
                cameraTransform.position = newCameraPos;
            }
        }
    }
}
