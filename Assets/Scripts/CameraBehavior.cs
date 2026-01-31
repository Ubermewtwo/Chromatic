using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Vector3 offset;
    public float minDamping = 5f;
    public float maxDamping = 20f;
    public float minOffset = 2f;
    public float maxOffset = 10f;

    public Transform target;

    private Vector3 velocity = Vector3.zero;

    public void FixedUpdate()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        targetPosition.z = transform.position.z; // Maintain original z position
        float distance = Vector3.Distance(transform.position, targetPosition);

        float t = Mathf.InverseLerp(minOffset, maxOffset, distance);
        float damping = Mathf.Lerp(maxDamping, minDamping, t);

        //Debug.Log($"Distance: {distance}, Damping: {damping}");

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, damping * Time.deltaTime);
    }
}
