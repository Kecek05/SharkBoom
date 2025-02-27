using UnityEngine;

public class DragAndShoot : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 direction;

    private float forceMultiplier;
    private float maxForceMultiplier = 100;
    private bool isDragging = false;

    [SerializeField] private Transform spawnPos;
    [SerializeField] private Transform shperePos;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Trajectory trajectory;

    private Vector3 velocity = Vector3.zero; // Armazena a velocidade da suavização
    [SerializeField] private float smoothTime = 0.1f; // Quanto menor, mais rápido responde

    private void Update()
    {
        StartDrag();
        MouseDrag();
    }

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray rayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, out hit) && hit.collider.gameObject == this.gameObject)
            {
                startPos = hit.point; // Corrigido para pegar a posição real do clique no objeto
                isDragging = true;
                trajectory.Show();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            shperePos.position = spawnPos.position;
        }
    }

    private void MouseDrag()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.forward, startPos);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                // Suaviza o movimento do arrasto
                endPos = Vector3.SmoothDamp(endPos, ray.GetPoint(distance), ref velocity, smoothTime);

                direction = (startPos - endPos).normalized;
                forceMultiplier = Vector3.Distance(startPos, endPos);

                // Limita a força máxima
                forceMultiplier = Mathf.Clamp(forceMultiplier, 0, 0.15f);

                // Atualiza a trajetória em tempo real
                trajectory.UpdateDots(shperePos.position, direction * forceMultiplier * maxForceMultiplier);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ReleaseDrag();
        }
    }

    private void ReleaseDrag()
    {
        rb.AddForce(direction * forceMultiplier * maxForceMultiplier, ForceMode.Impulse);
        isDragging = false;
        trajectory.Hide();
    }
}
