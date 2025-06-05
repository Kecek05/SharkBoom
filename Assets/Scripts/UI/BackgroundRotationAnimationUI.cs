using System.Collections;
using UnityEngine;

public class BackgroundRotationAnimationUI : MonoBehaviour
{
    [SerializeField] private RectTransform imageForAnimate;
    [SerializeField] private float rotationSpeed;

    private Coroutine rotationCoroutine;


    private void OnEnable()
    {
        if (rotationCoroutine == null)
        {
            rotationCoroutine = StartCoroutine(RotateLoop());
        }
    }

    private IEnumerator RotateLoop()
    {
        while (true)
        {
            imageForAnimate.rotation = Quaternion.identity;
            yield return null;
        }
    }

    private void OnDisable()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            imageForAnimate.Rotate(0f, 0f, 0f);
            rotationCoroutine = null;
        }
    }
}
