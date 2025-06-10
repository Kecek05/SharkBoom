using System.Collections;
using UnityEngine;

public class MainMenuCharacterController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject shark;
    [SerializeField] private GameObject orca;
    [Header("Settings")]
    [SerializeField] private int loopsForEachAnimation;
    [SerializeField] private readonly int[] animations =
    {
        Animator.StringToHash("Idle_L"),
        Animator.StringToHash("Idle_R"),
    };

    private Animator animator;
    private GameObject character;

    public GameObject Character => character;

    private void Start()
    {
        int randomPlayer = Random.Range(0, 2);

        if (randomPlayer == 0)
        {
            shark.SetActive(true);
            animator = shark.GetComponentInChildren<Animator>();
        }
        else
        {
            orca.SetActive(true);
            animator = orca.GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            StartCoroutine(RandomAnimation());
        }
    }

    private IEnumerator RandomAnimation()
    {
        while (true)
        {
            if (animations != null && animations.Length > 0)
            {
                int stateIndex = Random.Range(0, animations.Length);
                int animationHash = animations[stateIndex];

                animator.Play(animationHash);

                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.shortNameHash == animationHash && stateInfo.normalizedTime >= loopsForEachAnimation;
                });
            }
        }
    }

}
