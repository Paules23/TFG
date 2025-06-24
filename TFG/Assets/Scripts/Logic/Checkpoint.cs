using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.green;
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
    }
}
