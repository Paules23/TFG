using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.green;

    [Header("Camera Positioning")]
    [Tooltip("Posici�n a la que se mover� la c�mara al activarse este checkpoint")]
    public Vector3 cameraTargetPosition;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = inactiveColor;
    }

    public void SetActive(bool isActive)
    {
        if (sr != null)
            sr.color = isActive ? activeColor : inactiveColor;

        if (isActive)
        {
            MoveCameraToCheckpoint();
        }
    }

    private void MoveCameraToCheckpoint()
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(
                cameraTargetPosition.x,
                cameraTargetPosition.y,
                Camera.main.transform.position.z // mantener Z original
            );
        }
        else
        {
            Debug.LogWarning("[Checkpoint] No se encontr� la c�mara principal (Camera.main)");
        }
    }
}
