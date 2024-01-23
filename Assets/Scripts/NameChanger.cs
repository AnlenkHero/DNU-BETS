using System;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameChanger : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputFieldField;
    [SerializeField] private Button saveButton;

    public static event Action OnNameChanged;
    private void OnEnable()
    {
        saveButton.onClick.AddListener(Save);
    }

    private void Save()
    {
        if (String.IsNullOrWhiteSpace(nameInputFieldField.text))
        {
            nameInputFieldField.text = null;
            nameInputFieldField.placeholder.GetComponent<TextMeshProUGUI>().text = "Name is empty...";
        }
        else
        {
            UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
            {
                user.userName = nameInputFieldField.text;
                UserRepository.UpdateUserInfo(user).Then(_ =>
                {
                    UserData.Name = user.userName;
                    OnNameChanged?.Invoke();
                });
            });
        }
    }
}