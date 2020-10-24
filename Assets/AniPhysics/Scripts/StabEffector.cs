using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.AniPhysics
{
    [System.Serializable]
    public class StabEffector
    {
        #region Fields

        [SerializeField]
        [Range(0f, 1f)]
        private float effect = 1f;

        #endregion

        #region Properties

        public float Effect { get => effect; set => effect = value; }

        #endregion
    }
}
