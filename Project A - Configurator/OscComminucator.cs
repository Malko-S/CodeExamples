///////////////////////////////////////////////////////////////////////////////////////////
// OscComminucator
// 
// Responsible for all sending and receiving from OSC.
// 
// Author: Malko Sichelschmidt
// Created: 10.2019
// Modified: 06.10.2019
///////////////////////////////////////////////////////////////////////////////////////////

using Frameworks.ContentLoaderFramework;
using Frameworks.Core;
using System;
using UnityEngine;
using winOSC;

namespace App.Common
{
    public class OscComminucator : MonoBehaviour
    {
        public SC_OscLanguage OSCCurrentLanguage;
        public SO_OscContentState OSCContentState;
        public SO_OscNewConfigFile OSCConfigFile;
        public SO_OscReady OSCReadyState;
        public SC_OscFlipScreen OSCFlipScreenState;

        // Sending
        private OSCOut _oscOutEmailReady;
        private OSCOut _oscOutTouchReady;
        private OSCOut _oscOutAreaSelected;

        //Receiving
        private OSCIn _oscInImReady;
        private OSCIn _oscInContentState;
        private OSCIn _oscInNewConfigFile;
        private OSCIn _oscInSetLanguage;
        private OSCIn _oscInFlipScreen;

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
                new Messenger.Descriptor(OscEvents.SendRUReady, OscSendRUReady ),
                new Messenger.Descriptor(OscEvents.SendAreaSelected, OscSendAreaSelected ),
            };
            Messenger.AddListeners(_messages);
        }
        #endregion

        //Initialize
        private void OnActivateScene(object[] obj)
        {
            XMLAsset xml = ContentLoader.GetGlobalAsset<XMLAsset>("Settings_Shared");
            
            // Sending
            _oscOutEmailReady = new OSCOut(xml.root["Osc"].AttrValue<string>("ipMailProxy"),
                                          xml.root["Osc"].AttrValue<int>("portRUReady"),
                                          xml.root["Osc"].AttrValue<string>("addressRUReady"));

            _oscOutTouchReady = new OSCOut(xml.root["Osc"].AttrValue<string>("ipTouchScreen"),
                                          xml.root["Osc"].AttrValue<int>("portRUReady"),
                                          xml.root["Osc"].AttrValue<string>("addressRUReady"));

            _oscOutAreaSelected = new OSCOut(xml.root["Osc"].AttrValue<string>("ipTouchScreen"),
                                            xml.root["Osc"].AttrValue<int>("portSelectedArea"),
                                            xml.root["Osc"].AttrValue<string>("addressSelectedArea"));
            _oscOutAreaSelected.SendOnChange = false;
            _oscOutAreaSelected.AddInt("areaId");
            _oscOutAreaSelected.AddInt("stadiumId");

            //Receiving  
            _oscInImReady = new OSCIn(xml.root["Osc"].AttrValue<int>("portIMReady"), 
                                      xml.root["Osc"].AttrValue<string>("addressIMReady"));
            _oscInImReady.AddInt("appId");
            _oscInImReady.AddString("appName");
            _oscInImReady.MessageReceived += OscReadyReceived;

            _oscInContentState = new OSCIn(xml.root["Osc"].AttrValue<int>("portContentState"),
                                           xml.root["Osc"].AttrValue<string>("addressContentState"));
            _oscInContentState.AddInt("stateId");
            _oscInContentState.AddInt("stadiumId");
            _oscInContentState.MessageReceived += OscContentStateReceived;

            _oscInNewConfigFile = new OSCIn(xml.root["Osc"].AttrValue<int>("portNewConfigFile"),
                                            xml.root["Osc"].AttrValue<string>("addressNewConfigFile"));
            _oscInNewConfigFile.AddString("filePath");
            _oscInNewConfigFile.MessageReceived += OscNewConfigFileReceived;

            _oscInSetLanguage = new OSCIn(xml.root["Osc"].AttrValue<int>("portSetLanguage"),
                                          xml.root["Osc"].AttrValue<string>("addressSetLanguage"));
            _oscInSetLanguage.AddString("language");
            _oscInSetLanguage.MessageReceived += OscLanguageReceived;

            _oscInFlipScreen = new OSCIn(xml.root["Osc"].AttrValue<int>("portFlipScreen"),
                                         xml.root["Osc"].AttrValue<string>("addressFlipScreen"));
            _oscInFlipScreen.AddInt("isFlipped");
            _oscInFlipScreen.MessageReceived += OscFlipScreenReceived;
        }

        // Sending
        private void OscSendRUReady(object[] obj)
        {
            _oscOutEmailReady.Send();
            _oscOutTouchReady.Send();
        }

        private void OscSendAreaSelected(object[] obj)
        {
            _oscOutAreaSelected.Arguments.areaId = Convert.ToInt32(obj[0]);
            _oscOutAreaSelected.Arguments.stadiumId = Convert.ToInt32(obj[1]);
            _oscOutAreaSelected.Send();
        }

        //Receiving 
        private void OscReadyReceived(OSCIn sender)
        {
            //Debug.Log("ReadyReceived - " + _oscInImReady.Arguments.appId + "." + _oscInImReady.Arguments.appName);

            if (_oscInImReady.Arguments.appId == 2)
            {
                OSCReadyState.EmailReady = true;
            }

            if (_oscInImReady.Arguments.appId == 0)
            {
                OSCReadyState.TouchReady = true;
            }
        }

        private void OscContentStateReceived(OSCIn sender)
        {
            //Debug.Log("NewStateReceived - " + _oscInContentState.Arguments.stateId + "." + _oscInContentState.Arguments.stadiumId);
            OSCContentState.StateId = _oscInContentState.Arguments.stateId;
            OSCContentState.StadiumId = _oscInContentState.Arguments.stadiumId;
            OSCContentState.Update = true;
        }

        private void OscNewConfigFileReceived(OSCIn sender)
        {
            //Debug.Log("NewConfigFileReceived - " + _oscInNewConfigFile.Arguments.filePath);
            OSCConfigFile.FilePath = _oscInNewConfigFile.Arguments.filePath;
            OSCConfigFile.Update = true;
        }

        private void OscLanguageReceived(OSCIn sender)
        {
            //Debug.Log("Language received - " + _oscInImReady.Arguments.language);
            OSCCurrentLanguage.CurrentLanguage = _oscInSetLanguage.Arguments.language;
            OSCCurrentLanguage.Update = true;
        }

        private void OscFlipScreenReceived(OSCIn sender)
        {
            //Debug.Log("Flip Screen Received: " + _oscInFlipScreen.Arguments.isFlipped);
            OSCFlipScreenState.FlipScreen = _oscInFlipScreen.Arguments.isFlipped == 1;
            OSCFlipScreenState.Update = true;
        }
    }
}