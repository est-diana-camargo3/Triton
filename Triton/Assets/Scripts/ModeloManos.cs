using UnityEngine;

public class ModeloManos : MonoBehaviour
{
    [Header("IK Targets")]
    public Transform targetHandR;
    public Transform targetHandL;

    [Header("Quest Controllers")]
    public Transform rightControllerAnchor;
    public Transform leftControllerAnchor;

    [Header("Rotation Offset")]
    public Vector3 rightRotationOffset;
    public Vector3 leftRotationOffset;

    public GameObject modeloSirena;
    public GameObject camara;

    void Update()
    {
        modeloSirena.transform.position = camara.transform.position;
        
        if (rightControllerAnchor != null && targetHandR != null)
        {
            targetHandR.position = rightControllerAnchor.position;
            targetHandR.rotation = rightControllerAnchor.rotation * 
                                   Quaternion.Euler(rightRotationOffset);
        }

        if (leftControllerAnchor != null && targetHandL != null)
        {
            targetHandL.position = leftControllerAnchor.position;
            targetHandL.rotation = leftControllerAnchor.rotation * 
                                   Quaternion.Euler(leftRotationOffset);
        }
    }
}
