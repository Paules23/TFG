using UnityEngine;

public class AutoFadeAndDestroy : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeSpeed = 5f;
    public float alphaMin = 0.3f;
    public float alphaMax = 1f;
    public float lifetime = 2f;

    [Header("Color Settings")]
    public Color baseColor = Color.white; // Cambia este color desde el Inspector

    private SpriteRenderer[] renderers;
    private float t = 0f;

    void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        // Asignar color base al empezar
        foreach (var r in renderers)
        {
            if (r != null)
            {
                Color c = baseColor;
                c.a = r.color.a; // mantener alfa actual
                r.color = c;
            }
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        t += Time.deltaTime * fadeSpeed;
        float alpha = Mathf.Lerp(alphaMin, alphaMax, Mathf.PingPong(t, 1f));

        foreach (var r in renderers)
        {
            if (r != null)
            {
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }
        }
    }
}
