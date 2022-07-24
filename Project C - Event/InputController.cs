using App.Messages;
using Next.Core;
using Next.Core.Globals;
using Next.Core.OSC;
using System;
using UnityEngine;
using static Next.Core.MathLib;


namespace App.Main
{
    #region Enums
    public enum Scenes
    {
        None,
        Industry,
        Shop,
        Office
    }

    public enum InputDevices
    {
        None,
        Keyboard,
        MagnetTrack,
        Powermate
    }
    #endregion

    public class InputController : MonoBehaviour
    {
        public bool InputActive = true;

        [Range(0, 1)]
        public float CurrentTime;
        private InputDevices _inputDevice;
        private bool _mirrorTrack;
        private float _oscValue;
        private DynamicFloat _currentTime;

        // RFID
        private OscIn _oscRFID;

        // Keyboard
        public float KeyboardSpeed = 0.003f;
        private KeyCode _keyLeft = KeyCode.LeftArrow;
        private KeyCode _keyRight = KeyCode.RightArrow;
        private KeyCode _keyLanguage = KeyCode.L;

        // Magnet Track
        private float _valueLost;
        private float _lastValue;
        private int _bigSetpCount;
        private OscIn _oscInputMagnet;

        //Powermate
        private OscIn _powermateLeft;
        private OscIn _powermateRight;
        private OscIn _powermateLanguage;
        private float _powermateSpeed;

        // Screensaver
        private float _screensaverTime = 0;
        private float _lastInput = 0;
        private bool _isScreensaver = false;

        public void ResetTime()
        {
            CurrentTime = 0;
            _oscValue = 0;
            _currentTime.Reset(0);
        }

        void OnEnable()
        {
            _currentTime = new DynamicFloat(0, GlobalsManager.GetFloat("Globals", "General", "SpeedMax"), GlobalsManager.GetFloat("Globals", "General", "Damping"));
            _screensaverTime = GlobalsManager.GetFloat("Globals", "General", "ScreensaverTime");
            Enum.TryParse(GlobalsManager.GetString("Globals", "General", "InputDevice"), out _inputDevice);
            _mirrorTrack = GlobalsManager.GetBool("Globals", "General", "MirrorTrack");

            KeyboardSpeed = GlobalsManager.GetFloat("Globals", "Keyboard", "Speed");
            Enum.TryParse(GlobalsManager.GetString("Globals", "Keyboard", "KeyCodeLeft"), out _keyLeft);
            Enum.TryParse(GlobalsManager.GetString("Globals", "Keyboard", "KeyCodeRight"), out _keyRight);
            Enum.TryParse(GlobalsManager.GetString("Globals", "Keyboard", "KeyCodeLanguage"), out _keyLanguage);

            InitializeRFID();

            switch (_inputDevice)
            {
                case InputDevices.MagnetTrack:
                    InitializeMagnetTrack();
                    break;
                case InputDevices.Powermate:
                    InitializePowermate();
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            Messenger.Invoke(TimelineEvents.SetTime, _currentTime.Compute(Time.deltaTime, CurrentTime));

            if (!_isScreensaver && Time.time - _lastInput > _screensaverTime)
            {
                CurrentTime = 0;
                _oscValue = 0;
                _isScreensaver = true;
            }

            if (!InputActive)
                return;

            switch (_inputDevice)
            {
                case InputDevices.Keyboard:
                    if (Input.GetKey(_keyLeft))
                    {
                        UpdateScreensaverValues();
                        CurrentTime = Clamp01(CurrentTime - KeyboardSpeed * Time.deltaTime);
                    }
                    else if (Input.GetKey(_keyRight))
                    {
                        UpdateScreensaverValues();
                        CurrentTime = Clamp01(CurrentTime + KeyboardSpeed * Time.deltaTime);
                    }
                    break;
                case InputDevices.MagnetTrack:
                case InputDevices.Powermate:
                    CurrentTime = Clamp01(_oscValue);
                    break;
                default:
                    break;
            }

            if (Input.GetKeyDown(_keyLanguage))
            {
                UpdateScreensaverValues();
                Messenger.Invoke(MainEvents.LanguageSwitch);
            }

            #region Testing
            // Scenes
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UpdateScreensaverValues();
                Messenger.Invoke(MainEvents.SetScene, Scenes.Industry);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UpdateScreensaverValues();
                Messenger.Invoke(MainEvents.SetScene, Scenes.Shop);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UpdateScreensaverValues();
                Messenger.Invoke(MainEvents.SetScene, Scenes.Office);
            }
            #endregion
        }

