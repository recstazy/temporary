using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAnimationBlending : MonoBehaviour
{
    [SerializeField]
    private Transform referenceRootBone;

    [SerializeField]
    private Transform targetRootBone;

    private void Awake()
    {
        SetupBodies();
    }

    private void SetupBodies()
    {

    }
}
