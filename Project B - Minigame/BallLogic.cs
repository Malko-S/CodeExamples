using Next.Animation.StateControl;
using Next.Core;
using Next.Core.Globals;
using Sirenix.OdinInspector;
using UnityEngine;

public class BallLogic : MonoBehaviour
{
    [FoldoutGroup("Dependencies")]
    public ParticleSystem Twinkle;
    //[FoldoutGroup("Dependencies")]
    //public ParticleSystem Poof;
    [FoldoutGroup("Dependencies")]
    public Transform BallSprite;

    [Range(-1,1)]
    public float _rotateDirection = 0;
    private float _rotateSpeed = 3;
    private float _speedCurrent;
    private float _speedBase;
    private float _speedMultipiler = 1f;
    private Vector2 _currentDirection = new Vector2(0, 0);
    private Messenger.Descriptor[] _messages;

    private void OnDisable()
    {
        Messenger.RemoveListener(_messages);
    }

    private void OnEnable()
    {
        _messages = new Messenger.Descriptor[]
            {
                new Messenger.Descriptor( App.Messages.GameMessages.BallLaunch, OnLaunch ),
                new Messenger.Descriptor( App.Messages.GameMessages.BallReset, OnReset ),
                new Messenger.Descriptor( App.Messages.GameMessages.BallGone, OnGone ),
                new Messenger.Descriptor( App.Messages.GameMessages.CoinBurstBegin, OnCoinBurstBegin ),
                new Messenger.Descriptor( App.Messages.GameMessages.CoinBurstEnd, OnCoinBurstEnd ),
                new Messenger.Descriptor( App.Messages.GameMessages.BallSetSpeed, OnSetSpeed ),
                new Messenger.Descriptor( App.Messages.GameMessages.GameApplySettings, OnApplySettings)
            };
        Messenger.AddListener(_messages);
    }

    private void OnApplySettings(object obj)
    {
        _speedBase = GlobalsManager.GetFloat("Globals", "GameSettings", "SpeedBall");
        _speedMultipiler = 1f;
    }

    private void OnSetSpeed(object obj)
    {
        _speedMultipiler = (float)obj;
        _speedCurrent = _speedBase * _speedMultipiler;
    }

    private void OnCoinBurstEnd(object obj)
    {
        Messenger.Invoke(App.Messages.GameMessages.BallAnimate, new StateControlSimple.TriggerStateMsg("CoinBurstOff"));
        Twinkle.Stop();
    }

    private void OnCoinBurstBegin(object obj)
    {
        Messenger.Invoke(App.Messages.GameMessages.BallAnimate, new StateControlSimple.TriggerStateMsg("CoinBurstOn"));
        Twinkle.Play();
    }

    private void OnGone(object obj)
    {
        Messenger.Invoke(App.Messages.GameMessages.BallAnimate, new StateControlSimple.TriggerStateMsg("Dissapear"));
        _currentDirection = new Vector2(0, 0);
        _speedCurrent = 0;
        transform.position = new Vector3(10.72f, 0, 0);
    }

    private void OnLaunch(object obj)
    {
        float x = Random.Range(-40, 41);
        _speedCurrent = _speedBase * _speedMultipiler;
        _currentDirection = new Vector2(_speedCurrent * x, 1);
    }

    private void OnReset(object obj)
    {
        Messenger.Invoke(App.Messages.GameMessages.BallAnimate, new StateControlSimple.TriggerStateMsg("Appear"));
        _currentDirection = new Vector2(0, 0);
        _speedCurrent = 0;
        _rotateDirection = 0;
        transform.position = new Vector3(0, -2.5f, 0);
        BallSprite.rotation = new Quaternion();
    }

    private void FixedUpdate()
    {
        transform.Translate(_currentDirection.normalized * _speedCurrent);
        BallSprite.Rotate(0, 0, _rotateDirection * _rotateSpeed);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 oldDirection = _currentDirection;

        if (other.transform.tag.Equals("Pit"))
        {
            Messenger.Invoke(App.Messages.GameMessages.BallLost);
            Messenger.Invoke(App.Messages.GameMessages.BallAnimate, new StateControlSimple.TriggerStateMsg("Dissapear"));
            _speedCurrent = 0;
        }

        if (other.transform.tag.Equals("Wall") || other.transform.tag.Equals("Cloud"))
        {
            _currentDirection = Vector2.Reflect(_currentDirection, other.contacts[0].normal);
        }

        if (other.transform.tag.Equals("Winni"))
        {
            Vector2 newDirection = Vector2.Reflect(_currentDirection, other.contacts[0].normal);
            if (newDirection.y < 0)
            {
                _currentDirection = other.contacts[0].normal;
            }
            else
            {
                _currentDirection = newDirection;
            }
        }

        _rotateDirection = (Vector2.Angle(oldDirection, _currentDirection)-90)/90;

        if (_currentDirection.y >= 0 && _currentDirection.y < 0.2f)
            _currentDirection.y = 0.2f;

        if (_currentDirection.y < 0 && _currentDirection.y > -0.2f)
            _currentDirection.y = -0.2f;
    }

}
