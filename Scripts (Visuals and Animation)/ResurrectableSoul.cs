using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectableSoul : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> mRenderers = default;
    [SerializeField] private Animator animator = default;
    [SerializeField] private ParticleSystem particles = default;
    [SerializeField] private GameObject mainBody = default;
    [SerializeField] private Transform colliderT = default;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private int fadeDelay = 5;
    [SerializeField] private float speed = 15;
    [SerializeField] private float turnSpeed = 15;
    [SerializeField] private float shakeLevel = 100;

    public GameObject GetMainBody() { return mainBody; }

    private Transform target;

    private void Start()
    {
        int delay = GameOptions.GetGOPS().GetIsTutorialMode() ? fadeDelay * 5000 : fadeDelay;
        Invoke("BeginFade", delay);
        target = GameManagerScript.GetGMScript().GetPlayer().transform;
    }

    void Update()
    {
        CheckHighlighting();
        MoveShaky();
        RefacePlayer();
    }

    public void MoveShaky()
    {
        if (target)
                Homing.TurnSelfShaky(turnSpeed, target, mainBody.transform, shakeLevel);
        Homing.MoveForward(speed, mainBody.transform);
    }

    private void RefacePlayer()
    {
        transform.LookAt(GameManagerScript.GetGMScript().GetCamera().transform);
        transform.Rotate(offset);
    }

    private void BeginFade()
    {
        animator.SetTrigger("Fade");
        particles.Stop();
    }

    public void DestroySelf()
    {
        Destroy(mainBody);
    }

	private void CheckHighlighting()
	{
		if (MouseLook.LookTargetTransform != colliderT)
			MouseLook.RemoveHighlighting(mRenderers);
		else
			MouseLook.DoHighlighting(mRenderers);
	}
}
