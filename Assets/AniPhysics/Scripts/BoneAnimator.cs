using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAnimator : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private float maxAlingVelocity = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float impulsePropogation = 0.5f;

    #endregion

    #region Properties
    
    public Transform Reference { get; private set; }
    public Rigidbody CurrentBody { get; private set; }
    public List<BoneAnimator> ConnectedBones { get; private set; }
    public BoneAnimator PreviousBone { get; private set; }
    public BoneAnimator NextBone { get; private set; }

    #endregion

    private void FixedUpdate()
    {
        if (Reference != null)
        {
            AlignPositionAndRotation();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ResolveCollision(collision);
    }

    public void SetReference(Transform reference)
    {
        Reference = reference;
        CurrentBody = Reference.GetComponent<Rigidbody>();

        if (CurrentBody != null)
        {
            CurrentBody.isKinematic = true;
        }
    }

    public void InitializeConnected()
    {
        if (transform.parent != null)
        {
            PreviousBone = transform.parent.GetComponent<BoneAnimator>();
            
            if (PreviousBone != null)
            {
                ConnectedBones.Add(PreviousBone);
            }
        }

        if (transform.childCount > 0)
        {
            NextBone = transform.GetChild(0).GetComponent<BoneAnimator>();

            if (NextBone != null)
            {
                ConnectedBones.Add(NextBone);
            }
        }
    }

    private void ResolveCollision(Collision collision)
    {
        if (collision.rigidbody != null)
        {

        }
    }

    private void AlignPositionAndRotation()
    {
        transform.localPosition = Reference.localPosition;
        transform.localRotation = Reference.localRotation;
    }
}
