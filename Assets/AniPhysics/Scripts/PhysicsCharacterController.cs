using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.AniPhysics
{
    public class PhysicsCharacterController : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Rigidbody body;

        #endregion

        #region Properties

        #endregion

        public void StandingChanged(bool isStanding)
        {
            animator.SetBool("Standing", isStanding);
        }

        private void FixedUpdate()
        {
            var forwardDot = Mathf.Clamp(Vector3.Dot(transform.forward, body.velocity), -1f, 1f);
            var rightDot = Mathf.Clamp(Vector3.Dot(transform.right, body.velocity), -1f, 1f);

            animator.SetFloat("DirectionX", rightDot);
            animator.SetFloat("DirectionZ", forwardDot);
        }
    }
}
