using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float minJumpForce = 5f;
    public float maxJumpForce = 15f;
    public float maxChargeTime = 1f;
    public float fallThreshold = 5f;
    public float bounceForce = 10f;
    private float jumpChargeTime;
    private bool isChargingJump;
    private bool isJumping;
    private bool isWalking;
    private bool isWallHit;
    private bool isFallingFromHigh;
    private bool isGrounded;
    private float fallStartY;
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    // Sonidos
    public AudioClip jumpSound;
    public AudioClip fallSound;
    public AudioClip wallHitSound;
    public AudioClip highFallSound;

    private float jumpDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");

        // Si el jugador decide moverse, se desactiva la animaci�n de caer desde una altura
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            isFallingFromHigh = false;
            animator.SetBool("isFallingFromHigh", false);
        }

        // Movimiento horizontal solo si no est� cargando el salto y est� en el suelo
        if (!isChargingJump && isGrounded && !isFallingFromHigh)
        {
            rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
            isWalking = Mathf.Abs(moveInput) > 0.1f;
            animator.SetBool("isWalking", isWalking);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Detener el movimiento mientras carga el salto o cae
        }

        // Mirar al personaje en la direcci�n correcta si no est� cargando el salto
        if (!isChargingJump && !isFallingFromHigh && moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }

        // Cargar el salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isChargingJump = true;
            jumpChargeTime = 0f;
            animator.SetBool("isChargingJump", true);

            // Determinar la direcci�n del salto
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                jumpDirection = -1f;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                jumpDirection = 1f;
            }
            else
            {
                jumpDirection = 0f;
            }

            fallStartY = transform.position.y; // Registrar la altura desde donde se empieza a caer
        }

        // Mantener la carga del salto
        if (Input.GetKey(KeyCode.Space) && isChargingJump)
        {
            jumpChargeTime += Time.deltaTime;
            jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0f, maxChargeTime); // Limitar el tiempo de carga
        }

        // Saltar cuando se suelta la tecla
        if (Input.GetKeyUp(KeyCode.Space) && isChargingJump)
        {
            isChargingJump = false;
            float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpChargeTime / maxChargeTime);
            rb.velocity = new Vector2(jumpDirection * walkSpeed, jumpForce);
            animator.SetBool("isChargingJump", false);
            animator.SetBool("isJumping", true);
            isJumping = true;
            isWallHit = false; // Reiniciar la animaci�n de chocar con la pared
            PlaySound(jumpSound); // Reproducir sonido de salto
        }

        // Prevenir que el personaje cambie de direcci�n en el aire (como Jump King)
        if (isJumping && !isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Detener el movimiento horizontal en el aire
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Aseg�rate de que el personaje est� tocando el suelo
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            isJumping = false;
            isWallHit = false; // Desactivar la animaci�n de chocar con la pared al tocar el suelo
            animator.SetBool("isJumping", false);
            animator.SetBool("isWallHit", false);

            // Verificar si se cay� desde una altura considerable
            if (fallStartY - transform.position.y > fallThreshold)
            {
                isFallingFromHigh = true;
                animator.SetBool("isFallingFromHigh", true);
                PlaySound(highFallSound); // Reproducir sonido de ca�da desde altura
            }
            else
            {
                PlaySound(fallSound); // Reproducir sonido de ca�da normal
            }
        }
        // Detectar colisi�n con una pared durante el salto
        else if (isJumping && collision.contacts.Length > 0 && Mathf.Abs(collision.contacts[0].normal.x) > 0.5f)
        {
            // Rebote al chocar con la pared
            rb.velocity = new Vector2(-rb.velocity.x * bounceForce, rb.velocity.y); // Cambiar la direcci�n horizontal
            isWallHit = true;
            animator.SetBool("isWallHit", true);
            PlaySound(wallHitSound); // Reproducir sonido de choque con la pared
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Verificar si el personaje dej� de tocar el suelo
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = false;
        }
    }

    // M�todo para reproducir el sonido
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);

        }
    }
}
