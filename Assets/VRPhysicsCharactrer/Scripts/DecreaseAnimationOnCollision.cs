using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameOn.UnityHelpers;

namespace Recstazy.VRPhysics
{
    public class DecreaseAnimationOnCollision : MonoBehaviour
    {
        [SerializeField]
        private float duration = 1f;

        [SerializeField]
        private float effect = 0.1f;

        private PhysicsAnimationBlender blender;
        private float defaultEffect = 1f;
        private Coroutine effectRoutine;

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

        private void OnCollisionEnter(Collision collision)
        {
            if (effectRoutine != null)
            {
                StopCoroutine(effectRoutine);
            }

            effectRoutine = StartCoroutine(EffectRoutine());
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
            float t = 0f;

            while (t <= 1f)
            {
                blender.Effector.Effect = Mathf.Lerp(effect, defaultEffect, t);

                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime / duration;
            }

            blender.Effector.Effect = defaultEffect;
            effectRoutine = null;
        }
    }
}
