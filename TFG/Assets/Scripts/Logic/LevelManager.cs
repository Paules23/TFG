using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Destino de teletransporte")]
    public Transform teleportDestination;

    [Header("Referencia de la cámara")]
    public Transform cameraTransform;

    [Header("Offset entre el jugador y la cámara")]
    public Vector2 cameraOffset = new Vector2(4f, 0f);

    [Header("Etiqueta del jugador")]
    public string playerTag = "Player";

    [Header("Nivel actual (a eliminar)")]
    public GameObject currentLevel;

    [Header("Nivel siguiente (a activar)")]
    public GameObject nextLevel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // --- TELETRANSPORTE ---
        if (teleportDestination == null)
        {
            Debug.LogWarning("LevelManager: No se ha asignado el destino.");
            return;
        }

        other.transform.position = teleportDestination.position;

        if (cameraTransform != null)
        {
            Vector3 newCameraPos = teleportDestination.position + (Vector3)cameraOffset;
            newCameraPos.z = cameraTransform.position.z;
            cameraTransform.position = newCameraPos;
        }

        // --- GESTIÓN DE NIVELES ---
        if (currentLevel != null)
            Destroy(currentLevel);

        if (nextLevel != null)
            nextLevel.SetActive(true);
        else
            Debug.LogWarning("LevelManager: El nuevo nivel no está asignado.");
    }
}
