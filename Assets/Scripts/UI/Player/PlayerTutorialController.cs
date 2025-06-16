using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;


public class PlayerTutorialController : NetworkBehaviour
{

    public event Action<TutorialData> OnTutorialSelected;
    [SerializeField] private TutorialInfoSO[] tutorialInfo;
    private TutorialData selectedTutorialData;


    [Command("TriggerTutorial", MonoTargetType.All)]
    public void HandleOnItemSelectedSO(int itemId)
    {
        if (!IsOwner) return;

        for (int i = 0; i < tutorialInfo.Length; i++)
        {
            if (itemId == tutorialInfo[i].itemId)
            {
                selectedTutorialData = new TutorialData
                {
                    tutorialVideo = tutorialInfo[i].tutorialVideo,
                    tutorialTitle = tutorialInfo[i].tutorialTitle
                };

                OnTutorialSelected?.Invoke(selectedTutorialData);
                return;
            }
        }
    }
}

public struct TutorialData
{
    public VideoClip tutorialVideo;
    public string tutorialTitle;
}
