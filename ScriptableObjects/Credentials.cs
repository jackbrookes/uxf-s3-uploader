using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF_S3_Uploader
{

    [CreateAssetMenu(fileName = "private_credentials", menuName = "UXF_S3/Credentials", order = 1)]
    public class Credentials : ScriptableObject
    {	
        [Header("Enter your credentials here, but keep them private")]
        [TextArea]
        public string identityPoolID = "eu-west-2:0000-0000-0000";
        public string cognitoIdentityRegion = "eu-west-2";
        public string s3Region = "eu-west-2";
    }

}