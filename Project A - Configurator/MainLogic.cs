///////////////////////////////////////////////////////////////////////////////////////////
// MainLogic
// 
// Stets states and colors fields. Is also responsible for selecting single Fields on Arenas.
// 
// Author: Malko Sichelschmidt
// Created: 10.2019
// Modified: 06.10.2019
///////////////////////////////////////////////////////////////////////////////////////////

using Frameworks.ContentLoaderFramework;
using Frameworks.Core;
using Frameworks.InputFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Common
{
    public class MainLogic : MonoBehaviour
    {
        public SO_OscContentState OSCContentState;
        public SO_OscNewConfigFile OSCConfigFile;
        public SC_OscFlipScreen OSCFlipScreenState;
        public CanvasManager CanvasManagerInstance;
        public Transform InputCamera3D;
        public Transform InputCameraUI;

        private int _curentArenaId;
        private int _CurrentFieldId;
        private bool _isDisco = false;
        private List<ColorMixFraction> _black = new List<ColorMixFraction> { new ColorMixFraction(30, 1, 0) };
        private List<List<ColorMixFraction>> _sampleDisco = new List<List<ColorMixFraction>>();

        #region Unity Messages
        private Messenger.Descriptor[] _messages;

        private void OnDisable()
        {
            Messenger.RemoveListeners(_messages);
        }

        private void OnEnable()
        {
            _messages = new Messenger.Descriptor[]
            {
                new Messenger.Descriptor(LogicEvents.ActivateContentScene, OnActivateScene ),
                new Messenger.Descriptor( "3x3Grid", "Released" , On3x3GridReleased ),
                new Messenger.Descriptor( "Stylemaker3d", "Released" , OnStylemaker3dReleased ),
                new Messenger.Descriptor( "PlayGround", "Released" , OnPlayGroundReleased ),
                new Messenger.Descriptor( "AthleticTrack", "Released" , OnAthleticTrackReleased ),
                new Messenger.Descriptor( "TennisCourt", "Released" , OnTennisCourtReleased ),
                new Messenger.Descriptor( "BasketballCourt", "Released" , OnBasketballCourtReleased ),
                new Messenger.Descriptor( "Input Manager", "Any Interaction", OnAnyInput),
            };
            Messenger.AddListeners(_messages);
        }
        #endregion

        private void Update()
        {
            if (OSCContentState.Update)
            {
                OSCContentState.Update = false;
                OnContentStateReceived(OSCContentState.StateId, OSCContentState.StadiumId);
            }

            if (OSCConfigFile.Update)
            {
                OSCConfigFile.Update = false;
                if (_CurrentFieldId > 0)
                {
                    ColorSubarea(OSCConfigFile.FilePath, false);
                    ColorSamples(OSCConfigFile.FilePath);
                }
                else
                {
                    ColorArena(OSCConfigFile.FilePath);
                }
            }

            if (OSCFlipScreenState.Update)
            {
                OSCFlipScreenState.Update = false;
                if (OSCFlipScreenState.FlipScreen)
                {
                    Messenger.Invoke("UI", "Rotate", "Up");
                    InputCamera3D.localRotation = new Quaternion(0, 0, 180, 0);
                    InputCameraUI.localRotation = new Quaternion(0, 0, 180, 0);
                }
                else
                {
                    Messenger.Invoke("UI", "Rotate", "Down");
                    InputCamera3D.localRotation = new Quaternion(0, 0, 0, 0);
                    InputCameraUI.localRotation = new Quaternion(0, 0, 0, 0);
                }
            }
        }


        private void OnActivateScene(object[] obj)
        {
            StartCoroutine(WaitAndApplySettings());

        }
        private IEnumerator WaitAndApplySettings()
        {
            yield return new WaitForSeconds(0.1f);
            ColorAllArenas();
            ColorSamplesBlack();
            Messenger.Invoke(CarouselEvents.GetBirdView);
            for (int i = 0; i <= 23; i++)
            {
                _sampleDisco.Add(new List<ColorMixFraction> { new ColorMixFraction(i, 1, 0) });
            }
            _isDisco = true;
            StartCoroutine(TweenDisco());
        }

        private void OnContentStateReceived(int state, int stadium)
        {
            CanvasManagerInstance.SetReference(stadium);
            switch (state)
            {
                case 1: // Idle 
                    ColorAllArenas();
                    _isDisco = true;
                    Messenger.Invoke("Projection", "UI", "NotSelected", "BlendOut");
                    Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendOut");
                    Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
                    Messenger.Invoke(CarouselEvents.SetSelectionId, 0);
                    Messenger.Invoke(CarouselEvents.GetBirdView);
                    _curentArenaId = 0;
                    _CurrentFieldId = 0;
                    Messenger.Invoke("Projection", "UI", "LineButton", "BlendOut");
                    InputManager.SetStateGroup("Input", false);
                    break;

                case 2: // Preview
                    ColorSamplesBlack();
                    _isDisco = false;
                    Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendOut");
                    Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
                    Messenger.Invoke(CarouselEvents.SetSelectionId, stadium);
                    Messenger.Invoke(CarouselEvents.GetBirdView);
                    _curentArenaId = stadium;
                    _CurrentFieldId = 0;
                    Messenger.Invoke("Projection", "UI", "LineButton", "BlendOut");
                    InputManager.SetStateGroup("Input", false);
                    break;

                case 3: // Entdecken
                    ColorSamplesBlack();
                    _isDisco = false;
                    Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendOut");
                    Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
                    Messenger.Invoke(CarouselEvents.SetSelectionId, stadium);
                    Messenger.Invoke(CarouselEvents.GetDetailView);
                    _curentArenaId = stadium;
                    _CurrentFieldId = 0;
                    Messenger.Invoke("Projection", "UI", "LineButton", "BlendOut");
                    InputManager.SetStateGroup("Input", false);
                    break;

                case 4: // Erstellen
                    ColorSamplesBlack();
                    _isDisco = false;
                    Messenger.Invoke(CarouselEvents.SetSelectionId, stadium);
                    Messenger.Invoke(CarouselEvents.GetDetailView);
                    _curentArenaId = stadium;
                    _CurrentFieldId = 0;

                    if (_curentArenaId <= 3)
                    {
                        Messenger.Invoke("Projection", "UI", "LineButton", "BlendIn");
                    }
                    Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendIn");
                    InputManager.SetStateGroup("Input", true);
                    break;

                case 5: // Email vorbereiten
                    Messenger.Invoke("Projection", "UI", "NotSelected", "BlendOut");
                    ColorSamplesBlack();
                    _isDisco = false;
                    Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendOut");
                    Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
                    Messenger.Invoke(CarouselEvents.SetSelectionId, stadium);
                    Messenger.Invoke(CarouselEvents.GetBirdView);
                    _curentArenaId = stadium;
                    _CurrentFieldId = 0;
                    Messenger.Invoke("Projection", "UI", "LineButton", "BlendOut");
                    InputManager.SetStateGroup("Input", false);
                    break;

                default:
                    break;
            }
        }

        private void OnAnyInput(object[] obj)
        {
            Messenger.Invoke("Projection", "UI", "TouchInfo", "BlendOut");
        }

        private void OnBasketballCourtReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 1);
        }

        private void OnTennisCourtReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 2);
        }

        private void OnAthleticTrackReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 3);
        }

        private void OnPlayGroundReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 4);
        }

        private void OnStylemaker3dReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 5);
        }

        private void On3x3GridReleased(object[] obj)
        {
            SelectSubarea(Convert.ToInt32(obj[0]), 6);
        }

        private void SelectSubarea(int fieldId, int arenaId)
        {
            if (_CurrentFieldId == fieldId)
            {
                Messenger.Invoke("ColorMixer" + arenaId, "SelectArea", 0);
                _CurrentFieldId = 0;
                ColorSamplesBlack();
            }
            else
            {
                Messenger.Invoke("ColorMixer" + arenaId, "SelectArea", fieldId);
                _CurrentFieldId = fieldId;
            }

            Messenger.Invoke(OscEvents.SendAreaSelected, _CurrentFieldId, arenaId);
        }


        private void ColorSamplesBlack()
        {
            Messenger.Invoke("ColorMixerSamples", "SetColorMixFraction", _black);
        }

        private System.Random bla = new System.Random();

        private IEnumerator TweenDisco()
        {
            while (true)
            {
                if (_isDisco && _sampleDisco.Count > 0)
                {
                    Messenger.Invoke("ColorMixerSamples", "SetColorMixFraction", _sampleDisco[bla.Next(0, _sampleDisco.Count)]);
                }

                yield return new WaitForSeconds(3);
            }
        }

        private void ColorSamples(string path)
        {
            List<ColorMixFraction> colors;
            if (_CurrentFieldId == 0)
            {
                colors = _black;
            }
            else
            {
                colors = ConfigGranulatLoader.GetAreaColorMixFractions(path, _CurrentFieldId);
                if (colors.Count == 0)
                {
                    colors = _black;
                }
            }

            Messenger.Invoke("ColorMixerSamples", "SetColorMixFraction", colors);
        }

        private void ColorSubarea(string path, bool deselect)
        {
            List<ColorMixFraction> colors = ConfigGranulatLoader.GetAreaColorMixFractions(path, _CurrentFieldId);
            if (colors.Count == 0)
            {
                InputManager.SetStateGroup("Input", false);
                Messenger.Invoke("Projection", "UI", "NotSelected", "BlendIn");
                colors = _black;
            }
            else
            {
                InputManager.SetStateGroup("Input", true);
                Messenger.Invoke("Projection", "UI", "NotSelected", "BlendOut");
            }

            Messenger.Invoke("ColorMixer" + _curentArenaId, "SetColorMixFraction", colors);

            if (deselect)
            {
                Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
            }
        }

        private void ColorArena(string path)
        {
            List<List<ColorMixFraction>> colors = ConfigGranulatLoader.GetAllColorMixFractions(path, false);

            for (int j = 1; j <= colors.Count; j++)
            {
                Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", j);
                Messenger.Invoke("ColorMixer" + _curentArenaId, "SetColorMixFraction", colors[j - 1]);
            }

            Messenger.Invoke("ColorMixer" + _curentArenaId, "SelectArea", 0);
        }

        private void ColorAllArenas()
        {
            for (int i = 1; i <= 6; i++)
            {
                string path = ContentLoader.GetGlobalAssets<XMLAsset>(false, "preset_configuration_" + i + ".1_*")[0].GetAssetPath();
                List<List<ColorMixFraction>> colors = ConfigGranulatLoader.GetAllColorMixFractions(path, true);

                for (int j = 1; j <= colors.Count; j++)
                {
                    Messenger.Invoke("ColorMixer" + i, "SelectArea", j);
                    Messenger.Invoke("ColorMixer" + i, "SetColorMixFraction", colors[j - 1]);
                }
                Messenger.Invoke("ColorMixer" + i, "SelectArea", 0);
            }
        }


    }
}