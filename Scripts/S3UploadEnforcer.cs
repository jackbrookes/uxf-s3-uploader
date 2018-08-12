using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UXF_S3_Uploader
{

    /// <summary>
    /// This component enforces uploads of all files to your s3 bucket (ignoring any upload errors).
    /// </summary>
    public class S3UploadEnforcer : MonoBehaviour
    {
        S3Uploader uploader;

        [InspectorButton("ManualSafeEndAndQuit")]
        public bool manualSafeEndAndQuit;
        public UXF.Session session;
        public UnityEvent onFinishedEnforcing;
        
        
        void Awake()
        {
            uploader = GetComponent<S3Uploader>();
            Application.wantsToQuit += QuitHandler; // requires at least Unity 2018
        }

        /// <summary>
        /// For when we close a build
        /// </summary>
        /// <returns></returns>
        bool QuitHandler()
        {
            Application.wantsToQuit -= QuitHandler;
            StartCoroutine(_EnforceThenQuit());
            return false;
        }

        /// <summary>
        /// Useful to call in the editor where QuitHandler doesnt work.
        /// </summary>
        public void ManualSafeEndAndQuit()
        {
            Application.wantsToQuit -= QuitHandler;
            session.End();
            EnforceThenQuit();
        }

        void EnforceThenQuit()
        {
            StartCoroutine(_EnforceThenQuit());
        }

        IEnumerator _EnforceThenQuit()
        {
            yield return new WaitUntil(() => uploader.UploadingNum <= 0);
            Debug.Log("All files were uploaded, now quitting");
            // doesnt work in the editor
            Application.Quit();
        }

        /// <summary>
        /// Invokes an event only after all files have either finished uploading or failed in the process.
        /// This is designed to be assigned to the UXF.Session.onSessionEnd UnityEvent.
        /// </summary>
        public void EnforceThenInvokeEvent()
        {
            StartCoroutine(_EnforceThenInvokeEvent());
        }

        IEnumerator _EnforceThenInvokeEvent()
        {
            yield return new WaitUntil(() => (uploader.UploadingNum <= 0));
            onFinishedEnforcing.Invoke();
            Debug.Log("All files were uploaded, now invoking event");
        }

        void OnDestroy()
        {
            Application.wantsToQuit -= QuitHandler;
        }

    }

}