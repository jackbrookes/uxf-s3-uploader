# UXF AWS S3 Uploader

This is a Unity package that allows experiments built in UXF to upload their files to an Amazon Web Services S3 bucket.

Learn more about UXF [here](https://github.com/immersivecognition/unity-experiment-framework).

To use this in your project download the latest [Release](https://github.com/jackbrookes/uxf-s3-uploader/releases/latest)

## Setup

Note: Requires Unity 2018 or newer.

1. Add the latest [UXF package](https://github.com/jackbrookes/unity-experiment-framework/releases/latest) to your project
2. Add the latest [AWS Mobile SDK for Unity](https://docs.aws.amazon.com/mobile/sdkforunity/developerguide/what-is-unity-plugin.html) to your project (S3 package is required, others are optional)
3. Edit `s3_credentials.asset` with your credentials for AWS (or create new credentials: right click in the project panel -> create -> UXF_S3 -> Credentials). You can create/find your credentials in AWS Cognito. Find instructions in the AWS Mobile SDK for Unity documentation.
4. Add the `[UXF_S3_Uploader]` prefab to your scene.
5. Write your bucket name in the `S3Uploader` component.
6. In your `UXF.FileIOManager` component, add a new event to `OnWriteFile`, reference your `S3Uploader` component and select the `Upload` method. This means that files will be uploaded as soon a they are created.
7. If you have many files, your application could be quitting before they finish uploading. So the `S3UploadEnforcer` can be used to ensure files are finished before quitting. To do this make sure the `UXF.Session.onSessionEnd` does not quit the application, and add the `S3UploadEnforcer.EnforceThenInvokeEvent` method to the event. Now you can go to the `S3UploadEnforcer` component of the prefab instance in your scene and set it to do whatever you want when all files have been safely uploaded (like quit the application).



## FAQ

*Why does my editor keep crashing?*

There seems to be a bug with the AWS SDK that means it can crash Unity if there are any errors in your credentials. Make sure your credentials are correct and try to safely exit play mode using the button in the `S3UploadEnforcer` inspector.

*How do I set up AWS permissions correctly?*

* Make sure you have a Cognito identity pool set up with unauthorised access enabled (i.e. non logged in users)
* in IAM create a new policy on `Cognito_*UnauthRole` that has permissions to perform actions `s3:PutObject` and `s3:PutObjectTagging` on your bucket.
* The resource for this policy should be `"Resource": "arn:aws:s3:::your-bucket-name/*"`
