using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegarObjeto : MonoBehaviour
{
    private Transform draggedObject;
    private Vector3 offset;
    public float launchForce = 10f;
    void Update()
    {
        // Check for mouse input
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("PRESS");
            // Raycast to find object to drag
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if object is draggable
                if (hit.collider.gameObject.tag == "Beyblade")
                {
                    // Hold object
                    draggedObject = hit.collider.gameObject.transform;
                    offset = draggedObject.position - hit.point;
                    Debug.Log(draggedObject);
                }
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            // Release object
            if (draggedObject != null)
            {
                Vector3 forceDirection = Camera.main.transform.forward * launchForce;
                Rigidbody rb = draggedObject.GetComponent<Rigidbody>();
                rb.AddForce(forceDirection, ForceMode.Impulse);
            }

            draggedObject = null;
        }

        // Move dragged object with mouse
        if (draggedObject != null)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            Vector3 objectPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            draggedObject.position = objectPosition;
        }
    }
}