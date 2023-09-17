using System.Collections;
using System.Collections.Generic;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;
using Match = Libs.Models.Match;

public class DataMapper : MonoBehaviour
    {
        [SerializeField] private Transform matchPanelParent;
        [SerializeField] private MatchView matchPanel;
        [SerializeField] private MoneyView moneyView;
        [SerializeField] private SwipeMenu swipeMenu;
        [SerializeField] private Button updateMatchesButton;
        private void Awake()
        {
            updateMatchesButton.onClick.AddListener(MapData);
            MapData();
        }

        private void OnEnable()
        {
            NetworkCheck.OnInternetEstablished += MapData;
        }

        private void OnDisable()
        {
            NetworkCheck.OnInternetEstablished -= MapData;
        }
        
        private async void MapData()
        {
            MatchesRepository.GetBettingAvailableMatches().Then(matches =>
            {
                InitializeUserData();
                ClearExistingMatches();
                StartCoroutine(CreateMatchViews(matches));
            }).Catch((exception => Debug.LogError(exception.Message)));
        }

        private IEnumerator CreateMatchViews(List<Match> matches)
        {
            yield return new WaitForSeconds(0.5f);
            // foreach (var match in matches.Where(x=>!x.IsFinished))
            foreach (var match in matches)
            {
                var matchView = Instantiate(matchPanel,matchPanelParent);
              
                matchView.SetData(match);
            }
            swipeMenu.InitializeViews();
        }
        
        private void ClearExistingMatches()
        {
            foreach (Transform child in matchPanelParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void InitializeUserData()
        {
            // TODO: Fetch these from Firebase or other data source
            UserData.Name = "Bodya";
            //UserData.Balance = 1000;
            moneyView.SetMoney(1000);
        }
    }
