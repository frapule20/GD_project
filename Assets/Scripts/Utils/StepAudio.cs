using UnityEngine;
using System.Collections;

public class StepAudio : MonoBehaviour 
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    
    [Header("Speed Thresholds")]
    [SerializeField] private float walkThreshold = 0.08f;
    [SerializeField] private float runThreshold = 0.6f;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.2f;   

    private float currentSpeed;
    private bool isPlayingWalk = false;
    private bool isPlayingRun = false;
    private bool isFading = false;
    private float originalVolume;
    private bool hasStealthParameter = false;
    
    public static bool PlayerCanMove = true;
    public static bool PlayerIsHidden = false;
    
    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (animator == null)
            animator = GetComponent<Animator>();

        originalVolume = 1.5f; 

        CheckForStealthParameter();
    }
    
    void CheckForStealthParameter()
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == "IsStealth" && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    hasStealthParameter = true;
                    Debug.Log("Parametro IsStealth trovato nell'Animator");
                    break;
                }
            }
        }
    }
    
    void Update() 
    {
        currentSpeed = animator.GetFloat("moveAmount");
        HandleFootstepAudio();
    }

    bool IsInStealthMode()
    {
        if (!hasStealthParameter)
            return false;
            
        return animator.GetBool("IsStealth");
    }

    void HandleFootstepAudio()
    {
        // Controlla tutte le condizioni che dovrebbero fermare l'audio
        if (PlayerIsHidden || !PlayerCanMove || IsInStealthMode())
        {
            if (audioSource.isPlaying && !isFading)
            {
                StartCoroutine(FadeOutAndStop());
            }
            return; // Esce dalla funzione
        }
        
        if (currentSpeed < walkThreshold)
        {
            if (audioSource.isPlaying && !isFading)
            {
                StartCoroutine(FadeOutAndStop());
            }
        }
        else if (currentSpeed < runThreshold)
        {
            if (!isPlayingWalk)
            {
                audioSource.clip = walkClip;
                audioSource.loop = true;
                audioSource.volume = originalVolume;
                audioSource.Play();
                isPlayingWalk = true;
                isPlayingRun = false;
            }
        }
        else
        {
            if (!isPlayingRun)
            {
                audioSource.clip = runClip;
                audioSource.loop = true;
                audioSource.volume = originalVolume;
                audioSource.Play();
                isPlayingWalk = false;
                isPlayingRun = true;
            }
        }
    }

    IEnumerator FadeOutAndStop()
    {
        isFading = true;
        float startVolume = audioSource.volume;
        
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.volume = originalVolume;
        
        isPlayingWalk = false;
        isPlayingRun = false;
        isFading = false;
    }
}