using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIPanel : MonoBehaviour
{
    private LobbyRelay lobbyRelay;

    public GameObject mainUI;

    public Button button;

    public TextMeshProUGUI lobbyName;
    public TextMeshProUGUI amountOfPlayersInLobby;
    public string lobbyId;


    public bool full;
    public bool Full
    {
        get
        {
            return full;
        }
        set
        {
            button.enabled = !value;
            full = value;
        }
    }


    private void Start()
    {
        lobbyRelay = FindObjectOfType<LobbyRelay>();
        mainUI = transform.GetChild(0).gameObject;

        button = GetComponentInChildren<Button>(true);

        TextMeshProUGUI[] textFields = GetComponentsInChildren<TextMeshProUGUI>(true);
        lobbyName = textFields[1];
        amountOfPlayersInLobby = textFields[0];
    }
    public void JoinLobby()
    {
        lobbyRelay.JoinLobbyById(lobbyId);
    }
}