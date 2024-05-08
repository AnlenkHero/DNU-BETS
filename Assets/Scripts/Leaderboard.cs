using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using RSG;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform leaderboardGrid;
    [SerializeField] private LeaderboardElement leaderboardElementPrefab;
    [SerializeField] private GameObject leaderboardSkeletonLoading;
    [SerializeField] private Material biggestGamblerMaterial;
    [SerializeField] private TextMeshProUGUI leaderboardExceptionText;

    [SerializeField]
    private Color[] topColors = { Color.yellow, Color.gray, Color.Lerp(Color.red, Color.yellow, 0.5f) };
    
    private const int TopUsersCount = 3;
    private List<User> _allUsersList = new();
    private readonly Color32[] _gradientColors = { ColorHelper.PaleYellow, ColorHelper.LightGreen };
    private readonly float[] _gradientTimes = { 0.5f, 1.0f };
    private Gradient _biggestGamblerGradient;
    private bool _isLeaderboardRefreshing;

    private void OnEnable()
    {
        DataMapper.OnMapDataStarted += StartLoading;
        DataMapper.OnMapDataFinished += FetchAndDisplayLeaderboard;
    }

    private void Start()
    {
        _biggestGamblerGradient = GradientHelper.CreateGradient(_gradientColors, _gradientTimes);
    }

    private void StartLoading()
    {
        if (_isLeaderboardRefreshing) return;
        _isLeaderboardRefreshing = true;
        ClearLeaderboard();
    }

    private void ClearLeaderboard()
    {
        leaderboardGrid.ClearExistingElementsInParent();
        leaderboardSkeletonLoading.SetActive(true);
        leaderboardExceptionText.gameObject.SetActive(false);
    }

    private void FetchAndDisplayLeaderboard()
    {
        var promises = new List<IPromise> { WrapGetAllUsers() };

        Promise.All(promises).Then(ProcessUsers).Catch(HandleLeaderboardError);
    }

    private void ProcessUsers()
    {
        ApplyBuffPurchasesToUsers(_allUsersList);
        var topUsers = _allUsersList.OrderByDescending(u => u.balance).Take(TopUsersCount).ToList();
        DisplayTopUsers(topUsers);
        _isLeaderboardRefreshing = false;
    }

    private void ApplyBuffPurchasesToUsers(List<User> users)
    {
        foreach (var user in users)
        {
            user.balance += user.buffPurchase.Sum(buff => buff.price);
        }
    }

    private void DisplayTopUsers(List<User> topUsers)
    {
        for (var i = 0; i < topUsers.Count; i++)
        {
            var leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardGrid);
            leaderboardElement.SetData(topUsers[i], topColors[i], MatchCache.Matches);

            if (i == 0)
            {
                leaderboardElement.SetData(topUsers[i], topColors[i], MatchCache.Matches, _biggestGamblerGradient,
                    biggestGamblerMaterial);
            }
        }

        leaderboardSkeletonLoading.SetActive(false);
    }
    
    private IPromise WrapGetAllUsers()
    {
        var promise = new Promise();

        UserRepository.GetAllUsers()
            .Then(list =>
            {
                _allUsersList = list;
                promise.Resolve();
            })
            .Catch(exception => promise.Reject(exception));

        return promise;
    }

    private void HandleLeaderboardError(Exception exception)
    {
        _isLeaderboardRefreshing = false;
        leaderboardExceptionText.gameObject.SetActive(true);
        leaderboardExceptionText.text = $"Failed to load leaderboard: {exception.Message}";
        Debug.LogError($"Failed to load leaderboard: {exception}");
    }
}