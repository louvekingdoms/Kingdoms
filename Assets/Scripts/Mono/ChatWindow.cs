using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatWindow : MonoBehaviour
{
    public GameObject feedGameObject;
    public GameObject exampleLine;
    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        inputField.onSubmit.AddListener(o=> {
            Game.chat.SendMessage(o);
            inputField.text = "";
        });

        Game.chat.OnNewMessage += (author, msg) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(
                delegate
                {
                    var i = Instantiate(exampleLine);
                    exampleLine.GetComponent<TextMeshProUGUI>().text = "{0}: {1}".Format(author, msg);
                    exampleLine.SetActive(true);
                    i.transform.parent = feedGameObject.transform;
                }
            );
        };
    }
}
