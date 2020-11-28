using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAnimationSettings : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private float positionAlignSpeed;

    [SerializeField]
    private float rotationAlignSpeed;

    [SerializeField]
    private float maxPositionAlignSpeed;

    [SerializeField]
    private float maxRotationAlignSpeed;

    #endregion

    #region Properties

    public float PositionAlignSpeed { get => positionAlignSpeed; set => positionAlignSpeed = value; }
    public float RotationAlignSpeed { get => rotationAlignSpeed; set => rotationAlignSpeed = value; }
    public float MaxPositionAlignSpeed { get => maxPositionAlignSpeed; set => maxPositionAlignSpeed = value; }
    public float MaxRotationAlignSpeed { get => maxRotationAlignSpeed; set => maxRotationAlignSpeed = value; }

    #endregion
}
