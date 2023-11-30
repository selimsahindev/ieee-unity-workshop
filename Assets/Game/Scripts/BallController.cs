using UnityEngine;

public class BallController : MonoBehaviour
{
    public float MoveSpeed = 7.5f;
    public float RotateSpeed = 6f;
    public float MinY = 0.35f;
    public float MaxY = 0.9f;

    public Rigidbody rb;
    public Collider col;

    private bool isDribbling = false;
    private float time = 0f;

    private void Update()
    {
        if (isDribbling)
        {
            time += Time.deltaTime * MoveSpeed;

            float sin = Mathf.Abs(Mathf.Sin(time)); // Sinüs fonksiyonu mutlak değeri

            // Top sektirme izlenimini vermek için MaxY ve MinY arasında topu
            // hareket ettiriyoruz. (Bkz: Sinüs dalgasının şeklini inceleyiniz).
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                MaxY - sin * (MaxY - MinY),
                transform.localPosition.z
            );

            // Topun hangi yönlere rastgele rotate edileceğini gösteren vektör.
            Vector3 rotationVector = new Vector3(
                Random.Range(0f, 1f),   // x
                Random.Range(-1f, 1f),  // y
                0f                      // z
            );

            float rotationMagnitude = RotateSpeed * Time.deltaTime * 10f;

            // Topu kendi x ekseninde rotasyon vektörü ve büyüklüğü kadar döndürür.
            transform.Rotate(rotationVector * rotationMagnitude, Space.Self);
        }

        // Top aşağı düşerse tekrar platformun ortasında spawn et.
        if (transform.position.y < -5f)
        {
            transform.position = Vector3.up * 0.365f;
            rb.velocity = Vector3.zero;
        }
    }

    // Topu sektirmeye başlamak için.
    public void StartDribble()
    {
        isDribbling = true;
    }

    // Topu sektirmeyi durdurmak için.
    public void StopDribble()
    {
        isDribbling = false;
        time = 0f;
    }
}
