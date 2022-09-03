using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWanderer : MonoBehaviour
{
    [SerializeField] private float speed = 100;
    [SerializeField] private float acceleration = 1;
    [SerializeField] private float playerTargetWeighting = 0.1f;
    [SerializeField] private float sineWeighting = 0.3f;
    [SerializeField] private Animator animator = default;
    private float startingSineOffset = 0;
    private GameManagerScript gm;
    private Transform playerTransform;

    private void Start()
    {
        startingSineOffset = Random.Range(-1.0f, 1.0f);
        gm = GameManagerScript.GetGMScript();
        playerTransform = gm.GetPlayer().transform;
    }

    public void Update()
    {
        MoveSemiRandomly();
        speed += acceleration * Time.deltaTime;
    }

    private void MoveSemiRandomly()
    {
        Homing.NoYTurn(playerTargetWeighting, playerTransform, transform);

        float floatSine = Mathf.Sin(Time.time + startingSineOffset) / 2;
        transform.Rotate(Vector3.up * floatSine * sineWeighting);

        Homing.MoveForward(speed, transform);
    }
    public void AnimateDestroy()
    {
        animator.SetTrigger("kill");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
