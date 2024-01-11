using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
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

    private List<Match> _allMatchesList = new();
    private readonly Color32[] _gradientColors = { ColorHelper.PaleYellow, ColorHelper.LightGreen };
    private readonly float[] _gradientTimes = { 0.5f, 1.0f };
    private Gradient _biggestGamblerGradient;
    private bool _isLeaderboardRefreshing;

    private void OnEnable()
    {
        DataMapper.OnMapData += RefreshLeaderboard;
    }

    private void Start()
    {
        _biggestGamblerGradient = GradientHelper.CreateGradient(_gradientColors, _gradientTimes);
    }

    private void RefreshLeaderboard()
    {
        if (_isLeaderboardRefreshing) return;

        _isLeaderboardRefreshing = true;
        ClearLeaderboard();
        FetchAndDisplayLeaderboard();
    }

    private void ClearLeaderboard()
    {
        leaderboardGrid.ClearExistingElementsInParent();
        leaderboardSkeletonLoading.SetActive(true);
        leaderboardExceptionText.gameObject.SetActive(false);
    }

    private void FetchAndDisplayLeaderboard()
    {
        MatchesRepository.GetAllMatches().Then(list => _allMatchesList = list)
            .Catch(exception =>
            {
                _allMatchesList = null;
                Debug.LogError($"Failed to get all matches for leaderboard {exception}");
            });

        UserRepository.GetAllUsers()
            .Then(ProcessUsers)
            .Catch(HandleLeaderboardError);
    }

    private void ProcessUsers(List<User> users)
    {
        ApplyBuffPurchasesToUsers(users);
        var topUsers = users.OrderByDescending(u => u.balance).Take(3).ToList();
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
            leaderboardElement.SetData(topUsers[i], topColors[i], _allMatchesList);

            if (i == 0)
            {
                leaderboardElement.SetData(topUsers[i], topColors[i], _allMatchesList, _biggestGamblerGradient,
                    biggestGamblerMaterial);
            }
        }

        leaderboardSkeletonLoading.SetActive(false);
    }

    private void HandleLeaderboardError(Exception exception)
    {
        _isLeaderboardRefreshing = false;
        leaderboardExceptionText.gameObject.SetActive(true);
        leaderboardExceptionText.text = $"Failed to load leaderboard: {exception.Message}";
        Debug.LogError($"Failed to load leaderboard: {exception}");
    }
}