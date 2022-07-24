using Next.Animation.StateControl;
using Next.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] Levels;

    private int _activeLevelNumber = 0;
    private int _activeLevelCloudCount = 0;
    private int _cloudsDestroyed;

    private Messenger.Descriptor[] _messages;

    private void OnDisable()
    {
        Messenger.RemoveListener(_messages);
    }

    private void OnEnable()
    {
        _messages = new Messenger.Descriptor[]
            {
                new Messenger.Descriptor( App.Messages.GameMessages.LevelStart, OnLevelStart ),
                new Messenger.Descriptor( App.Messages.GameMessages.LevelSwitch, OnLevelSwitch ),
                new Messenger.Descriptor(App.Messages.GameMessages.CloudDestroy, OnCloudDestroy),
                new Messenger.Descriptor( App.Messages.GameMessages.GameOver, OnGameOver ),
            };
        Messenger.AddListener(_messages);
    }

    private void OnGameOver(object obj)
    {
        _cloudsDestroyed = 0;
    }

    private void OnCloudDestroy(object obj)
    {
        _cloudsDestroyed++;

        if (_activeLevelCloudCount == _cloudsDestroyed)
        {
            Messenger.Invoke(App.Messages.GameMessages.LevelDone, _activeLevelNumber);
            Messenger.Invoke(App.Messages.GameMessages.BallSetSpeed, 1f);
            _cloudsDestroyed = 0;
        }
        else
        {
            Messenger.Invoke(App.Messages.GameMessages.BallSetSpeed, ((float)_cloudsDestroyed / (float)_activeLevelCloudCount) + 1f);
        }
    }

    [Button]
    private void SwitchLevelTest(int number)
    {
        OnLevelSwitch(number);
    }

    private void OnLevelSwitch(object obj)
    {
        StartCoroutine(SwitchLevel((int)obj));
    }

    private void OnLevelStart(object obj)
    {
        Messenger.Invoke(App.Messages.GameMessages.BallLaunch);
    }

    private IEnumerator SwitchLevel(int number)
    {
        if (number < Levels.Length && number >= 0)
        {
            Messenger.Invoke(App.Messages.GameMessages.LevelTransition, new StateControlSimple.TriggerStateMsg("LevelOut"));
            yield return new WaitForSeconds(1.1f);
            Levels[_activeLevelNumber].SetActive(false);
            _activeLevelNumber = number;
            _activeLevelCloudCount = Levels[_activeLevelNumber].transform.childCount;
            Levels[_activeLevelNumber].SetActive(true);
            yield return new WaitForEndOfFrame();
            Messenger.Invoke(App.Messages.GameMessages.LevelReset);
            Messenger.Invoke(App.Messages.GameMessages.LevelTransition, new StateControlSimple.TriggerStateMsg("LevelIn"));
        }
        else
        {
            Messenger.Invoke(App.Messages.GameMessages.GameOver);
        }
        yield return new WaitForEndOfFrame();
    }
}
