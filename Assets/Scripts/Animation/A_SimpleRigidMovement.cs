using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class A_SimpleRigidMovement : MonoBehaviour
{
    [SerializeField]private bool loop;
    public Vector2 moveDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartCoroutine(GravityKick());
    }

    private void FixedUpdate()
    {
        transform.Translate(moveDirection, Space.World);
    }

    IEnumerator GravityKick()
    {
        while (loop)
        {
            yield return new WaitForSeconds(1.4f);
            moveDirection = new Vector2(0, 0.01f);
            yield return new WaitForSeconds(1.4f);
            moveDirection = new Vector2(0, -0.01f);
        }
    }
}
