using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Recstazy.AniPhysics
{
    public class OnFallDispatcher : MonoBehaviour
    {
        [System.Serializable]
        public class FallEvent : UnityEvent<bool> { }

        #region Fields

        [SerializeField]
        private FallEvent onStandUp;

        [SerializeField]
        private FallEvent onFall;

        private FallOnCollision fallOnCollision;

        #endregion

        #region Properties

        #endregion

        private void Awake()
        {
            fallOnCollision = GetComponent<FallOnCollision>();

            if (fallOnCollision != null)
            {
                fallOnCollision.OnStandingChanged += StandingChanged;
            }
        }

        private void OnDestroy()
        {
            fallOnCollision.OnStandingChanged -= StandingChanged;
        }

        private void StandingChanged(bool isStanding)
        {
            onStandUp?.Invoke(isStanding);
            onFall?.Invoke(!isStanding);
        }
    }
}
