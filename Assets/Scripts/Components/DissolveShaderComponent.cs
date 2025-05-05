using System;
using System.Collections;
using UnityEngine;

public class DissolveShaderComponent : MonoBehaviour
{
    [SerializeField] private float dissolveDurationFadeIn = 1f;
    [SerializeField] private float dissolveDurationFadeOut = 0.3f;
    private float dissolveStrenght;

    public void DissolveFadeIn(Action dissolverCallback = null)
    {
        StartCoroutine(Dissolver(true, dissolverCallback));
    }

    private IEnumerator Dissolver(bool isFadeIn, Action dissolverCallback = null)
    {
        float elapsedTime = 0f;
        Material dissolveMaterial = GetComponent<Renderer>().material;

        if(isFadeIn)
        {
            dissolveMaterial.SetFloat("_DissolveStrenght", 1);
        }

        while (elapsedTime < (isFadeIn? dissolveDurationFadeIn : dissolveDurationFadeOut))
        {
            elapsedTime += Time.deltaTime;

            if(isFadeIn)
            {
                dissolveStrenght = Mathf.Lerp(1, 0, elapsedTime / dissolveDurationFadeIn);
            } else
            {
                dissolveStrenght = Mathf.Lerp(0, 1, elapsedTime / dissolveDurationFadeOut);
            }

            dissolveMaterial.SetFloat("_DissolveStrenght", dissolveStrenght);
            yield return null;
        }

        //Finished
        dissolverCallback?.Invoke();
    }


    public void DissolveFadeOut(Action dissolverCallback = null)
    {
        if(gameObject.activeInHierarchy)
            StartCoroutine(Dissolver(false, dissolverCallback));
    }
}
