using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.VRPhysics
{
    public class StabJoint : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private Rigidbody connectedBody;

        [SerializeField]
        private bool useLocalPositions = false;

        [SerializeField]
        private StabSettings settings;

        private Vector3[] angularVelocities = new Vector3[3];

        #endregion

        #region Properties

        public bool IsGrabbing => CurrentBody != null;
        public Rigidbody CurrentBody { get; private set; }
        public PhysicsLock PhysicsLock { get; private set; }
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

                    float targetDrag = Mathf.Lerp(Settings.MaxDrag, PhysicsLock.StartDrag, alpha);
                    float targetAngularDrag = Mathf.Lerp(Settings.MaxAngularDrag, PhysicsLock.StartAngularDrag, alpha);

                    if (PhysicsLock.ConnectionsCount == 1)
                    {
                        PhysicsLock.CurrentDrag = targetDrag;
                        PhysicsLock.CurrentAngularDrag = targetAngularDrag;
                    }
                    else
                    {
                        PhysicsLock.CurrentDrag = Mathf.Max(targetDrag, PhysicsLock.CurrentDrag);
                        PhysicsLock.CurrentAngularDrag = Mathf.Max(targetAngularDrag, PhysicsLock.CurrentAngularDrag);
                    }

                    CurrentBody.drag = PhysicsLock.CurrentDrag;
                    CurrentBody.angularDrag = PhysicsLock.CurrentAngularDrag;
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
                }

                if (Settings.RotationStab)
                {
                    if (useLocalPositions)
                    {
                        var startRotation = CurrentBody.rotation.eulerAngles;
                        alpha = Time.fixedDeltaTime * Settings.RotationStabSpeed;
                        CurrentBody.angularVelocity = Vector3.Lerp(CurrentBody.angularVelocity, Vector3.zero, alpha);

                        Quaternion rotation = Quaternion.Inverse(CurrentBody.transform.localRotation) * Quaternion.RotateTowards(CurrentBody.transform.localRotation, transform.localRotation, 30f);
                        rotation = CurrentBody.rotation * rotation;
                        CurrentBody.MoveRotation(Quaternion.Slerp(CurrentBody.rotation, rotation, alpha));
                        WriteAngularVelocity(CurrentBody.rotation.eulerAngles - startRotation);
                    }
                    else
                    {
                        var startRotation = CurrentBody.rotation.eulerAngles;
                        alpha = Time.fixedDeltaTime * Settings.RotationStabSpeed;
                        CurrentBody.angularVelocity = Vector3.Lerp(CurrentBody.angularVelocity, Vector3.zero, alpha);

                        Quaternion rotation = Quaternion.RotateTowards(CurrentBody.rotation, transform.rotation, 30f);
                        CurrentBody.MoveRotation(Quaternion.Slerp(CurrentBody.rotation, rotation, alpha));
                        WriteAngularVelocity(CurrentBody.rotation.eulerAngles - startRotation);
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
                CurrentBody.drag = PhysicsLock.StartDrag;
                CurrentBody.angularDrag = PhysicsLock.StartAngularDrag;
                PhysicsLock.ConnectionsCount--;

                if (PhysicsLock.ConnectionsCount <= 0)
                {
                    if (Settings.RotationStab)
                    {
                        CurrentBody.AddTorque(GetAngularvelocity(), ForceMode.VelocityChange);
                    }

                    Destroy(PhysicsLock);
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
                PhysicsLock = CurrentBody.GetComponent<PhysicsLock>();

                if (PhysicsLock == null)
                {
                    PhysicsLock = CurrentBody.gameObject.AddComponent<PhysicsLock>();
                    PhysicsLock.StartDrag = CurrentBody.drag;
                    PhysicsLock.StartAngularDrag = CurrentBody.angularDrag;
                }

                PhysicsLock.ConnectionsCount++;

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
