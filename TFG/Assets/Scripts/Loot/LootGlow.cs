using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LootGlow : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    private SpriteRenderer sr;
    private Color baseColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        baseColor.a = maxAlpha; // Siempre empieza visible
        sr.color = baseColor;
    }

    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
