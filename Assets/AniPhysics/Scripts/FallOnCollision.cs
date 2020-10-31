using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameOn.UnityHelpers;

namespace Recstazy.AniPhysics
{
    public class FallOnCollision : MonoBehaviour
    {
        public event System.Action<bool> OnStandingChanged;

        [SerializeField]
        private float lieDownTime = 1f;

        [SerializeField]
        private float standUpTime = 1f;

        [SerializeField]
        private float fallTreshold = 5000f;

        [SerializeField]
        private float regenerationSpeed = 500f;

        [SerializeField]
        private float damage = 0f;

        private PhysicsAnimationBlender blender;
        private float defaultEffect = 1f;
        private Coroutine effectRoutine;

        public bool IsStanding { get; private set; } = true;

        private void Awake()
        {
            blender = GetComponent<PhysicsAnimationBlender>();
            defaultEffect = blender.Effector.Effect;
        }

        private void Start()
        {
            foreach (var b in blender.Bodies)
            {
                var del = b.gameObject.AddComponent<CollisionDelegate>();
                del.OnCollision.AddListener(OnCollision);
                del.Mode = GameOn.UnityHelpers.DelegateFireMode.OnEnter;
                del.LayerMask = LayerMask.GetMask("Default");
            }
        }

        private void Update()
        {
            damage = Mathf.Clamp(damage - 500f * Time.deltaTime, 0f, float.MaxValue);
        }

        private void OnCollisionEnter(Collision collision)
        {
            damage += collision.impulse.magnitude;

            if (damage >= fallTreshold)
            {
                IsStanding = false;
                OnStandingChanged?.Invoke(IsStanding);

                if (effectRoutine != null)
                {
                    StopCoroutine(effectRoutine);
                }

                effectRoutine = StartCoroutine(EffectRoutine());
            }
        }

        private void OnCollision(Collision collision, bool isEnter)
        {
            if (isEnter)
            {
                OnCollisionEnter(collision);
            }
        }

        private IEnumerator EffectRoutine()
        {
            blender.Effector.Effect = 0f;
            yield return new WaitForSeconds(lieDownTime);

            float t = 0f;

            while (t <= 1f)
            {
                blender.Effector.Effect = Mathf.Lerp(0f, defaultEffect, t);

                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime / standUpTime;
            }

            blender.Effector.Effect = defaultEffect;
            IsStanding = true;
            OnStandingChanged?.Invoke(IsStanding);
            effectRoutine = null;
        }
    }
}
