using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.AniPhysics
{
    public class BoneAttractorOld : MonoBehaviour
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
                float alpha = 0f;

                if (Settings.PositionStab)
                {
                    Vector3 distanceVector;

                    if (useLocalPositions)
                    {
                        distanceVector = transform.localPosition - CurrentBody.transform.localPosition;
                    }
                    else
                    {
                        distanceVector = transform.position - CurrentBody.position;
                    }

                    var distanceSquared = distanceVector.sqrMagnitude;
                    distanceSquared *= Settings.DistanceProportion;

                    alpha = distanceSquared / 100f;

                    float targetDrag = Mathf.Lerp(Settings.MaxDrag, startDrag, alpha);
                    float targetAngularDrag = Mathf.Lerp(Settings.MaxAngularDrag, startAngularDrag, alpha);

                    CurrentBody.drag = targetDrag;
                    CurrentBody.angularDrag = targetAngularDrag;
                    var force = distanceVector.normalized * Settings.Attraction * distanceSquared;

                    if (force.magnitude > Settings.MaxForce)
                    {
                        force = force.normalized * Settings.MaxForce;
                    }

                    if (useLocalPositions)
                    {
                        CurrentBody.AddRelativeForce(force * Time.fixedDeltaTime, ForceMode.Force);
                    }
                    else
                    {
                        CurrentBody.AddForce(force * Time.fixedDeltaTime, ForceMode.Force);
                    }

                    CurrentBody.AddForce(-Physics.gravity * CurrentBody.mass * settings.GravityCompensation * Settings.Effector.Effect, ForceMode.Force);
                }

                if (Settings.RotationStab)
                {
                    if (useLocalPositions)
                    {
                        alpha = Time.fixedDeltaTime * Settings.RotationStabSpeed;
                        CurrentBody.angularVelocity = Vector3.Lerp(CurrentBody.angularVelocity, Vector3.zero, alpha);

                        Quaternion rotation = Quaternion.Inverse(CurrentBody.transform.localRotation) * Quaternion.RotateTowards(CurrentBody.transform.localRotation, transform.localRotation, 30f);
                        rotation = CurrentBody.rotation * rotation;
                        CurrentBody.MoveRotation(Quaternion.Slerp(CurrentBody.rotation, rotation, alpha));
                    }
                    else
                    {
                        alpha = Time.fixedDeltaTime * Settings.RotationStabSpeed;
                        CurrentBody.angularVelocity = Vector3.Lerp(CurrentBody.angularVelocity, Vector3.zero, alpha);

                        Quaternion rotation = Quaternion.RotateTowards(CurrentBody.rotation, transform.rotation, 30f);
                        CurrentBody.MoveRotation(Quaternion.Slerp(CurrentBody.rotation, rotation, alpha));
                    }
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
