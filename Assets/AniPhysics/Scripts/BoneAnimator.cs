using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAnimator : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private BoneAnimationSettings settings;

    #endregion

    #region Properties
    
    public Transform Reference { get; private set; }
    public Rigidbody CurrentBody { get; private set; }
    public List<BoneAnimator> ConnectedBones { get; private set; } = new List<BoneAnimator>();
    public BoneAnimator PreviousBone { get; private set; }
    public BoneAnimator NextBone { get; private set; }
    public BoneAnimationSettings Settings { get => settings; set => settings = value; }

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
        CurrentBody = GetComponent<Rigidbody>();

        if (CurrentBody != null)
        {
            CurrentBody.isKinematic = false;
            CurrentBody.useGravity = false;
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
        Vector3 lastPosition = transform.localPosition;
        Quaternion lastRotation = transform.localRotation;

        var newPosition = Vector3.Lerp(transform.localPosition, Reference.localPosition, Time.fixedDeltaTime * Settings.PositionAlignSpeed);
        var delta = newPosition - lastPosition;
        newPosition = lastPosition + delta.normalized * Mathf.Clamp(delta.magnitude, 0f, Settings.MaxPositionAlignSpeed * Time.fixedDeltaTime);
        transform.localPosition = newPosition;

        var newRotation = Quaternion.Slerp(transform.localRotation, Reference.localRotation, Time.fixedDeltaTime * Settings.RotationAlignSpeed);
        transform.localRotation = Quaternion.RotateTowards(lastRotation, newRotation, Time.fixedDeltaTime * Settings.MaxRotationAlignSpeed);

        if (CurrentBody != null)
        {
            CurrentBody.velocity = Vector3.zero;
            CurrentBody.angularVelocity = Vector3.zero;
        }
    }
}
