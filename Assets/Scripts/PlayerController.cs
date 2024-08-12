using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float boundaryX = 1.5f;

    public Button leftButton;
    public Button rightButton;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private AudioSource engineAudioSource;
    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition;

        leftButton.onClick.AddListener(MoveLeft);
        rightButton.onClick.AddListener(MoveRight);

        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.loop = true;
        engineAudioSource.Play();
    }

    void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Playing)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
       

    }

    public void MoveLeft()
    {
        targetPosition = new Vector3(-boundaryX, transform.position.y, transform.position.z);
        AudioManager.Instance.PlaySound("AcceleratedEngine", engineAudioSource);
    }

    public void MoveRight()
    {
        targetPosition = new Vector3(boundaryX, transform.position.y, transform.position.z);
        AudioManager.Instance.PlaySound("AcceleratedEngine", engineAudioSource);
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
        targetPosition = initialPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}