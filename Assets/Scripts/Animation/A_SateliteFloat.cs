using UnityEngine;

public class A_SateliteFloat : MonoBehaviour
{
    [SerializeField] private Transform target1;
    [SerializeField] private float t;

    void FixedUpdate()
    {
        if (target1 == null) return;
        Vector2 a = transform.position;
        Vector2 b = target1.position;
        transform.position = Vector2.Lerp(a, b, Mathf.Clamp01(t));
    }
}
