using UnityEngine;
using System.Collections;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using System;
using Amazon.S3.Util;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using Amazon;

namespace UXF_S3_Uploader
{

    public class S3Uploader : MonoBehaviour
    {
        /* 
        The credential asset should be assigned in the inspector and contain the following information:

            Identity Pool ID
            Cognito identity region string
            S3 region string

        Region strings can be found here https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/Concepts.RegionsAndAvailabilityZones.html
        */
        public Credentials s3Credentials;
        public S3Bucket s3Bucket;
        /// <summary>
        /// This is the name of the folder within the bucket
        /// </summary>
        public string experimentGroup = "UXF_S3_Uploader";
        public LogLevel logLevel = LogLevel.Normal;

        /// <summary>
        /// Number of file uploads in progress.
        /// </summary>
        /// <value></value>
        public int UploadingNum { get { return uploadingSet.Count; } }

        private bool isReady = false;
        private AmazonS3Client s3Client;
        private AWSCredentials awsCredentials;
        private HashSet<string> uploadingSet = new HashSet<string>();

        void Start()
        {
            UnityInitializer.AttachToGameObject(transform.root.gameObject);
            AWSConfigsS3.UseSignatureVersion4 = true;
            AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
            if (logLevel >= LogLevel.Debugging)
            {
                AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
                AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.OnError;
                AWSConfigs.LoggingConfig.LogMetrics = true;
            }
            AWSConfigs.CorrectForClockSkew = true;

            awsCredentials = new CognitoAWSCredentials(
                s3Credentials.identityPoolID,
                RegionEndpoint.GetBySystemName(s3Credentials.cognitoIdentityRegion)
                );

            s3Client = new AmazonS3Client(
                awsCredentials,
                RegionEndpoint.GetBySystemName(s3Credentials.s3Region)
                );

            isReady = true;
            
            s3Client.ExceptionEvent += HandleException;
        }

        public void Upload(UXF.WriteFileInfo writeFileInfo)
        {
            if (!isReady)
                throw new InvalidOperationException("Trying to upload before credentials are set up");

            List<Tag> tags = new List<Tag>()
            {
                new Tag(){ Key = "Source", Value = "UXF S3 Uploader" },
                new Tag(){ Key = "UXF Filetype", Value = writeFileInfo.fileType.ToString() }
            };

            var stream = new FileStream(writeFileInfo.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            string safePath = experimentGroup + "/" + writeFileInfo.RelativePath.Replace("\\", "/");
            var request = new PutObjectRequest()
            {
                BucketName = s3Bucket.bucketName,
                Key = safePath,
                InputStream = stream,
                TagSet = tags,
                CannedACL = S3CannedACL.Private
            };

            if (logLevel >= LogLevel.Normal)
            {
                Debug.LogFormat("Beginning upload {0}", writeFileInfo.RelativePath);
            }

            uploadingSet.Add(safePath);
            s3Client.PutObjectAsync(request, (responseObj) =>
            {
                uploadingSet.Remove(safePath);
                if (logLevel >= LogLevel.Normal)
                {
                    if (responseObj.Exception == null)
                    {
                        Debug.LogFormat(
                            "Uploaded {0}",
                            writeFileInfo.RelativePath);
                    }
                    else
                    {
                        if (responseObj.Response == null)
                        {
                            Debug.LogErrorFormat(
                                "Exception uploading {0}: (No response)",
                                writeFileInfo.RelativePath);
                        }
                        else
                        {
                            Debug.LogErrorFormat(
                                "Exception uploading {0}: {1}",
                                writeFileInfo.RelativePath,
                                responseObj.Response.HttpStatusCode);
                        }
                    }
                }
            }, options: new AsyncOptions(){ ExecuteCallbackOnMainThread = false });

        }


        void HandleException(object sender, ExceptionEventArgs e)
        {
            if (logLevel >= LogLevel.Normal)
            {
                Debug.LogError("Exception raised on uploading file");
            }
        }

        void OnDestroy()
        {
            if (s3Client != null)
                s3Client.Dispose();
        }

    }


    public enum LogLevel
    {
        None, Normal, Debugging
    }

}