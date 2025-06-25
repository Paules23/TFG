using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Teletransporte")]
    public Transform teleportDestination;
    [Header("Cámara")]
    public Transform cameraTransform;
    public Vector2 cameraOffset = new Vector2(4f, 0f);
    [Header("Niveles")]
    public GameObject currentLevel;
    public GameObject nextLevel;
    [Header("Tags")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Si no hay siguiente nivel, mostramos menú de fin de juego
        if (nextLevel == null)
        {
            UIManager.Instance.ShowEndGame();
            return;
        }

        // ... resto de tu teletransporte ...
        other.transform.position = teleportDestination.position;

        if (cameraTransform != null)
        {
            var pos = (Vector3)cameraOffset + teleportDestination.position;
            pos.z = cameraTransform.position.z;
            cameraTransform.position = pos;
        }

        if (currentLevel != null)
            Destroy(currentLevel);
        nextLevel.SetActive(true);
    }
}
