using System;
using System.IO;
using Reign;
using SGF.Logger;
using UnityEngine;
using UnityEngine.UI;
using Snaker.Service.UIManager;
using SGF.Network.Utils;
using BarcodeScanner;
using BarcodeScanner.Scanner;
using System.Collections;

namespace Snaker.UI.PVP
{
    public class UIRoomFindWin : UIWindow
    {
        public InputField inputIPPort;
        public Text txtScanResult;

        private Texture2D m_TexImage;
        private string m_ImageInfo = "";

        //qr camera scanner
        private IScanner BarcodeScanner;
        public RawImage Image;
        public AudioSource Audio;
        private float RestartTime;

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
        }


        private void StartScanner()
        {
            BarcodeScanner.Scan((barCodeType, barCodeValue) => {
                BarcodeScanner.Stop();
                txtScanResult.text = "";
                inputIPPort.text = "";
                txtScanResult.text += barCodeValue;
                inputIPPort.text += barCodeValue;
                RestartTime += Time.realtimeSinceStartup + 1f;

                // Feedback
                Audio.Play();

                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
                #endif
            });
        }

        public void OnBtnOK()
        {
            StartCoroutine(DestroyScanner(() =>
            {
            }));
            this.Close(inputIPPort.text);
        }

        public void OnBtnCancel()
        {
            StartCoroutine(DestroyScanner(() =>
            {
            }));
            this.Close(null);
        }

        void Update()
        {
            txtScanResult.text = m_ImageInfo;
            if (BarcodeScanner != null)
            {
                BarcodeScanner.Update();
            }
            // Check if the Scanner need to be started or restarted
            if (RestartTime != 0 && RestartTime < Time.realtimeSinceStartup)
            {
                StartScanner();
                RestartTime = 0;
            }

        }

        public void OnBtnScan()
        {
            m_TexImage = null;

#if UNITY_EDITOR
            //m_ImageInfo = "opening photos";
            //StreamManager.LoadFileDialog(FolderLocations.Pictures, 512, 512,
            //    new string[] { ".png", ".jpg", ".jpeg" }, OnImageLoaded);
            m_ImageInfo = "opening the camera";
            startToScanQRCode();
#else
            m_ImageInfo = "opening the camera";
            startToScanQRCode();
#endif
        }

        private void startToScanQRCode()
        {
            // Create a basic scanner
            BarcodeScanner = new Scanner();
            BarcodeScanner.Camera.Play();

            // Display the camera texture through a RawImage
            BarcodeScanner.OnReady += (sender, QRarg) => {
                // Set Orientation & Texture
                Image.transform.localEulerAngles = BarcodeScanner.Camera.GetEulerAngles();
                Image.transform.localScale = BarcodeScanner.Camera.GetScale();
                Image.texture = BarcodeScanner.Camera.Texture;

                // Keep Image Aspect Ratio
                var rect = Image.GetComponent<RectTransform>();
                var newHeight = rect.sizeDelta.x * BarcodeScanner.Camera.Height / BarcodeScanner.Camera.Width;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);

                RestartTime = Time.realtimeSinceStartup;
            };
        }

        private IEnumerator DestroyScanner(Action callback)
        {
            // Stop Scanning
            Image = null;
            if (BarcodeScanner != null)
            {
                if(BarcodeScanner.Camera.IsPlaying())
                    BarcodeScanner.Camera.Stop();
                BarcodeScanner.Camera.Destroy();
                BarcodeScanner.Destroy();
                BarcodeScanner = null;
            }
            // Wait a bit
            yield return new WaitForSeconds(0.1f);
        }

        private void OnImageLoaded(Stream stream, bool succeeded)
        {
            if (!succeeded)
            {
                m_ImageInfo = "fail to open camera";
                this.LogError("OnImageLoaded()" + m_ImageInfo);


                if (stream != null)
                {
                    stream.Dispose();
                }
                return;
            }

            try
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                var newImage = new Texture2D(512, 512);
                newImage.LoadImage(data);
                newImage.Apply();
                m_TexImage = newImage;

                string content = QRCodeUtils.DecodeFromImage(m_TexImage);
                if (string.IsNullOrEmpty(content))
                {
                    m_ImageInfo = "fail to decode QR code!";
                }
                else
                {
                    m_ImageInfo = "QR code decoding succuessful!";
                }

                inputIPPort.text = content;
            }
            catch (Exception e)
            {
                m_ImageInfo = "Error:" + e.Message;
                this.LogError("OnImageLoaded() " + m_ImageInfo);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

        }
    }
}

