using Firebase.Messaging;
using UnityEngine;

public class PushNotificationsManager : MonoBehaviour
{
    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseMessaging.TokenReceived += TokenReceived;
            FirebaseMessaging.MessageReceived += MessageReceived;
        });
    }
    private void TokenReceived(object sender, TokenReceivedEventArgs e)
    {
        Debug.Log("TokenReceived :" + e.Token);
    }
    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("MessageReceived :" + e.Message);
    }
}
