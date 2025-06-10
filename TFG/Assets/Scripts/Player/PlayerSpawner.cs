using UnityEngine;

/// <summary>
/// Instancia al Player desde un prefab cuando arranca la escena,
/// posicion�ndolo en un punto de spawn definido.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefab & Spawn Point")]
    [Tooltip("Arrastra aqu� tu prefab del Player")]
    public GameObject playerPrefab;
    [Tooltip("Lugar donde quieres que aparezca el Player. Si es null, usa la posici�n de este GameObject.")]
    public Transform spawnPoint;

    private GameObject playerInstance;

    void Awake()
    {
        // Validaciones
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: debes asignar el Player Prefab en el inspector.");
            enabled = false;
            return;
        }

        // Si no has puesto un spawnPoint, usamos la posici�n de este objeto
        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Quaternion rot = (spawnPoint != null) ? spawnPoint.rotation : Quaternion.identity;

        // Instanciamos el Player
        playerInstance = Instantiate(playerPrefab, pos, rot);
        playerInstance.name = playerPrefab.name; // limpia el "(Clone)"

        // Opcional: si quieres guardar una referencia est�tica:
        // GameManager.Instance.Player = playerInstance;
    }

    /// <summary>
    /// Permite acceder desde otro script a la instancia reci�n creada.
    /// </summary>
    public GameObject GetPlayerInstance()
    {
        return playerInstance;
    }
}
