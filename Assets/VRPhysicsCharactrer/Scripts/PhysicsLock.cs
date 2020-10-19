using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsLock : MonoBehaviour
{
    #region Fields
    
    #endregion

    #region Properties
    
    public float StartDrag { get; set; }
    public float StartAngularDrag { get; set; }
    public int ConnectionsCount { get; set; }
    public bool UsedGravity { get; set; }

    public float CurrentDrag { get; set; }
    public float CurrentAngularDrag { get; set; }

    #endregion
}
