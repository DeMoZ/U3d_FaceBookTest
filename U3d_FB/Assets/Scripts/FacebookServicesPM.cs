using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook.Unity;
using UniRx;
using UnityEngine;

public class FacebookServicesPM : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<string> onAuthFacebook;
        public ReactiveCommand onLogoutFacebook;
        public ReactiveCommand onClickAuthFacebook;
        public ReactiveCommand onClickLogoutFacebook;

        public ReactiveCommand onClickLoadPersonData;
        public ReactiveCommand<string> onDataLoaded;
    }

    private Ctx _ctx;

    public FacebookServicesPM(Ctx ctx)
    {
        _ctx = ctx;

        _ctx.onClickAuthFacebook.Subscribe(_=>Auth());
        _ctx.onClickLogoutFacebook.Subscribe(_=>Logout());
        
        _ctx.onClickLoadPersonData.Subscribe(_=>LoadPersonData());

        Init();
    }

    private void Auth()
    {
        Debug.Log($"<color=red>FB</color> logIn clicked");
        if (FB.IsLoggedIn)
            return;

        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void Logout()
    {
        Debug.Log($"<color=red>FB</color> logOut clicked");
        FB.LogOut();
        _ctx.onLogoutFacebook.Execute();
    }

    private void Init()
    {
        if (!FB.IsInitialized)
            FB.Init(InitCallback, OnHideUnity);
        else
            ActivateApp();
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
            ActivateApp();
    }

    private void ActivateApp()
    {
        FB.ActivateApp();

        //AuthCheckAfterDelay(1).DoAsync();
        AuthCheckAfterDelay(1);
    }

    private async Task AuthCheckAfterDelay(float delaySeconds)
    {
        await Task.Delay((int)(delaySeconds * 1000));

        if (FB.IsLoggedIn)
            _ctx.onAuthFacebook.Execute(FB.Mobile.UserID);
    }

    private void OnHideUnity(bool isGameShown)
    {
        Time.timeScale = isGameShown ? 1 : 0;
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("[FACEBOOK] login success");
            var accessToken = result.AccessToken;
            _ctx.onAuthFacebook.Execute(accessToken.UserId);
        }
        else
        {
            Debug.Log($"[FACEBOOK] login failure: {result.RawResult}");
        }
    }
//-----------------------------------------

    private void LoadPersonData()
    {
        if (!FB.IsLoggedIn)
            return;
        
        var query =
            "/me?fields=id,name,first_name,middle_name,last_name,name_format,email,gender,installed";
        FB.API(query, HttpMethod.GET, GetUserInfoCallback);
    }

    private void GetUserInfoCallback(IGraphResult result)
    {
        _ctx.onDataLoaded.Execute(result.RawResult);
    }

    public void Dispose()
    {
    }
}