using System.Collections;
using System.Linq;
using Libs.Helpers;
using Libs.Repositories;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform leaderboardGrid;
    [SerializeField] private LeaderboardElement leaderboardElementPrefab;
    [SerializeField] private GameObject leaderboardSkeletonLoading;
    [SerializeField] private Material biggestGamblerMaterial;
    [SerializeField]
    private Color[] topColors = { Color.yellow, Color.gray, Color.Lerp(Color.red, Color.yellow, 0.5f) };

    private readonly Color32[] _gradientColors = { ColorHelper.PaleYellow, ColorHelper.LightGreen };
    private readonly float[] _gradientTimes = { 0.5f, 1.0f };
    private Gradient _biggestGamblerGradient;

    private void OnEnable()
    {
        DataMapper.OnMapData += () =>
        {
            ClearExistingLeaderboard();
            RefreshLeaderboard();
        };
    }

    private void Start()
    {
        _biggestGamblerGradient = GradientHelper.CreateGradient(_gradientColors, _gradientTimes);
    }

    private void RefreshLeaderboard()
    {
        ClearExistingLeaderboard();
        leaderboardSkeletonLoading.SetActive(true);
        UserRepository.GetAllUsers().Then(users =>
        {
            foreach (var user in users)
            {
                foreach (var buff in user.buffPurchase)
                    user.balance += buff.price;
            }

            var topUsers = users.OrderByDescending(x => x.balance).Take(3).ToList();

            var index = 0;
            foreach (var user in topUsers)
            {
                var leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardGrid);
                leaderboardElement.SetData(user, topColors[index]);

                if (index == 0)
                    leaderboardElement.SetData(user, topColors[index], _biggestGamblerGradient, biggestGamblerMaterial);

                index++;
            }
            leaderboardSkeletonLoading.SetActive(false);
        });
    }

    private void ClearExistingLeaderboard()
    {
        foreach (Transform child in leaderboardGrid) 
            Destroy(child.gameObject);
    }
}