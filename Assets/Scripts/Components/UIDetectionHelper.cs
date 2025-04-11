using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDetectionHelper : MonoBehaviour
{

    public bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current); // We create a new event data for get the tocuh position
        eventData.position = Input.mousePosition; // we dont need to use touch position, because unity converts automatically for us
        List<RaycastResult> results = new List<RaycastResult>(); // we make a list of raycast results for store the results
        EventSystem.current.RaycastAll(eventData, results); // Ui make a raycast for check if the pointer is over a UI element

        return results.Count > 0; 
    }
}
