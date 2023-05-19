using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLine : MonoBehaviour
{
    [SerializeField] private LineRenderer lr = default;
    internal Transform startTarget;
    internal Transform endTarget;
    internal Vector3 lastStartPos = Vector3.zero;
    internal Vector3 lastEndPos = Vector3.zero;
    internal Vector3 startPosOffset = Vector3.zero;
    internal Vector3 endPosOffset = Vector3.zero;
    internal Vector3 endFollowOffset = Vector3.zero;

    internal void SetMaterial(Material value) { lr.material = value; }
    internal void SetStartOffset(Vector3 value) { startPosOffset = value; }
    internal void SetEndOffset(Vector3 value) { endPosOffset = value; }


    void LateUpdate()
    {
        HandleStartPos();

        HandleEndPos();
    }

    private void HandleEndFollow()
    {
        if (startTarget)
        {
            lr.SetPosition(0, startTarget.position + endFollowOffset);
        }
    }

    private void HandleStartPos()
    {
        if (startTarget)
        {
            lastStartPos = startTarget.position;
            lr.SetPosition(0, startTarget.position + startPosOffset);
        }
        else
            lr.SetPosition(0, lastStartPos + startPosOffset);
    }

    private void HandleEndPos()
    {
        if (endTarget)
        {
            lastEndPos = endTarget.position;
            lr.SetPosition(1, endTarget.position + endPosOffset);
        }
        else if (lastEndPos != Vector3.zero)
        {
            lr.SetPosition(1, lastEndPos + endPosOffset);
            lastEndPos = lastEndPos + endPosOffset;
        }
        else
            lr.SetPosition(1, lastEndPos + endPosOffset);
    }

    internal void SetTargets(Transform start, Transform end)
    {
        startTarget = start;
        endTarget = end;
    }

    internal void SetTargets(Transform start, Vector3 end)
    {
        startTarget = start;
        lastEndPos = end;
    }

    internal void SetTargets(Vector3 start, Transform end)
    {
        lastStartPos = start;
        endTarget = end;
    }

    internal void SetTargets(Vector3 start, Vector3 end)
    {
        lastStartPos = start;
        lastEndPos = end;
    }

    internal void SetWidths(float start, float end)
    {
        lr.startWidth = start;
        lr.endWidth = end;
    }
}
