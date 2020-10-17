using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStabJointSettings", menuName = "Animation Blending/Stab Joint Settings", order = 131)]
public class StabJointSettings : ScriptableObject
{
    [SerializeField]
    private float attraction = 300f;

    [SerializeField]
    private float maxForce = 10000f;

    [SerializeField]
    [Range(0f, 10f)]
    private float distanceProportion = 1f;

    [SerializeField]
    [Range(0f, 30f)]
    private float maxDrag = 15f;

    [SerializeField]
    [Range(0f, 2f)]
    private float maxAngularDrag = 0.8f;

    [SerializeField]
    private bool rotationStab = false;

    [SerializeField]
    [Range(0f, 20f)]
    private float rotationStabSpeed = 3f;

    public float Attraction { get => attraction; set => attraction = value; }
    public float MaxForce { get => maxForce; set => maxForce = value; }
    public float DistanceProportion { get => distanceProportion; set => distanceProportion = value; }
    public float MaxDrag { get => maxDrag; set => maxDrag = value; }
    public float MaxAngularDrag { get => maxAngularDrag; set => maxAngularDrag = value; }
    public bool RotationStab { get => rotationStab; set => rotationStab = value; }
    public float RotationStabSpeed { get => rotationStabSpeed; set => rotationStabSpeed = value; }
}
