using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float ydirection = 1;
    [SerializeField] private float spinrate = 30.0f;

    private void FixedUpdate()
    {
        Rotate(transform, spinrate, ydirection);
    }

    public static void Rotate(Transform transform, float rate, float ydir)
    {
        transform.Rotate(0, ydir * rate * Time.deltaTime, 0, Space.Self);
    }
}
