using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
    private Vector3 from,
                     to;

    private float howfar;

    protected bool idle = true;
    public bool Idle
    {
        get
        {
            return idle;
        }
    }

    [SerializeField] private float speed = 1;
    [SerializeField] private TrailRenderer trail;

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        from = transform.position;
        to = targetPosition;
        howfar = 0;
        idle = false;

        trail.enabled = true;
        do
        {
            howfar += speed * Time.deltaTime;

            if (howfar > 1)
                howfar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        trail.enabled = false;

        idle = true;
    }


    public IEnumerator MoveToTransform(Transform target)
    {
        from = transform.position;
        to = target.position;
        howfar = 0;
        idle = false;

        do
        {
            howfar += speed * Time.deltaTime;

            if (howfar > 1)
                howfar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        trail.enabled = false;

        idle = true;
    }

    private float Easing(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}
