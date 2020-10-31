using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GameOn.UnityHelpers;

public class AIBase : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private NavMeshAgent navAgent;

    #endregion

    #region Properties

    #endregion

    public void Move(Vector3 destination)
    {
        navAgent?.SetDestination(destination);
    }

    private void PathCalculated(NavMeshPath path, Vector3 destination)
    {
        var positions = new Vector3[path.corners.Length + 2];

        positions[0] = transform.parent.position;

        for (int i = 0; i < path.corners.Length; i++)
        {
            positions[i + 1] = path.corners[i];
        }

        positions[positions.Length - 1] = destination;
    }
}
