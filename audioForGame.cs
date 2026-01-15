// MusicSound.cs
// Handles player movement audio, jumping sounds, combo detection, enemy fall events,
// win/game-over states, and scene resets. This script manages all sound effects and
// related gameplay triggers for the player and enemy interactions in the game.
// Used in Unity
// Author: Navid Nafeie

using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicSound : MonoBehaviour
{
    /* =======================
     * Player Movement
     * ======================= */
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private Vector3 lastPosition;
    private int jumpCount = 0; // Allows double jump (max 2)

    /* =======================
     * Audio
     * ======================= */
    private AudioSource audioSource;

    public AudioClip movementSound;
    public AudioClip jumpingSound1;
    public AudioClip jumpingSound2;
    public AudioClip landingSound;
    public AudioClip fallingSound;
    public AudioClip gameOverSound;
    public AudioClip collisionSound;
    public AudioClip comboSound;
    public AudioClip summonSound;
    public AudioClip enemyFallingSound;
    public AudioClip winSound;

    /* =======================
     * Game State
     * ======================= */
    public bool isGameOver = false;
    public float fallingThreshold = -11f;

    /* =======================
     * Combo System
     * ======================= */
    private int comboCounter = 0;
    private float comboTimer = 0f;
    public float comboReset = 1.5f;

    /* =======================
     * Enemy Tracking
     * ======================= */
    public GameObject enemyCube;
    private Vector3 enemyStartPosition;
    public float enemyFallThreshold = -11f;
    private bool enemyFallSoundPlayed = false;

    /* =======================
     * Unity Lifecycle
     * ======================= */
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        lastPosition = transform.position;

        if (enemyCube != null)
            enemyStartPosition = enemyCube.transform.position;
    }

    void Update()
    {
        if (isGameOver) return;

        HandleMovement();
        HandleJumping();
        HandleFalling();
        HandleComboTimer();
        CheckEnemyFalling();
    }

    /* =======================
     * Player Controls
     * ======================= */
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (transform.position != lastPosition && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(movementSound);
            lastPosition = transform.position;
        }
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            audioSource.PlayOneShot(jumpCount == 0 ? jumpingSound1 : jumpingSound2);
            jumpCount++;
        }
    }

    private void HandleFalling()
    {
        if (transform.position.y < fallingThreshold)
        {
            audioSource.PlayOneShot(fallingSound);

            if (!isGameOver)
                GameOver();
        }
    }

    /* =======================
     * Combo System
     * ======================= */
    private void HandleComboTimer()
    {
        if (comboCounter <= 0) return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
            comboCounter = 0;
    }

    private void ComboHit()
    {
        comboCounter++;
        comboTimer = comboReset;

        if (comboCounter >= 2)
            audioSource.PlayOneShot(comboSound);
    }

    /* =======================
     * Enemy Logic
     * ======================= */
    private void CheckEnemyFalling()
    {
        if (enemyCube == null || enemyFallSoundPlayed) return;

        if (enemyCube.transform.position.y < enemyFallThreshold)
        {
            audioSource.PlayOneShot(enemyFallingSound);
            Invoke(nameof(aWin), enemyFallingSound.length);
            Invoke(nameof(SummonEnemy), enemyFallingSound.length + 0.5f);

            enemyFallSoundPlayed = true;
        }
    }

    private void SummonEnemy()
    {
        enemyCube.transform.position = enemyStartPosition;
        enemyCube.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

        audioSource.PlayOneShot(summonSound);
        enemyFallSoundPlayed = false;
    }

    /* =======================
     * Game Over
     * ======================= */
    private void GameOver()
    {
        isGameOver = true;

        audioSource.PlayOneShot(gameOverSound);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Invoke(nameof(ReloadScene), 2.5f);
        Invoke(nameof(PlaySummonSound), 2.5f);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void PlaySummonSound()
    {
        audioSource.PlayOneShot(summonSound);
    }

    public void aWin()
    {
        audioSource.PlayOneShot(winSound);
    }

    /* =======================
     * Collisions
     * ======================= */
    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSound != null)
            audioSource.PlayOneShot(collisionSound);

        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            audioSource.PlayOneShot(landingSound);
        }

        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("ComboTarget"))
        {
            ComboHit();
        }
    }
}
