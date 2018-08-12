using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF_S3_Uploader
{
	[CreateAssetMenu(fileName = "s3_bucket", menuName = "UXF_S3/S3Bucket", order = 2)]

	public class S3Bucket : ScriptableObject
	{
		public string bucketName = "my-bucket-name";

	}
}
