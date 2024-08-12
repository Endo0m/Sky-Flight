using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;

public class FortuneWheelManager : MonoBehaviour
{
    public GameObject fortuneWheelPanel;
    public Button spinButton;
    public Button getButton;
    public TextMeshProUGUI getButtonText;
    public TMP_SpriteAsset ticketSpriteAsset;
    public Transform wheelTransform;

    public float spinDuration = 4f;
    public AnimationCurve spinCurve;
    public AudioSource wheelSfxSource;
    private int currentReward;
    private bool isSpinning = false;
    private AudioSource wheelSpinAudioSource;
    [System.Serializable]
    public class WheelSection
    {
        public string name;
        public int reward;
        public float probability;
    }

    public WheelSection[] wheelSections = new WheelSection[]
    {
        new WheelSection { name = "Plane", reward = 1000, probability = 0.01f },
        new WheelSection { name = "Bomb", reward = 0, probability = 0.22f },
        new WheelSection { name = "Plane", reward = 1000, probability = 0.01f },
        new WheelSection { name = "Cherry", reward = 200, probability = 0.22f },
        new WheelSection { name = "Plane", reward = 1000, probability = 0.01f },
        new WheelSection { name = "Gem", reward = 100, probability = 0.09f },
        new WheelSection { name = "Plane", reward = 1000, probability = 0.01f },
        new WheelSection { name = "Clover", reward = 400, probability = 0.22f }
    };

    private void Start()
    {
        spinButton.onClick.AddListener(StartSpin);
        getButton.onClick.AddListener(ClaimReward);
        getButton.gameObject.SetActive(false);
        wheelSpinAudioSource = gameObject.AddComponent<AudioSource>();
        wheelSpinAudioSource.loop = true;
    }

    public void ShowFortuneWheel()
    {
        fortuneWheelPanel.SetActive(true);
        spinButton.gameObject.SetActive(true);
        getButton.gameObject.SetActive(false);
        wheelTransform.rotation = Quaternion.identity;
        GameManager.Instance.PauseGame();
    }

    private void StartSpin()
    {
        if (!isSpinning)
        {
            Debug.Log("Starting spin");
            spinButton.gameObject.SetActive(false);
            StartCoroutine(SpinWheel());
            AudioManager.Instance.PlaySound("WheelSpin", wheelSfxSource);
            wheelSpinAudioSource.Play();
        }
    }

    private IEnumerator SpinWheel()
    {
        isSpinning = true;
        float elapsedTime = 0f;
        float startRotation = wheelTransform.eulerAngles.z;
        float endRotation = GetRandomWeightedAngle();
        float totalRotation = endRotation + 360f * Random.Range(2, 5);

        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;  // Use unscaledDeltaTime because the game is paused
            float t = elapsedTime / spinDuration;
            float curveValue = spinCurve.Evaluate(t);
            float currentRotation = Mathf.Lerp(startRotation, startRotation + totalRotation, curveValue);
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }

        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endRotation);
        GetReward();
        isSpinning = false;
    }

    private float GetRandomWeightedAngle()
    {
        float totalProbability = wheelSections.Sum(section => section.probability);
        float randomValue = Random.Range(0f, totalProbability);
        float currentSum = 0f;

        for (int i = 0; i < wheelSections.Length; i++)
        {
            currentSum += wheelSections[i].probability;
            if (randomValue <= currentSum)
            {
                return i * (360f / wheelSections.Length);
            }
        }

        return 0f;
    }

    private void GetReward()
    {
        float rot = wheelTransform.eulerAngles.z;
        int sectionIndex = Mathf.FloorToInt(rot / (360f / wheelSections.Length));
        sectionIndex = Mathf.Clamp(sectionIndex, 0, wheelSections.Length - 1);

        WheelSection winningSection = wheelSections[sectionIndex];
        currentReward = winningSection.reward;
        getButtonText.text = $"GET {currentReward} <sprite=\"{ticketSpriteAsset.name}\" index=0>";
        getButton.gameObject.SetActive(true);
        wheelSpinAudioSource.Stop();
        AudioManager.Instance.StopSound(wheelSfxSource);
        AudioManager.Instance.PlaySound(currentReward > 0 ? "WheelWin" : "WheelLose", wheelSfxSource);
    }

    private void ClaimReward()
    {
        GameManager.Instance.AddTickets(currentReward);
        fortuneWheelPanel.SetActive(false);
        GameManager.Instance.ResumeGame();

    }
}