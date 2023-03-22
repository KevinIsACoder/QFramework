using UnityEngine;

public class DontGoThroughtThings : MonoBehaviour {
    public LayerMask layerMask = -1;

    private float minimumExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    private Collider myCollider;
    private float radius;

    void Start()
    {
        radius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        previousPosition = myRigidbody.position;
        minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }

    void FixedUpdate()
    {
        //have we moved more than our minimum extent? 
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;
            if (Physics.SphereCast(previousPosition, radius, movementThisStep, out hitInfo, movementMagnitude, layerMask.value))
            {
                if (!hitInfo.transform.tag.Equals(transform.tag))
                {
                    if (!hitInfo.collider)
                        return;

                    if (!hitInfo.collider.isTrigger)
                    {
                        Vector3 endPosition = previousPosition + (movementThisStep.normalized * hitInfo.distance);
                        myRigidbody.position = endPosition;
                    }

                }
            }
        }
        previousPosition = myRigidbody.position;
    }
}