using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentAndFollowPosition : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Transform ragdollRoot;

    [SerializeField]
    private Transform ragdollHips;

    #endregion

    #region Properties
    
    #endregion

    private void OnEnable()
    {
        ragdollRoot.transform.SetParent(transform.parent);
    }

    private void OnDisable()
    {
        ragdollRoot.transform.SetParent(transform);
    }

    private void FixedUpdate()
    {
        var position = ragdollHips.transform.position;
        position.y = transform.position.y;
        transform.position = position;
    }
}
