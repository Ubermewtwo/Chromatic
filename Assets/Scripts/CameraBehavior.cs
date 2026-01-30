using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform target;
    public float maxMoveSpeed;
    public float minMoveSpeed;
    public float speedOffset;
    public float upOffset;
    public float dist;
    void Start()
    {

    }

    void Update()
    {
        if (target)
        {
            Vector3 targetPos = new Vector3(target.position.x, (target.position.y + upOffset), transform.position.z);

            //Prueba velocidad
            Vector2 target2D = new Vector2(target.position.x, (target.position.y + upOffset));
            Vector2 current2D = new Vector2(transform.position.x, transform.position.y);
            dist = Vector3.Distance(target2D, current2D);

            //float balancedSpeed = Mathf.InverseLerp(maxMoveSpeed, minMoveSpeed, (distMultiplier / 1000) / speedOffset);


            transform.position = Vector3.MoveTowards(transform.position, targetPos, maxMoveSpeed * dist * Time.deltaTime);
        }
    }
}
