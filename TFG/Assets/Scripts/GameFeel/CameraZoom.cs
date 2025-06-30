using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance;

    [Header("Zoom Settings")]
    [Tooltip("Máximo número de zooms que pueden estar en cola")]
    [SerializeField] private int maxQueueSize = 5;

    private Camera cam;
    private float originalSize;
    private bool isZooming = false;
    private Queue<ZoomRequest> zoomQueue = new Queue<ZoomRequest>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        cam = Camera.main;
        originalSize = cam.orthographicSize;
    }

    public void Zoom(float zoomAmount = 0.05f, float duration = 0.01f)
    {
        if (zoomQueue.Count >= maxQueueSize)
            return;

        zoomQueue.Enqueue(new ZoomRequest(zoomAmount, duration));

        if (!isZooming)
            StartCoroutine(ProcessZoomQueue());
    }

    private IEnumerator ProcessZoomQueue()
    {
        isZooming = true;

        while (zoomQueue.Count > 0)
        {
            ZoomRequest request = zoomQueue.Dequeue();

            float targetSize = originalSize + request.amount;
            float elapsed = 0f;
            float zoomTime = request.duration;

            // Zoom OUT (hacia targetSize)
            float startSize = cam.orthographicSize;
            while (elapsed < zoomTime)
            {
                elapsed += Time.unscaledDeltaTime;
                cam.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / zoomTime);
                yield return null;
            }

            cam.orthographicSize = targetSize;

            // Pequeña pausa en el punto máximo
            yield return new WaitForSecondsRealtime(zoomTime * 0.5f);

            // Zoom IN (volver al original)
            elapsed = 0f;
            while (elapsed < zoomTime)
            {
                elapsed += Time.unscaledDeltaTime;
                cam.orthographicSize = Mathf.Lerp(targetSize, originalSize, elapsed / zoomTime);
                yield return null;
            }

            cam.orthographicSize = originalSize;

            yield return new WaitForSecondsRealtime(0.02f); // espacio entre colas
        }

        isZooming = false;
    }

    private struct ZoomRequest
    {
        public float amount;
        public float duration;

        public ZoomRequest(float amt, float dur)
        {
            amount = amt;
            duration = dur;
        }
    }
}
