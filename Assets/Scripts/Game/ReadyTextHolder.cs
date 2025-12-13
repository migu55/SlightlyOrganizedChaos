using UnityEngine;

public class ReadyTextHolder : MonoBehaviour
{
    // Resets the local transform to the starting position of the text holder
    public void ResetTransform()
    {
        transform.SetLocalPositionAndRotation(new(0,2,-0.2f), new(0,0,0,0));
    }
}