        private void UpdateScreensaverValues()
        {
            _lastInput = Time.time;
            _isScreensaver = false;
        }


        #region RFID
        public void InitializeRFID()
        {
            _oscRFID = new OscIn(GlobalsManager.GetInt("Globals", "RFID", "Port"),
                                        GlobalsManager.GetString("Globals", "RFID", "Address"));
            _oscRFID.MessageReceived += RFIDValueReceived;
            _oscRFID.Add<string>("ID");
        }

        private void RFIDValueReceived(OscIn sender)
        {
            if (_oscRFID.GetMessageCount() == 0)
                return;

            string rfid = _oscRFID.Get<string>("ID", 0);
            UpdateScreensaverValues();

            if (ProjectLoader.RfidToScene.ContainsKey(rfid))
            {
                Messenger.Invoke(MainEvents.SetScene, ProjectLoader.RfidToScene[rfid]);
            }
        }
        #endregion

        #region MagnetTrack
        public void InitializeMagnetTrack()
        {
            _oscInputMagnet = new OscIn(GlobalsManager.GetInt("Globals", "MagnetTrackOSC", "Port"),
                                        GlobalsManager.GetString("Globals", "MagnetTrackOSC", "Address"));
            _oscInputMagnet.MessageReceived += MagnetTrackValueReceived;
            _oscInputMagnet.Add<float>("ShiftPosition");

            _valueLost = GlobalsManager.GetFloat("Globals", "MagnetTrackOSC", "ValueLost");
        }

        private void MagnetTrackValueReceived(OscIn sender)
        {
            if (_oscInputMagnet.GetMessageCount() == 0)
                return;

            float value = _oscInputMagnet.Get<float>("ShiftPosition", 0);

            if (value < _valueLost)
                return;

            if (Math.Abs(value - _lastValue) > 0.2f && _bigSetpCount < 5)
            {
                _bigSetpCount++;
            }
            else
            {
                _lastValue = value;
                _bigSetpCount = 0;
                UpdateScreensaverValues();

                if (_mirrorTrack)
                {
                    _oscValue = 1 - value;
                }
                else
                {
                    _oscValue = value;
                }
            }
        }
        #endregion

        #region Powermate
        private void InitializePowermate()
        {
            int port = GlobalsManager.GetInt("Globals", "OSC", "Port");

            _powermateLeft = new OscIn(port, GlobalsManager.GetString("Globals", "OSC", "EncoderLeft"));
            _powermateLeft.MessageReceived += PowermateLeftReceived;

            _powermateRight = new OscIn(port, GlobalsManager.GetString("Globals", "OSC", "EncoderRight"));
            _powermateRight.MessageReceived += PowermateRightReceived;

            _powermateLanguage = new OscIn(port, GlobalsManager.GetString("Globals", "OSC", "ButtonPress"));
            _powermateLanguage.MessageReceived += PowermateLanguageReceived;

            _powermateSpeed = GlobalsManager.GetFloat("Globals", "OSC", "Speed");
        }

        private void PowermateLeftReceived(OscIn sender)
        {
            UpdateScreensaverValues();
            if (!_mirrorTrack)
            {
                _oscValue = Mathf.Clamp01(_oscValue - _powermateSpeed);
            }
            else
            {
                _oscValue = Mathf.Clamp01(_oscValue + _powermateSpeed);
            }
        }

        private void PowermateRightReceived(OscIn sender)
        {
            UpdateScreensaverValues();
            if (!_mirrorTrack)
            {
                _oscValue = Mathf.Clamp01(_oscValue + _powermateSpeed);
            }
            else
            {
                _oscValue = Mathf.Clamp01(_oscValue - _powermateSpeed);
            }
        }

        private void PowermateLanguageReceived(OscIn sender)
        {
            if (InputActive)
            {
                Messenger.Invoke(MainEvents.LanguageSwitch);
                UpdateScreensaverValues();
            }
        }
        #endregion
    }
}