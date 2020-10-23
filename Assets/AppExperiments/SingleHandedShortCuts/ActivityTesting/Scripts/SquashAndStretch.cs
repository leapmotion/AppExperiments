using UnityEngine;
 
[ExecuteInEditMode]
public class SquashAndStretch : MonoBehaviour
{
    Vector3 lastPosition;
 
    public float SquashScale;

    void Start()
    {
        lastPosition = transform.position;
    }
   
    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        transform.localRotation = Quaternion.LookRotation(delta + Vector3.forward * 0.001f);
        float l = 1f + (delta.magnitude * SquashScale);
        float wh = Mathf.Sqrt(1f / l);
        transform.localScale = new Vector3(wh, wh, l);
 
        lastPosition = transform.position;
    }
}