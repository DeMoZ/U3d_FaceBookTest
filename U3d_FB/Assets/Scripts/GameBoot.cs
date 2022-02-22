using Facebook.Unity;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameBoot : MonoBehaviour
{
    [SerializeField] private Button _loginButton = default;
    [SerializeField] private Button _logoutButton = default;
    [SerializeField] private Button _loadPersonDataButton = default;

    [SerializeField] private ColorSemaphore _semaphore = default;
    [SerializeField] private TextMeshProUGUI _loadResult = default;

    private FacebookServicesPM _facebookServicesPm;

    private ReactiveCommand<string> onAuthFacebook;
    private ReactiveCommand onLogoutFacebook;
    private ReactiveCommand onClickAuthFacebook;
    private ReactiveCommand onClickLogoutFacebook;

    private ReactiveCommand<SemaphoreColor> onSetColor;

    private ReactiveCommand onClickLoadPersonData;
    private ReactiveCommand<string> onDataLoaded;

    void Start()
    {
        onSetColor = new ReactiveCommand<SemaphoreColor>();

        onAuthFacebook = new ReactiveCommand<string>();
        onLogoutFacebook = new ReactiveCommand();
        onClickAuthFacebook = new ReactiveCommand();
        onClickLogoutFacebook = new ReactiveCommand();

        onClickLoadPersonData = new ReactiveCommand();
        onDataLoaded = new ReactiveCommand<string>();

        var fbCtx = new FacebookServicesPM.Ctx
        {
            onAuthFacebook = onAuthFacebook,
            onLogoutFacebook = onLogoutFacebook,
            onClickAuthFacebook = onClickAuthFacebook,
            onClickLogoutFacebook = onClickLogoutFacebook,

            onClickLoadPersonData = onClickLoadPersonData,
            onDataLoaded = onDataLoaded
        };
        _facebookServicesPm = new FacebookServicesPM(fbCtx);

        _loginButton.onClick.AddListener(ClickLogIn);
        _logoutButton.onClick.AddListener(ClickLogOut);
        _loadPersonDataButton.onClick.AddListener(ClickLoadPersonData);

        onAuthFacebook.Subscribe(SuccessLogIn).AddTo(this);
        onLogoutFacebook.Subscribe(_ => SuccessLogOut()).AddTo(this);

        onDataLoaded.Subscribe(OnDataLoaded).AddTo(this);

        var semaphoreCtx = new ColorSemaphore.Ctx
        {
            onSetColor = onSetColor
        };
        _semaphore.SetCtx(semaphoreCtx);

        if (FB.IsLoggedIn)
        {
            onSetColor.Execute(SemaphoreColor.Green);
            _loadResult.text = "<color=green>logged before</color>";
        }
        else
        {
            onSetColor.Execute(SemaphoreColor.Red);
            _loadResult.text = "<color=red>not logged</color>";
        }
    }

    private void ClickLoadPersonData()
    {
        onClickLoadPersonData?.Execute();
    }

    private void ClickLogIn()
    {
        onClickAuthFacebook?.Execute();
    }

    private void ClickLogOut()
    {
        onClickLogoutFacebook?.Execute();
    }

    public void SuccessLogIn(string success)
    {
        onSetColor.Execute(SemaphoreColor.Green);
        _loadResult.text = "Log <color=green>in</color> Success";
    }

    private void SuccessLogOut()
    {
        onSetColor.Execute(SemaphoreColor.Yellow);
        _loadResult.text = "Log <color=yellow>out</color> Success";
    }

    private void OnDataLoaded(string data)
    {
        _loadResult.text = data;
    }
}