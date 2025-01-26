using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f; // How long the shake lasts
    [SerializeField] private float magnitude = 0.1f; // How strong the shake is

    private Vector3 originalPosition; // To store the camera's original position

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        StartCoroutine(ShakeRoutine());
    }

    private System.Collections.IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Generate a random offset for the camera
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            // Apply the offset
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Reset the camera to its original position
        transform.localPosition = originalPosition;
    }
}
