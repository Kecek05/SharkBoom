using GooglePlayGames;
using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI authTxt;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private TextMeshProUGUI authAndroidTxt;
    [SerializeField] private TextMeshProUGUI authStateTxt;

    [Space(3)]
    [SerializeField] private Button authBtn;
    [SerializeField] private Button authAnonymouslyBtn;


    private void Awake()
    {
        ShowButtons();
        HideTxt();

        authBtn.onClick.AddListener(async () =>
        {
            //HideButtons();
            //ShowTxt();
            //auth mobile
            // await ClientSingleton.Instance.AuthClientUnity();
        });
        authAnonymouslyBtn.onClick.AddListener(async () =>
        {
            HideButtons();
            ShowTxt();
            await ClientSingleton.Instance.AuthClientAnonymously();
        });
    }


    private async void Start()
    {
        AuthenticationWrapper.OnSignInFail += AuthenticationWrapper_OnSignInFail;



        if(Application.platform == RuntimePlatform.Android)
        {
            //Loggin with Google Play
            HideButtons();
            ShowTxt();
            await ClientSingleton.Instance.AuthAndroid();
            authAndroidTxt.text = $"Token: {AuthenticationWrapper.GooglePlayToken} name: {PlayGamesPlatform.Instance.GetUserDisplayName()} ID: {PlayGamesPlatform.Instance.GetUserId()}";
        }
    }

    private void Update()
    {
        authStateTxt.text = AuthenticationWrapper.AuthState.ToString();
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
