﻿using System;
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
        public ReactiveCommand onClickLoadFriends;

        public ReactiveCommand<string> onClickLaunchCommand;
        public ReactiveCommand<string> onDataLoaded;
        public ReactiveCommand<string> onFriendsLoaded;
        public ReactiveCommand<string> onError;
    }

    private Ctx _ctx;
    private int _countActivateCalls = 0; 
    
    public FacebookServicesPM(Ctx ctx)
    {
        _ctx = ctx;

        _ctx.onClickAuthFacebook.Subscribe(_ => Auth());
        _ctx.onClickLogoutFacebook.Subscribe(_ => Logout());
        _ctx.onClickLoadFriends.Subscribe(_ => LoadFriends());

        _ctx.onClickLaunchCommand.Subscribe(LaunchCommand);

        Init();
    }

    private void Auth()
    {
        Debug.Log($"[FacebookServicesPm]  logIn clicked");
        if (FB.IsLoggedIn)
        {
            Debug.Log($"[FacebookServicesPm]  Already logged in, return;");
            return;
        }

        var perms = new List<string>() { "public_profile", "user_friends" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void Logout()
    {
        Debug.Log($"[FacebookServicesPm] logOut clicked");
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
        Debug.LogWarning($"[FacebookServicesPm] check FB.IsInitialized on init {FB.IsInitialized}");

        if (FB.IsInitialized)
            ActivateApp();
    }

    private void ActivateApp()
    {
        _countActivateCalls++;
        Debug.Log($"[FacebookServicesPm] Call Activate App times = {_countActivateCalls}");

        //FB.ActivateApp();
        AuthCheckAfterDelay();
    }

    private async Task AuthCheckAfterDelay(float? delaySeconds = null)
    {
        if(delaySeconds.HasValue)
            await Task.Delay((int) (delaySeconds * 1000));

        var isLoggedInPlugin = FB.IsLoggedIn;

        Debug.Log($"[FacebookServicesPm] FB.IsLoggedIn = {isLoggedInPlugin}");

        if (FB.IsLoggedIn)
        {
            _ctx.onAuthFacebook.Execute(FB.Mobile.UserID);
            Debug.Log($"[FacebookServicesPm] FB.IsLoggedIn ; will ask for FB.ClientToken");
            Debug.Log($"[FacebookServicesPm] FB.IsLoggedIn ; FB.ClientToken = {FB.ClientToken}");
            var query = "/me/friends";
            
            FB.API(query, HttpMethod.GET, GetUserFriendsCallback);
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        Time.timeScale = isGameShown ? 1 : 0;
    }

    private void AuthCallback(ILoginResult result)
    {
        Debug.Log($"[FacebookServicesPm] [FACEBOOK] AuthCallback, result: {result}");
        if (FB.IsLoggedIn)
        {
            Debug.Log("[FacebookServicesPm] [FACEBOOK] login success");
            var accessToken = result.AccessToken;
            _ctx.onAuthFacebook.Execute(accessToken.UserId);
        }
        else
        {
            Debug.Log($"[FacebookServicesPm] [FACEBOOK] login failure: {result.RawResult}");
            _ctx.onError.Execute($"{result.RawResult}");
        }
    }
//-----------------------------------------

    private void LaunchCommand(string query)
    {
        if (!FB.IsLoggedIn)
            return;

        FB.API(query, HttpMethod.GET, GetUserInfoCallback);
    }
    
    private void LoadFriends()
    {
        if (!FB.IsLoggedIn)
            return;
    
        var query = "/me/friends";
        FB.API(query, HttpMethod.GET, GetUserFriendsCallback);
    }

    private void GetUserInfoCallback(IGraphResult result)
    {
        Debug.Log($"[FacebookServicesPm] [FACEBOOK] GetUser Info {result.RawResult}");
        _ctx.onDataLoaded.Execute(result.RawResult);
    }
    private void GetUserFriendsCallback(IGraphResult result)
    {
        Debug.Log($"[FacebookServicesPm] [FACEBOOK] GetUser Friends ERROR {result.Error}");
         
        Debug.Log($"[FacebookServicesPm] [FACEBOOK] GetUser Friends {result.RawResult}");
        _ctx.onFriendsLoaded.Execute(result.RawResult);
    }

    public void Dispose()
    {
    }
}