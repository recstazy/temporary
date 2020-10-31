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
        private NavMeshAgent navAgent;

        #endregion

        #region Properties

        #endregion

        public void StandingChanged(bool isStanding)
        {
            animator.SetBool("Standing", isStanding);

            if (isStanding)
            {
                navAgent.SetDestination(transform.position);
            }
        }

        private void FixedUpdate()
        {
            var forwardDot = Vector3.Dot(transform.forward, navAgent.velocity.normalized);
            var rightDot = Vector3.Dot(transform.right, navAgent.velocity.normalized);

            animator.SetFloat("DirectionX", rightDot);
            animator.SetFloat("DirectionZ", forwardDot);
        }
    }
}
