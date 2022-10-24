using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class SpectatePlayer : MonoBehaviourPunCallbacks
{
    private Player currentSpectateTarget;
    public PhotonView pv;
    public Camera cam;
    public GameObject spectateUI;
    public TextMeshProUGUI spectateText;

    private float xRotation = 0f;
    private float yRotation = 0f;

    public List<Player> spectatablePlayers = new List<Player>();
    private int spectateIndex = 0;

    private void Start()
    {
        if (pv.IsMine == false)
        {
            GetComponent<AudioListener>().enabled = false;
            cam.enabled = false;
            spectateUI.SetActive(false);
        }
        else
        {
            AudioListenerManager.SetCurrentAudioListener(GetComponent<AudioListener>());
            GetSpectatableTargets();
            Spectate(GetPlayer(spectateIndex));
        }
    }

    public override void OnEnable()
    {
        if (pv.IsMine)
        {
            PlayersInMatch.PlayerSpawned += OnPlayerSpawned;
            PlayersInMatch.PlayerDeSpawned += OnPlayerDeSpawned;
        }
    }

    public override void OnDisable()
    {
        if (pv.IsMine)
        {
            PlayersInMatch.PlayerSpawned -= OnPlayerSpawned;
            PlayersInMatch.PlayerDeSpawned -= OnPlayerDeSpawned;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Spectate(NextTarget());
            }

            if (Input.GetMouseButtonDown(1))
            {
                Spectate(PreviousTarget());
            }

            if(currentSpectateTarget == null)
            {
                FreeControlMouse();
            }
        }
    }

    private void GetSpectatableTargets()
    {
        foreach(var player in FindObjectsOfType<PlayerCamera>())
        {
            var owner = player.photonView.Owner;
            if (CareAboutTeams())
            {
                if (SameTeam(owner))
                {
                    spectatablePlayers.Add(owner);
                }
            }
            else
            {
                spectatablePlayers.Add(owner);
            }
        }
    }

    private void FreeControlMouse()
    {
        float sens = ControlSettingsManager.sensitivity;

        var mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        var mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        yRotation += mouseX;
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void Spectate(Player player)
    {
        SetupSpectateText(player);

        if(player == null)
        {
            return;
        }

        //Make sure index isn't invalid
        spectateIndex = GetValidSpectateIndex();

        spectateUI.SetActive(true);

        Transform playerCamToSpectate = null;

        foreach(var playerCam in FindObjectsOfType<PlayerCamera>())
        {
            if(playerCam.photonView.Owner == player)
            {
                playerCamToSpectate = playerCam.transform;
            }
        }


        if (playerCamToSpectate != null)
        {
            transform.SetParent(playerCamToSpectate);
            transform.position = playerCamToSpectate.position;
            transform.rotation = playerCamToSpectate.rotation;
            currentSpectateTarget = player;
        }
    }

    private Player GetPlayer(int index)
    {
        if(index >= spectatablePlayers.Count || index < 0 || spectatablePlayers.Count == 0)
        {
            return null;
        }
        else
        {
            return spectatablePlayers[index];
        }
    }

    private void SetupSpectateText(Player spectateTarget)
    {
        var textColor = GetColorForText(spectateTarget);
        spectateText.color = textColor;

        if(spectateTarget == null)
        {
            spectateText.text = "Spectating none";
        }
        else
        {
            if ((bool)spectateTarget.CustomProperties[PlayerPropertyKeys.isDeadKey] == false)
            {
                var playerName = spectateTarget.NickName.ToString();
                var playerKills = spectateTarget.CustomProperties[PlayerPropertyKeys.killsKey].ToString();
                var playerDeaths = spectateTarget.CustomProperties[PlayerPropertyKeys.deathsKey].ToString();

                spectateText.text = $"{playerName} - Kills : {playerKills} - Deaths {playerDeaths}";
            }
            else
            {
                spectateText.text = "Spectating none";
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (pv.IsMine)
        {
            bool changeSpectateTarget = false;
            if (spectatablePlayers.Contains(targetPlayer))
            {
                //Check if targetPlayer changed teams to an unspectatable team
                if (CareAboutTeams())
                {
                    if (SameTeam(targetPlayer) == false)
                    {
                        changeSpectateTarget = currentSpectateTarget == targetPlayer;
                        spectatablePlayers.Remove(targetPlayer);
                    }
                }
                else
                {
                    changeSpectateTarget = currentSpectateTarget == targetPlayer;
                    spectatablePlayers.Remove(targetPlayer);
                }
            }
            else
            {
                //Check if player should be added to spectatable list
                if (changedProps.ContainsKey(PlayerPropertyKeys.teamIDKey))
                {
                    if (CareAboutTeams())
                    {
                        if (SameTeam(targetPlayer))
                        {
                            spectatablePlayers.Add(targetPlayer);
                            changeSpectateTarget = currentSpectateTarget == null;
                        }
                    }
                    else
                    {
                        spectatablePlayers.Add(targetPlayer);
                        changeSpectateTarget = currentSpectateTarget == null;
                    }
                }
            }

            if (changeSpectateTarget)
            {
                Spectate(GetPlayer(spectateIndex));
            }
        }
    }

    private void OnPlayerSpawned(Player player)
    {
        if (spectatablePlayers.Contains(player))
        {
            //Make sure the team is spectatable
            if(CareAboutTeams() && SameTeam(player) == false)
            {
                spectatablePlayers.Remove(player);
                if(currentSpectateTarget == player)
                {
                    NextTarget();
                }
            }
        }
        else
        {
            if (CareAboutTeams())
            {
                if (SameTeam(player))
                {
                    spectatablePlayers.Add(player);
                    if(currentSpectateTarget == null)
                    {
                        NextTarget();
                    }
                }
            }
            else
            {
                spectatablePlayers.Add(player);
                if(currentSpectateTarget == null)
                {
                    NextTarget();
                }
            }
        }
    }

    private void OnPlayerDeSpawned(Player player)
    {
        if (spectatablePlayers.Contains(player))
        {
            spectatablePlayers.Remove(player);
            if(currentSpectateTarget == player)
            {
                NextTarget();
            }
        }
    }

    private int GetValidSpectateIndex()
    {
        return Mathf.Clamp(spectateIndex, 0, spectatablePlayers.Count - 1);
    }

    private Player PreviousTarget()
    {
        spectateIndex--;
        if (spectateIndex < 0)
        {
            spectateIndex = spectatablePlayers.Count - 1;
        }

        return GetPlayer(spectateIndex);
    }
    private Player NextTarget()
    {
        spectateIndex++;
        if(spectateIndex == spectatablePlayers.Count)
        {
            spectateIndex = 0;
        }


        return GetPlayer(spectateIndex);
    }

    private bool CareAboutTeams()
    {
        return (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey] != -1;
    }

    private bool SameTeam(Player player)
    {
        return (int)player.CustomProperties[PlayerPropertyKeys.teamIDKey] == (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropertyKeys.teamIDKey];
    }

    private Color GetColorForText(Player spectatingTarget)
    {
        if(spectatingTarget == null)
        {
            return Color.white;
        }

        var team = (int)spectatingTarget.CustomProperties[PlayerPropertyKeys.teamIDKey];
        if(team == -1)
        {
            return Color.white;
        }
        else if(team == 0)
        {
            return Color.blue;
        }
        else
        {
            return Color.red;
        }
    }
}
