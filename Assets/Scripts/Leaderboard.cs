using System.Collections;
using System.Linq;
using Libs.Repositories;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform leaderboardGrid;
    [SerializeField] private LeaderboardElement leaderboardElementPrefab;

    [SerializeField]
    private Color[] topColors = { Color.yellow, Color.gray, Color.Lerp(Color.red, Color.yellow, 0.5f) };

    private void Start()
    {
        StartCoroutine(LeaderBoardCoroutine());
    }

    private IEnumerator LeaderBoardCoroutine()
    {
        while (true)
        {
            ClearExistingLeaderboard();
            yield return new WaitForSeconds(1f);
            RefreshLeaderboard();
            yield return new WaitForSeconds(60f);
        }
    }

    private void RefreshLeaderboard()
    {
        UserRepository.GetAllUsers().Then(users =>
        {
            foreach (var user in users)
            {
                foreach (var buff in user.buffPurchase)
                {
                    user.balance += buff.price;
                }
            }

            var topUsers = users.OrderByDescending(x => x.balance).Take(3).ToList();

            int index = 0;
            foreach (var user in topUsers)
            {
                var leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardGrid);
                leaderboardElement.SetData(user, topColors[index]);
                index++;
            }
        });
    }

    private void ClearExistingLeaderboard()
    {
        foreach (Transform child in leaderboardGrid)
        {
            Destroy(child.gameObject);
        }
    }
}