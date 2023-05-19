using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeLoop : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer = default;
    [SerializeField] private Mesh skinnedMesh = default;
    int blendShapeCount;

    int playIndex = 0;

    void Awake()
    {
        blendShapeCount = skinnedMesh.blendShapeCount;
    }

    void Start()
    {
        blendShapeCount = skinnedMesh.blendShapeCount;
    }

    void Update()
    {
        if (playIndex > 0)
            skinnedMeshRenderer.SetBlendShapeWeight(playIndex - 1, 0f);
        if (playIndex == 0)
            skinnedMeshRenderer.SetBlendShapeWeight(playIndex - 1, 0f);

        skinnedMeshRenderer.SetBlendShapeWeight(playIndex, 100f);
        playIndex++;
        if (playIndex > blendShapeCount - 1)
            playIndex = 0;
    }
}
