using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Acm;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.CloudFront.Inputs;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.Route53;
using Pulumi.Aws.Route53.Inputs;
using Pulumi.Aws.S3;
using Pulumi.Aws.S3.Inputs;

namespace PulumiProvisioner.Aws.Templates
{
    public record DomainArgs(string DomainName, string SubDomain = "www", string? SslDomainName = null);

    public class StaticWebsite
    {
        private readonly Bucket _bucket = null!;
        private readonly Distribution _cloudfront = null!;

        internal StaticWebsite(
            string applicationName,
            string indexDocument,
            string errorDocument,
            string fileCopyDirectory,
            DomainArgs? domainArgs)
        {
            var bucket = CreateBucket(applicationName, indexDocument, errorDocument);
            _bucket = bucket;

            CopyWebsiteFiles(applicationName, bucket, fileCopyDirectory);

            Output<GetCertificateResult>? certificate = null;
            if (domainArgs != null)
            {
                certificate = GetSslCertificate(applicationName, domainArgs.SslDomainName ?? domainArgs.DomainName);
            }

            var fullyQualifiedDomainName = domainArgs != null ? $"{domainArgs.SubDomain}.{domainArgs.DomainName}" : null;
            var cdn = CreateCdn(applicationName, bucket, indexDocument, fullyQualifiedDomainName, certificate);
            _cloudfront = cdn;

            if (domainArgs != null && certificate != null)
            {
                CreateDomainRecord(applicationName, cdn, domainArgs.DomainName, domainArgs.SubDomain);
            }
        }

        public Output<string> BucketUrl => Output.Format($"http://{_bucket.WebsiteEndpoint}");
        public Output<string> Url => _cloudfront.DomainName;

        private static Bucket CreateBucket(string applicationName, string indexDocument, string errorDocument)
        {
            var bucket = new Bucket(applicationName, new BucketArgs
            {
                BucketName = applicationName.ToLower(),
                Website = new BucketWebsiteArgs
                {
                    IndexDocument = indexDocument,
                    ErrorDocument = errorDocument
                }
            });

            var bucketPolicy = new BucketPolicy($"{applicationName}-policy", new BucketPolicyArgs
            {
                Bucket = bucket.BucketName,
                Policy = bucket.Id.Apply(x =>
                {
                    return GetPolicyDocument.Invoke(new GetPolicyDocumentInvokeArgs
                    {
                        Statements = new[]
                        {
                            new GetPolicyDocumentStatementInputArgs
                            {
                                Principals = new[]
                                {
                                    new GetPolicyDocumentStatementPrincipalInputArgs
                                    {
                                        Type = "AWS",
                                        Identifiers = { "*" },
                                    },
                                },
                                Actions = { "s3:GetObject" },
                                Resources = { $"arn:aws:s3:::{x}/*" }
                            }
                        }
                    }).Apply(x => x.Json);
                })
            });

            return bucket;
        }
        private static void CopyWebsiteFiles(string applicationName, Bucket bucket, string fileDirectory)
        {
            var ownershipControls = new BucketOwnershipControls($"{applicationName}-ownership-controls", new()
            {
                Bucket = bucket.Id,
                Rule = new BucketOwnershipControlsRuleArgs
                {
                    ObjectOwnership = "ObjectWriter"
                }
            });

            var publicAccessBlock = new BucketPublicAccessBlock($"{applicationName}-public-access-block", new()
            {
                Bucket = bucket.Id,
                BlockPublicAcls = false
            }, new CustomResourceOptions { Parent = bucket });

            // Grab all you website files to upload
            UploadAllFilesInDirectory(fileDirectory, file =>
            {
                // Dont need directory prefix, plus it seems to not play nice with bucket upload api
                var name = file
                    .Replace($"{fileDirectory}", string.Empty)
                    .Replace($"\\", string.Empty);
                var contentType = MimeTypes.GetMimeType(file);

                var _ = new BucketObject(name, new BucketObjectArgs
                {
                    Acl = "public-read",
                    Bucket = bucket.BucketName,
                    ContentType = contentType,
                    Source = new FileAsset(file)
                }, new CustomResourceOptions
                {
                    Parent = bucket,
                    DependsOn = new Resource[] { publicAccessBlock, ownershipControls }
                });
            });
        }

