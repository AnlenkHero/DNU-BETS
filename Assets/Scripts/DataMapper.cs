using System.Collections.Generic;
using Libs.Models;
using Libs.Services;
using UnityEngine;
using Match = Libs.Models.Match;

public class DataMapper : MonoBehaviour
    {
        private FirebaseDataLoader _loader;
        [SerializeField] private Transform matchPanelParent;
        [SerializeField] private MatchView matchPanel;
        [SerializeField] private MoneyView moneyView;
        [SerializeField] private SwipeMenu swipeMenu;
        private async void Awake()
        {
            List<Match> matches = await FirebaseDataLoader.FetchMatchesData();
            //TODO Get from firebase
            UserData.Name = "Bodya";
            UserData.Balance = 1000;
            moneyView.SetMoney(UserData.Balance);
           // foreach (var match in matches.Where(x=>!x.IsFinished))
            foreach (var match in matches)
            {
              var matchView = Instantiate(matchPanel,matchPanelParent);
              
              matchView.SetData(match);
            }
            swipeMenu.InitializeViews();
        }
    }
