using System.Collections.Generic;
using Facebook.Unity;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public enum FbRequestType
{
    None,
    Firends
}

public class GameBoot : MonoBehaviour
{
    [SerializeField] private GameObject _buttonPrefab = default;
    
    [SerializeField] private Button _loginButton = default;
    [SerializeField] private Button _logoutButton = default;
    [SerializeField] private Button _launchButton = default;
    [SerializeField] private Button _loadFriends = default;

    [SerializeField] private TMP_InputField _input = default;
    [SerializeField] private ColorSemaphore _semaphore = default;
    [SerializeField] private TMP_InputField _loadResult = default;

    [SerializeField] private RectTransform _queryButtonParent = default;
    [SerializeField] private List<string> _queryList = default;
    private List<Button> _queryButtons = new List<Button>();

    private FacebookServicesPM _facebookServicesPm;

    private ReactiveCommand<string> onAuthFacebook;
    private ReactiveCommand onLogoutFacebook;
    private ReactiveCommand onClickAuthFacebook;
    private ReactiveCommand onClickLogoutFacebook;
    private ReactiveCommand onClickLoadFriends;

    private ReactiveCommand<SemaphoreColor> onSetColor;

    private ReactiveCommand<string> onClickLaunchCommand;
    private ReactiveCommand<string> onDataLoaded;
    private ReactiveCommand<string> onFriendsLoaded;
    private ReactiveCommand<string> onError;

    void Start()
    {
        onSetColor = new ReactiveCommand<SemaphoreColor>();

        onAuthFacebook = new ReactiveCommand<string>();
        onLogoutFacebook = new ReactiveCommand();
        onClickAuthFacebook = new ReactiveCommand();
        onClickLogoutFacebook = new ReactiveCommand();
        onClickLoadFriends = new ReactiveCommand();

        onClickLaunchCommand = new ReactiveCommand<string>();
        onDataLoaded = new ReactiveCommand<string>();
        onError = new ReactiveCommand<string>();
        onFriendsLoaded = new ReactiveCommand<string>();
        
        var fbCtx = new FacebookServicesPM.Ctx
        {
            onAuthFacebook = onAuthFacebook,
            onLogoutFacebook = onLogoutFacebook,
            onClickAuthFacebook = onClickAuthFacebook,
            onClickLogoutFacebook = onClickLogoutFacebook,
            onClickLoadFriends = onClickLoadFriends,

            onClickLaunchCommand = onClickLaunchCommand,
            onDataLoaded = onDataLoaded,
            onError = onError,
            onFriendsLoaded = onFriendsLoaded
        };
        _facebookServicesPm = new FacebookServicesPM(fbCtx);

        _loginButton.onClick.AddListener(ClickLogIn);
        _logoutButton.onClick.AddListener(ClickLogOut);
        _launchButton.onClick.AddListener(ClickLaunch);
        _loadFriends.onClick.AddListener(ClickLoadFriends);

        onAuthFacebook.Subscribe(SuccessLogIn).AddTo(this);
        onLogoutFacebook.Subscribe(_ => SuccessLogOut()).AddTo(this);

        onDataLoaded.Subscribe(OnDataLoaded).AddTo(this);
        onFriendsLoaded.Subscribe(OnFriendsLoaded).AddTo(this);
        onError.Subscribe(OnDataLoaded).AddTo(this);

        var semaphoreCtx = new ColorSemaphore.Ctx
        {
            onSetColor = onSetColor
        };
        _semaphore.SetCtx(semaphoreCtx);

        FillQueryList();
        CreateQueryButtons();
        CheckFbStatusOnStart();
    }

    private void FillQueryList()
    {
        _queryList.Add("/me?");
        _queryList.Add("/me?fields=id,name,first_name,middle_name,last_name,email,gender,picture");
        _queryList.Add("/me?fields=friends");
        _queryList.Add("/me/friends");
        _queryList.Add("/?fields=id,name,first_name,middle_name,last_name,gender,picture"); // put here id to get info
        //_queryList.Add();
    }

    private void CreateQueryButtons()
    {
        foreach (var query in _queryList)
        {
            var go = Instantiate(_buttonPrefab,_queryButtonParent,false);
            
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            var subLen = query.Length > 15 ? 15 : query.Length-1;
            txt.text = query.Substring(1, subLen);
            
            btn.onClick.AddListener(() =>
            {
                _input.text = query;
            });

            _queryButtons.Add(btn);
        }
    }

    private void boo()
    {
        throw new System.NotImplementedException();
    }

    private void CheckFbStatusOnStart()
    {
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

    private void ClickLaunch() => onClickLaunchCommand?.Execute(_input.text);
    private void ClickLogIn() => onClickAuthFacebook?.Execute();
    private void ClickLogOut() => onClickLogoutFacebook?.Execute();
    private void ClickLoadFriends() => onClickLoadFriends?.Execute();
    
    
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
    
    private void OnFriendsLoaded(string data)
    {
        // tOdo
        _loadResult.text = data;
    }
}