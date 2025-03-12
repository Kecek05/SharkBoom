using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI authTxt;
    [SerializeField] private GameObject buttonsParent;


    [Space(3)]
    [SerializeField] private Button authBtn;
    [SerializeField] private Button authAnonymouslyBtn;


    private void Awake()
    {
        ShowButtons();
        HideTxt();

        authBtn.onClick.AddListener(async () =>
        {
            HideButtons();
            ShowTxt();
            //auth mobile
            await ClientSingleton.Instance.AuthClient();
        });
        authAnonymouslyBtn.onClick.AddListener(async () =>
        {
            HideButtons();
            ShowTxt();
            await ClientSingleton.Instance.AuthClientAnonymously();
        });
    }


    private void Start()
    {
        AuthenticationWrapper.OnSignInFail += AuthenticationWrapper_OnSignInFail;



        if(Application.platform == RuntimePlatform.Android)
        {
            //Loggin with Google Play
        }
    }

    private void AuthenticationWrapper_OnSignInFail()
    {
        HideTxt();
        ShowButtons();
    }

    private void HideTxt()
    {
        authTxt.gameObject.SetActive(false);
    }

    private void ShowTxt()
    {
        authTxt.gameObject.SetActive(true);
    }

    private void HideButtons()
    {
        buttonsParent.SetActive(false);
    }

    private void ShowButtons()
    {
        buttonsParent.SetActive(true);
    }

    private void OnDestroy()
    {
        AuthenticationWrapper.OnSignInFail -= AuthenticationWrapper_OnSignInFail;
    }
}
