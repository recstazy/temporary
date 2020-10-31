using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.AniPhysics
{
    public class BoneMotor : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private Rigidbody connectedBody;

        [SerializeField]
        private bool useLocalPositions = false;

        [SerializeField]
        private StabSettings settings;

        [SerializeField]
        private float forceMagnitude = 1f;

        private Vector3[] angularVelocities = new Vector3[3];

        private float startDrag = 0f;
        private float startAngularDrag = 0.05f;

        #endregion

        #region Properties

        public bool IsGrabbing => CurrentBody != null;
        public Rigidbody ConnectedBody => connectedBody;
        public Rigidbody CurrentBody { get; private set; }
        public StabSettings Settings
        {
            get => settings;
            set
            {
                if (value != null)
                {
                    settings = value;
                }
            }
        }

        #endregion

        private void FixedUpdate()
        {
            if (CurrentBody != connectedBody)
            {
                SetAttachedBody(connectedBody);
            }

            if (IsGrabbing)
            {

            }
        }

        public void SetAttachedBody(Rigidbody body)
        {
            if (body != null)
            {
                if (IsGrabbing)
                {
                    ReleaseBody();
                }

                AttachBody(body);
            }
            else
            {
                if (IsGrabbing)
                {
                    ReleaseBody();
                }
            }

            connectedBody = CurrentBody;
        }

        private void ReleaseBody()
        {
            if (CurrentBody != null)
            {
                CurrentBody.drag = startDrag;
                CurrentBody.angularDrag = startAngularDrag;

                if (Settings.RotationStab)
                {
                    CurrentBody.AddTorque(GetAngularvelocity(), ForceMode.VelocityChange);
                }

                ChangeDetectionModeOnSleep(CurrentBody, CollisionDetectionMode.Discrete);
                CurrentBody = null;
            }
        }

        private void AttachBody(Rigidbody body)
        {
            CurrentBody = body;

            if (CurrentBody != null)
            {
                startDrag = CurrentBody.drag;
                startAngularDrag = CurrentBody.angularDrag;
                CurrentBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }

        private void ChangeDetectionModeOnSleep(Rigidbody body, CollisionDetectionMode mode)
        {
            StartCoroutine(ChangeDetectionModeRoutine(body, mode));
        }

        private IEnumerator ChangeDetectionModeRoutine(Rigidbody body, CollisionDetectionMode mode)
        {
            yield return new WaitUntil(() => body.IsSleeping());
            body.collisionDetectionMode = mode;
        }

        private void WriteAngularVelocity(Vector3 velocity)
        {
            for (int i = 1; i < angularVelocities.Length; i++)
            {
                angularVelocities[i - 1] = angularVelocities[i];
            }

            angularVelocities[angularVelocities.Length - 1] = velocity;
        }

        private Vector3 GetAngularvelocity()
        {
            Vector3 result = default;

            foreach (var v in angularVelocities)
            {
                result += v;
            }

            result /= angularVelocities.Length;

            return result;
        }
    }
}
