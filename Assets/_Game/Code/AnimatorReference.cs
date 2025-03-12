using Unity.Entities;
using UnityEngine;

public class AnimatorReference : MonoBehaviour
{
    public Animator animatorReference;
}

public struct AnimatorReferenceComponent : ICleanupComponentData
{
    public Animator animatorReference;
}