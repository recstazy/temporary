using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.VRPhysics
{
    public class PhysicsAnimationBlender : MonoBehaviour
    {
        [System.Serializable]
        public class TransformTouple
        {
            public Transform Reference;
            public Transform Target;
        }

        #region Fields

        [SerializeField]
        private Transform referenceRootBone;

        [SerializeField]
        private Transform targetRootBone;

        [SerializeField]
        private StabJointSettingsAsset defaultSettings;

        [SerializeField]
        private StabEffector effectBlend;

        [SerializeField]
        private TransformTouple[] touples;

        #endregion

        #region Properties

        public StabJointSettingsAsset Settings => defaultSettings;
        public List<Rigidbody> Bodies { get; private set; } = new List<Rigidbody>();
        public StabEffector Effector => effectBlend;

        #endregion

        private void Awake()
        {
            SetupBodies(touples);
        }

        private void SetupBodies(TransformTouple[] touples)
        {
            foreach (var t in touples)
            {
                var stab = t.Reference.gameObject.AddComponent<StabJoint>();
                var body = t.Target.GetComponent<Rigidbody>();

                if (body == null)
                {
                    body = t.Target.gameObject.AddComponent<Rigidbody>();
                }

                Bodies.Add(body);

                stab.SetAttachedBody(body);
                stab.Settings = new StabSettings(defaultSettings.Settings);
                stab.Settings.Effector = effectBlend;
            }
        }
    }
}