        private static void UploadAllFilesInDirectory(string filePath, Action<string> fileAction)
        {
            foreach (string d in Directory.GetDirectories(filePath))
            {
                UploadAllFilesInDirectory(d, fileAction);
            }

            // Grab all you website files to upload
            var files = Directory.GetFiles(filePath);
            foreach (var file in files)
            {
                fileAction(file);
            }
        }

        private static Distribution CreateCdn(
            string applicationName,
            Bucket bucket,
            string defaultRootObject,
            string? domainName,
            Output<GetCertificateResult>? certificateDetails)
        {
            var originAccessIdentity = new OriginAccessIdentity($"{applicationName}-origin-access-identity");

            var cdn = new Distribution(applicationName, new DistributionArgs
            {
                DefaultRootObject = defaultRootObject,
                Enabled = true,
                IsIpv6Enabled = true,
                Aliases = domainName != null
                    ? [ domainName ]
                    : new(),
                DefaultCacheBehavior = new DistributionDefaultCacheBehaviorArgs
                {
                    AllowedMethods = { "GET", "HEAD", "OPTIONS" },
                    CachedMethods = { "GET", "HEAD", "OPTIONS" },
                    TargetOriginId = bucket.BucketName,
                    ForwardedValues = new DistributionDefaultCacheBehaviorForwardedValuesArgs
                    {
                        QueryString = false, 
                        Cookies = new DistributionDefaultCacheBehaviorForwardedValuesCookiesArgs
                        {
                            Forward = "none",
                        }
                    },
                    ViewerProtocolPolicy = "redirect-to-https"
                },
                Restrictions = new DistributionRestrictionsArgs
                {
                    GeoRestriction = new DistributionRestrictionsGeoRestrictionArgs
                    {
                        RestrictionType = "none"
                    }
                },
                ViewerCertificate = new DistributionViewerCertificateArgs
                {
                    CloudfrontDefaultCertificate = true,
                    SslSupportMethod = "sni-only",
                    AcmCertificateArn = certificateDetails != null ? certificateDetails.Apply(x => x.Arn) : (Input<string>?)null
                },
                Origins = new[]
                {
                    new DistributionOriginArgs
                    {
                        DomainName = bucket.BucketDomainName,
                        OriginId = bucket.Id,
                        S3OriginConfig = new DistributionOriginS3OriginConfigArgs
                        {
                            OriginAccessIdentity = originAccessIdentity.Id.Apply(x => $"origin-access-identity/cloudfront/{x}")
                        }
                    }
                }
            });

            return cdn;
        }

        private static Output<GetCertificateResult>? GetSslCertificate(string applicationName, string domainName)
        {
            var region = new Provider($"{applicationName}-get-certificate", new ProviderArgs
            {
                Region = "us-east-1"
            });

            var certificate = GetCertificate.Invoke(new GetCertificateInvokeArgs
            {
                Domain = domainName,
            }, 
            new InvokeOptions
            {
                Provider = region
            });
            return certificate;
        }

        private static void CreateDomainRecord(
            string applicationName,
            Distribution distribution,
            string domainName,
            string subDomainName)
        {
            var hostedZone = GetZone.Invoke(new GetZoneInvokeArgs
            {
                Name = domainName
            });

            _ = new Record($"{applicationName}-record", new RecordArgs
            {
                Name = $"{subDomainName}.{domainName}",
                Type = "A",
                ZoneId = hostedZone.Apply(x => x.Id),
                Aliases =
                [
                    new RecordAliasArgs
                    {
                        Name = distribution.DomainName,
                        ZoneId = distribution.HostedZoneId,
                        EvaluateTargetHealth = false
                    }
                ]
            });
        }
    }
}
