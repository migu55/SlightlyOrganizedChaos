using UnityEngine;

public class ReadyTextHolder : MonoBehaviour
{
    public Vector3 pos;
    public Quaternion rot;

    void Start()
    {
        pos = transform.position;
        rot = transform.rotation;
    }

    public void ResetTransform()
    {
        pos = new(0,2,-0.2f);
        rot = new(0,0,0,0);
    }
}
