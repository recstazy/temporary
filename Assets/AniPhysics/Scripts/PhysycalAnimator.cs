using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycalAnimator : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Transform referenceHips;

    [SerializeField]
    private Transform targetHips;

    private BoneAnimator[] animators;

    #endregion

    #region Properties
    
    #endregion

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        var references = referenceHips.GetComponentsInChildren<Transform>();
        var targets = targetHips.GetComponentsInChildren<Transform>();

        animators = new BoneAnimator[references.Length];

        for (int i = 0; i < references.Length; i++)
        {
            animators[i] = targets[i].gameObject.AddComponent<BoneAnimator>();
            animators[i].SetReference(references[i]);
        }

        foreach (var a in animators)
        {
            a.InitializeConnected();
        }
    }
}
