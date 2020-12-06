using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.AniPhysics
{
    public class BoneAttractor : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private Rigidbody connectedBody;

        [SerializeField]
        private bool useLocalPositions = false;

        [SerializeField]
        private StabSettings settings;

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
                Vector3 distanceVector;

                distanceVector = transform.position - CurrentBody.position;
                var distanceSquared = distanceVector.sqrMagnitude;
                distanceSquared *= Settings.DistanceProportion;

                var alpha = distanceSquared / 100f;
                float targetDrag = Mathf.Lerp(Settings.MaxDrag, startDrag, alpha);
                float targetAngularDrag = Mathf.Lerp(Settings.MaxAngularDrag, startAngularDrag, alpha);
                CurrentBody.drag = targetDrag;
                CurrentBody.angularDrag = targetAngularDrag;

                if (Settings.PositionStab)
                {
                    var force = distanceVector.normalized * Settings.Attraction * distanceSquared;
                    force = force.normalized * Mathf.Clamp(force.magnitude, -Settings.MaxForce, Settings.MaxForce);
                    CurrentBody.AddForce(force * Time.fixedDeltaTime, ForceMode.Force);
                    CurrentBody.AddForce(-Physics.gravity * CurrentBody.mass * settings.GravityCompensation * Settings.Effector.Effect, ForceMode.Force);
                }

                if (Settings.RotationStab)
                {
                    alpha = Time.fixedDeltaTime * Settings.RotationStabSpeed;
                    CurrentBody.angularVelocity = Vector3.Lerp(CurrentBody.angularVelocity, Vector3.zero, alpha);
                    Quaternion rotation = Quaternion.RotateTowards(CurrentBody.rotation, transform.rotation, 30f);
                    CurrentBody.MoveRotation(Quaternion.Slerp(CurrentBody.rotation, rotation, alpha));
                }
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
    }
}
