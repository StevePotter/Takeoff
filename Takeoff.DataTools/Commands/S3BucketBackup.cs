using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using CommandLine;
using LitS3;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class S3BucketBackupOptions
    {
        [Option('s', "sourcebucket", Required = true, HelpText = "Bucket name of the source to backup")]
        public string SourceBucket { get; set; }
        [Option('k', "sourcekey", Required = true, HelpText = "The public key of the account to backup.")]
        public string SourceAwsKey { get; set; }
        [Option('p', "sourceprivatekey", Required = true, HelpText = "The private key of the account to backup.")]
        public string SourceAwsPrivateKey { get; set; }
        [Option('t', "targetbucket", Required = true)]
        public string TargetBucket { get; set; }
        [Option('u', "targetkey", Required = true)]
        public string TargetAwsKey { get; set; }
        [Option('v', "targetprivatekey", Required = true)]
        public string TargetAwsPrivateKey { get; set; }

        [Option('d', "synctargetdeletes", Required = false)]
        public bool DeleteTargetFilesNotInSource { get; set; }
    }

    //todo: propogate deletions.  also eventually you should add a "deep" comparison.  this will check for differences in files, in case of an overwrite or dare I say corruption

    /// <summary>
    /// Takes a source bucket and ensures its files are in the target.  Optionally deletes any files in the target but not in source.
    /// </summary>
    public class S3BucketBackup : BaseCommandWithOptions<S3BucketBackupOptions>
    {
        public S3BucketBackup()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
            SourceKeys = new List<string>();
            TargetKeys = new List<string>();
        }

        private List<string> SourceKeys;
        private List<string> TargetKeys;
        private string[] KeysToCopy;
        private string[] TargetKeysToDelete;
        
        private LitS3.S3Service LitS3Source;
        private AmazonS3Client AwsS3Source;
        private LitS3.S3Service LitS3Target;
        private AmazonS3Client AwsS3Target;

        protected override void Perform(S3BucketBackupOptions options)
        {
            AwsS3Source = new AmazonS3Client(options.SourceAwsKey, options.SourceAwsPrivateKey);
            LitS3Source = new LitS3.S3Service
            {
                AccessKeyID = options.SourceAwsKey,
                SecretAccessKey = options.SourceAwsPrivateKey,
            };
            AwsS3Target = new AmazonS3Client(options.TargetAwsKey, options.TargetAwsPrivateKey);
            LitS3Target = new LitS3.S3Service
            {
                AccessKeyID = options.TargetAwsKey,
                SecretAccessKey = options.TargetAwsPrivateKey,
            };

            Step("GetAllSourceKeys", false, 5, TimeSpan.FromSeconds(2), () =>
            {
                SourceKeys.Clear();
                GetAllKeys(null, SourceKeys, options.SourceBucket, LitS3Source);
            });
            Step("GetAllTargetKeys", false, 5, TimeSpan.FromSeconds(2), () =>
            {
                TargetKeys.Clear();
                GetAllKeys(null, TargetKeys, options.TargetBucket, LitS3Target);
            });
            AddReportAttribute("TargetKeyCount", TargetKeys.Count);
            AddReportAttribute("SourceKeysCount", SourceKeys.Count);
            if (options.DeleteTargetFilesNotInSource)
            {
                Step("GetKeysToDelete", () =>
                {
                    //any keys that were in source but not target should be copied
                    var source = new HashSet<string>(TargetKeys);
                    source.Remove(SourceKeys);
                    TargetKeysToDelete = source.ToArray();
                    AddReportAttribute("TargetKeysToDelete", TargetKeysToDelete.Length);
                });
                Step("DeleteTargetFilesNotInSource", DeleteTargetFilesNotInSource);
            }
            Step("GetKeysToCopy", () =>
            {
                //any keys that were in source but not target should be copied
                var source = new HashSet<string>(SourceKeys);
                source.Remove(TargetKeys);
                KeysToCopy = source.ToArray();
                AddReportAttribute("SourceKeys", SourceKeys.Count);
                AddReportAttribute("TargetKeys", TargetKeys.Count);
                AddReportAttribute("KeysToCopy", KeysToCopy.Length);
            });
            Step("CopyFiles", CopyFiles);

        }


        void GetAllKeys(ListEntry[] objs, List<string> keys, string bucket, LitS3.S3Service s3Service)
        {
            if ( objs == null )
            {                
                Step("ListMainObjects", false, 5, TimeSpan.FromSeconds(2), () =>
                                          {
                                            objs = s3Service.ListAllObjects(bucket).ToArray();                                
                                          });
            }
            foreach (var obj in objs)
            {
                var asPrefix = obj as CommonPrefix;
                if (asPrefix != null)
                {
                    ListEntry[] children = null;
                    Step(new StepParams
                    {
                        Name = "ListObjectsWithPrefix",
                        RunIfErrorOccured = false,
                        MaxTries = 5,
                        BetweenTries = TimeSpan.FromSeconds(2),
                        WriteStartAndEndToConsole = false,
                        Work = () =>
                        {
                            children = s3Service.ListAllObjects(bucket, asPrefix.Prefix).ToArray();
                        }
                    });
                    if (children.HasItems())
                        GetAllKeys(children, keys, bucket, s3Service);
                }
                else
                {
                    keys.Add(((ObjectEntry)obj).Key);
                }
            }
            
        }

        void DeleteTargetFilesNotInSource()
        {
            int i = 0;
            foreach (var key in TargetKeysToDelete)
            {
                i++;
                Console.WriteLine("{0} of {1} - {2}", i, TargetKeysToDelete.Length, key);
                Step("DeleteFile", true, 10, TimeSpan.FromSeconds(2), false, () =>
                                                                                                {
                                                                                                    AddReportAttribute("Key",key);
                                                                                                    AwsS3Target.DeleteObject(new Amazon.S3.Model.DeleteObjectRequest
                                                                                                                                 {
                                                                                                                                     BucketName = Options.TargetBucket,
                                                                                                                                     Key = key,
                                                                                                                                 });
                                                                                                });
            }
        }

        void CopyFiles()
        {
            int i = 0;
            foreach (var key in KeysToCopy)
            {
                i++;
                Console.WriteLine("{0} of {1} - {2}", i, KeysToCopy.Length, key);
                Step("CopyFile", true, 10, TimeSpan.FromSeconds(2), false, () => CopyFile(key));
            }
        }

        void CopyFile(string key)
        {
            try
            {
                var metaData = AwsS3Target.GetObjectMetadata(new GetObjectMetadataRequest
                                                                  {
                                                                      BucketName = Options.TargetBucket,
                                                                      Key = key
                                                                  });
                Console.WriteLine("Key existed.  Moving on.");
                return;
            }
            catch (Amazon.S3.AmazonS3Exception)
            {

            }

            var path = Path.GetTempFileName();
            if (File.Exists(path))
                File.Delete(path);

            if (!Step("DownloadFile", true, 10, TimeSpan.FromSeconds(2), false, () =>
                                                                                    {
                                                                                        var response = AwsS3Source.GetObject(new Amazon
                                                                                                                  .S3.
                                                                                                                  Model.
                                                                                                                  GetObjectRequest
                                                                                                                  {
                                                                                                                      Key = key,
                                                                                                                      BucketName = Options.SourceBucket
                                                                                                                  });
                                                                                        response.WriteResponseStreamToFile(path);
                                                                                    }
                                                                                    ))
            {                
                AddDynamicObjectToReport("FailedDownload", new
                                                               {
                                                                   Key = key,
                                                               });
                return;
            }

            if ( !Step("UploadFile", true, 10, TimeSpan.FromSeconds(2), false, () => AwsS3Target.PutObject(new PutObjectRequest
                                                                                                                              {
                                                                                                                                  Key = key,
                                                                                                                                  BucketName = Options.TargetBucket,
                                                                                                                                  FilePath = path,
                                                                                                                                  StorageClass = S3StorageClass.ReducedRedundancy,//saves costs on storage.  
                                                                                                                                  Timeout = TimeSpan.FromSeconds(3600000),
                                                                                                                              })) )
            {
                AddDynamicObjectToReport("FailedUpload", new
                {
                    Key = key,
                });
                return;

            }

            if (File.Exists(path))
                File.Delete(path);
        }        
    }
}
