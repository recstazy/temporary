using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToMoveCharacter : MonoBehaviour, IPointerClickHandler
{
    #region Fields

    [SerializeField]
    private AIBase ai;

    [SerializeField]
    private LayerMask layerMask;

    #endregion

    #region Properties

    #endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        var ray = Camera.main.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition);
        RaycastHit hit;
        bool wasHit = Physics.Raycast(ray, out hit, 100f, layerMask, QueryTriggerInteraction.Ignore);

        if (wasHit)
        {
            ai.Move(hit.point);
        }
    }
}
