using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataMapper : MonoBehaviour
    {
        private FirebaseDataLoader _loader;
        [SerializeField] private Transform matchPanelParent;
        [SerializeField] private MatchView matchPanel;
        [SerializeField] private MoneyView moneyView;
        private async void Awake()
        {
            _loader = new();
            List<Match> matches = await _loader.FetchMatchesData();
            //TODO Get from firebase
            UserData.Name = "Bodya";
            UserData.Balance = 1000;
            moneyView.SetMoney(UserData.Balance);
            //foreach (var match in matches.Where(x=>!x.IsFinished))
            foreach (var match in matches)
            {
              var matchView = Instantiate(matchPanel,matchPanelParent);
              
              matchView.SetData(match);
            }
        }
    }
