using System.Collections;
using UnityEngine;

public class MainMenuCharacterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] charactersPrefabs;
    private int[] animations =
    {
        Animator.StringToHash("Idle_L"),
        Animator.StringToHash("Idle_R"),
        Animator.StringToHash("Jump_L"),
        Animator.StringToHash("Jump_R"),
    };

    [Header("Animation Settings")]
    [SerializeField] private float minDelay = 2f;
    [SerializeField] private float maxDelay = 5f;

    private Animator animator;

    private void Start()
    {
        if(charactersPrefabs != null && charactersPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, charactersPrefabs.Length);
            GameObject character = Instantiate(charactersPrefabs[randomIndex], transform.position, Quaternion.identity);
            animator = character.GetComponent<Animator>();

            if(animator != null)
            {
                StartCoroutine(RandomAnimation());
            }
        }
    }

    private IEnumerator RandomAnimation()
    {
        while(true)
        {
            float delay = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(delay);

            if (animations != null && animations.Length > 0)
            {
                int stateIndex = Random.Range(0, animations.Length);
                animator.Play(animations[stateIndex]);
            }

        }
    }

}
