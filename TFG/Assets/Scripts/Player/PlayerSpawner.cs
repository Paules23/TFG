using UnityEngine;

/// <summary>
/// Instancia al Player desde un prefab cuando arranca la escena,
/// posicionándolo en la posición y rotación del GameObject (este spawner).
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("Arrastra aquí tu prefab del Player")]
    public GameObject playerPrefab;

    private GameObject playerInstance;

    void Awake()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: debes asignar el Player Prefab en el Inspector.");
            enabled = false;
            return;
        }

        // Usamos la posición y rotación del GameObject (este spawner) para instanciar el Player.
        playerInstance = Instantiate(playerPrefab, transform.position, transform.rotation);
        playerInstance.name = playerPrefab.name; // Elimina el "(Clone)"
        playerInstance.tag = "Player";  // Aseguramos que tiene la etiqueta "Player" para que otros scripts lo encuentren.
    }

    public GameObject GetPlayerInstance()
    {
        return playerInstance;
    }
}
