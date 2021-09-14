using UnityEngine;
public class BasicCam : MonoBehaviour
{
    public Transform target;
    Vector3 offset;

    private void Start() => offset = transform.position - target.transform.position;
    private void LateUpdate() => transform.position = target.position + offset;
}
