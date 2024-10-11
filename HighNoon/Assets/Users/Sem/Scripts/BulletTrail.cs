using System.Collections;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    public LineRenderer lineRenderer; // Reference to the LineRenderer
    public float bulletSpeed = 20f; // Speed of the bullet
    public float trailDuration = 0.1f; // How long the trail will last

    public Transform muzzlePoint; // The starting point of the bullet (usually the gun muzzle)

    public void FireBullet(Vector3 target)
    {
        // Start a coroutine to draw the trail
        StopAllCoroutines();
        StartCoroutine(DrawBulletTrail(muzzlePoint.position, target));
    }

    private IEnumerator DrawBulletTrail(Vector3 start, Vector3 end)
    {
        // Set the start and end positions for the LineRenderer
        lineRenderer.SetPosition(0, start); // Set the starting point
        lineRenderer.SetPosition(1, start); // Temporarily set the endpoint to the start point

        float distance = Vector3.Distance(start, end);
        float travelTime = distance / bulletSpeed;
        float currentTime = 0f;

        // Animate the trail moving from start to end
        while (currentTime < travelTime)
        {
            currentTime += Time.deltaTime;
            float lerpValue = currentTime / travelTime;
            Vector3 currentPosition = Vector3.Lerp(start, end, lerpValue);

            lineRenderer.SetPosition(1, currentPosition); // Update the endpoint of the line

            yield return null;
        }

        // After reaching the target, leave the trail for a short duration
        yield return new WaitForSeconds(trailDuration);

        // Disable the LineRenderer after the trail has been displayed
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
    }
}
