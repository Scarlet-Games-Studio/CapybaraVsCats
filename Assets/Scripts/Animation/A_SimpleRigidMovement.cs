using System.Collections;
using UnityEngine;

/// <summary>
/// Applies the small authored drift used by background props. The parallax layer
/// remains responsible for scrolling the whole scene; this component only adds
/// optional local drift/oscillation.
/// </summary>
public class A_SimpleRigidMovement : MonoBehaviour
{
    [SerializeField] bool loop;
    [Tooltip("Deslocamento por frame a 50 FPS, preservando os valores antigos do Inspector.")]
    public Vector2 moveDirection;
    public bool horizontal;

    const float ReferenceFrameRate = 50f;
    Coroutine oscillationRoutine;

    void OnEnable()
    {
        if (loop) oscillationRoutine = StartCoroutine(GravityKick());
    }

    void OnDisable()
    {
        if (oscillationRoutine != null)
        {
            StopCoroutine(oscillationRoutine);
            oscillationRoutine = null;
        }
    }

    void FixedUpdate()
    {
        if (moveDirection.sqrMagnitude <= 0f) return;
        transform.Translate(moveDirection * ReferenceFrameRate * Time.fixedDeltaTime, Space.World);
    }

    IEnumerator GravityKick()
    {
        float amplitude = Mathf.Max(0.01f, moveDirection.magnitude);
        while (loop)
        {
            yield return new WaitForSeconds(horizontal ? 2f : 1.4f);
            moveDirection = horizontal ? new Vector2(amplitude, 0f) : new Vector2(0f, amplitude);
            yield return new WaitForSeconds(horizontal ? 2f : 1.4f);
            moveDirection = horizontal ? new Vector2(-amplitude, 0f) : new Vector2(0f, -amplitude);
        }
    }
}
