using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDetection
{
    public static bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current); // We create a new event data for get the tocuh position
        eventData.position = Input.mousePosition; // we dont need to use touch position, because unity converts automatically for us
        List<RaycastResult> results = new List<RaycastResult>(); // we make a list of raycast results for store the results
        EventSystem.current.RaycastAll(eventData, results); // Ui make a raycast for check if the pointer is over a UI element

        return results.Count > 0; 
    }

    public static bool IsPointerOverThisObject(GameObject gameObject)
    {

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

        List<RaycastResult> resultsList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultsList);

        foreach (RaycastResult raycastResult in resultsList)
        {
            if (raycastResult.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
