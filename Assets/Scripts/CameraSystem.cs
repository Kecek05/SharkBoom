using UnityEngine;

public class CameraSystem : MonoBehaviour
{

    private void Update()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        if(Input.GetKey(KeyCode.W)) moveDir.y = +1;
        if(Input.GetKey(KeyCode.S)) moveDir.y = -1;
        if(Input.GetKey(KeyCode.A)) moveDir.x = -1;
        if(Input.GetKey(KeyCode.D)) moveDir.x = +1;
        
        float moveSpeed = 10;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
