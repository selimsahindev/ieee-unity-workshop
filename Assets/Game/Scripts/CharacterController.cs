using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float JumpMagnitude = 5f;

    public Rigidbody rb;
    public Animator animator;

    public BallController Ball;
    public Transform BallPosition;
    public Transform ShootPosition;
    public Transform Target;

    private bool isOnGround = false;
    private bool isBallInHands = false;
    private bool isBallFlying = false;
    private float time = 0f;
    private Vector3 randomShootVector;

    private void Update()
    {
        // Kullanıcı inputunu bir değişkene atıyorum.
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        // Yerde değilsek inputu sıfırlayalım. Bu sayede havadayken hem hareket
        // etmeyeceğiz hem de yönümüz ve animasyonumuz değişmeyecek.
        if (!isOnGround)
        {
            input = Vector3.zero;
        }

        // deltaTime ile çarparak FPS'ten bağımsız hale getirdim.
        transform.position += input * Time.deltaTime * MoveSpeed;

        // Animator'deki Movement parametresinin değerini güncelle.
        animator.SetFloat("Movement", input.magnitude);

        // Karakterin baktığı yönü güncelle.
        if (input.magnitude > 0f)
        {
            HandleRotation(input);
        }

        // Space'e basınca karakter zıplasın.
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            Jump();
        }

        // Space'i bırakınca topu fırlatalım.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Top elimizdeyse topu fırlat.
            if (isBallInHands)
            {
                ShootTheBall();
            }

            // Atış animasyonuna ihtiyacımız kalmadı.
            animator.SetBool("IsShooting", false);
        }

        // Platformdan düşersek platformun ortasında spawn olalım.
        if (transform.position.y < -5f)
        {
            transform.position = Vector3.zero;
        }

        if (isBallFlying)
        {
            HandleBallFly();
        }
    }

    private void HandleRotation(Vector3 input)
    {
        transform.rotation = Quaternion.LookRotation(input);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * JumpMagnitude, ForceMode.VelocityChange);

        if (isBallInHands)
        {
            GetReadyToShoot();
        }

        isOnGround = false; // Artık yerde değiliz!
    }

    private void GetReadyToShoot()
    {
        // Top sektirme hareketine son ver.
        Ball.StopDribble();

        // Topun parent transformunu atış konumunu belirten ShootPosition
        // objesine ayarla, yereldeki konumunu ve yereldeki açılarını sıfırla.
        Ball.transform.parent = ShootPosition;
        Ball.transform.localPosition = Vector3.zero;
        Ball.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // Animator "IsShooting" parametresini günceller,
        // bu sayede "Shooting" animasyonun dosyasını oynatır.
        animator.SetBool("IsShooting", true);

        // Zıpladığında potaya doğru bak!
        transform.LookAt(new Vector3(Target.position.x, 0f, Target.position.z));
    }

    private void ShootTheBall()
    {
        // Topu bizim child objemiz olmaktan kurtar! Yoksa bizle birlikte
        // harelet etmeye devam edecek!
        Ball.transform.parent = null;

        isBallInHands = false;
        isBallFlying = true;

        time = 0f; // Topun havada olduğu zamanı sıfırladım.

        // Topu forlatırken rastgele bir miktar hedefi ıskalama payı ekle.
        randomShootVector = new Vector3(Random.Range(-0.75f, 0.75f), 0f, Random.Range(-0.75f, 0.5f));
    }

    private void HandleBallFly()
    {
        time += Time.deltaTime;
        float duration = 0.5f;
        float tN = time / duration;

        Vector3 A = ShootPosition.position;
        Vector3 B = Target.position + randomShootVector;
        Vector3 pos = Vector3.Lerp(A, B, tN); // (bkz: lineer interpolasyon)

        // Topu yay çizerek ilerletelim.
        Vector3 arch = Vector3.up * 1.8f * Mathf.Sin(tN * Mathf.PI);

        Ball.transform.position = pos + arch;

        // Top hedefe ulaştığında yapacağı hareket bu kısımda:
        if (tN >= 1)
        {
            isBallFlying = false;

            // Topun komponentlerini sıfırladım.
            Ball.col.enabled = true;
            Ball.rb.isKinematic = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // "Ground" etiketine sahip bir objeye çarptığında:
        if (collision.collider.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // "Ball" etiketine sahip, isTrigger seçili bir collider'a temas ettiğinde:
        if (other.CompareTag("Ball"))
        {
            isBallInHands = true;

            // Topun Rigidbody komponentini kinematik olarak ayarla.
            // Bu sayede yer çekimi ve diğer kuvvetlerden etkilenmeyecektir.
            Ball.rb.isKinematic = true;

            // Topu tekrar tekrar toplamaması için collider komponentini
            // geçici olarak kapatıyoruz.
            Ball.col.enabled = false;

            // Topun parent komponentini BallPosition objemiz yapıyoruz.
            Ball.transform.parent = BallPosition;

            // Topu BallContainer objesinin biraz yukarısına merkezliyoruz.
            Ball.transform.localPosition = Vector3.zero + Vector3.up * 0.9f;

            // Top sektirme hareketini başlatır.
            Ball.StartDribble();
        }
    }
}
