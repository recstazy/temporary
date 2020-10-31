using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.AniPhysics
{
    [System.Serializable]
    public class StabSettings
    {
        [SerializeField]
        private bool positionStab = true;

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
        private bool rotationStab = true;

        [SerializeField]
        private float rotationStabSpeed = 3f;

        [SerializeField]
        [Range(0f, 1f)]
        private float gravityCompensation = 1f;

        public bool PositionStab { get => positionStab; set => positionStab = value; }
        public float Attraction { get => GetBlend(0f, attraction); set => attraction = value; }
        public float MaxForce { get => GetBlend(0f, maxForce); set => maxForce = value; }
        public float DistanceProportion { get => GetBlend(0f, distanceProportion); set => distanceProportion = value; }
        public float MaxDrag { get => GetBlend(0f, maxDrag); set => maxDrag = value; }
        public float MaxAngularDrag { get => GetBlend(0.05f, maxAngularDrag); set => maxAngularDrag = value; }
        public bool RotationStab { get => rotationStab; set => rotationStab = value; }
        public float RotationStabSpeed { get => GetBlend(0f, rotationStabSpeed); set => rotationStabSpeed = value; }
        public StabEffector Effector { get; set; }
        public float GravityCompensation { get => gravityCompensation; set => gravityCompensation = value; }

        public StabSettings(StabSettings source)
        {
            PositionStab = source.positionStab;
            Attraction = source.attraction;
            MaxForce = source.maxForce;
            DistanceProportion = source.distanceProportion;
            MaxDrag = source.maxDrag;
            MaxAngularDrag = source.maxAngularDrag;
            RotationStab = source.rotationStab;
            RotationStabSpeed = source.rotationStabSpeed;
            GravityCompensation = source.gravityCompensation;
        }

        private float GetBlend(float min, float max)
        {
            float effect = Effector != null ? Effector.Effect : 1f;
            return Mathf.Lerp(min, max, effect);
        }
    }

    [CreateAssetMenu(fileName = "NewStabJointSettings", menuName = "Animation Blending/Stab Joint Settings", order = 131)]
    public class StabJointSettingsAsset : ScriptableObject
    {
        [SerializeField]
        private StabSettings settings;

        public StabSettings Settings => settings;
    }
}
