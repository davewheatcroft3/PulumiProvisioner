using Pulumi;
using Pulumi.Aws.Alb;
using Pulumi.Aws.Alb.Inputs;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;
using Pulumi.Aws.Ecs;
using Pulumi.Aws.Iam;
using System.Text.Json;

namespace PulumiProvisioner.Aws.Templates
{
    public class FargateEcsWebsite
    {
        private readonly Vpc _vpc = null!;
        private readonly LoadBalancer _loadBalancer = null!;
        private readonly Pulumi.Aws.ElastiCache.Cluster _redisCluster = null!;
        private readonly Service _service = null!;

        public FargateEcsWebsite(string name, string buildDirectory)
        {
            var (vpc, publicSubnet, privateSubet) = ConstructVpcAndSubnets(name);
            _vpc = vpc;

            var fargateCluster = ConstructFargeteCluster(name);

            var loadBalancer = ConstructPublicLoadBalancer(name, vpc, publicSubnet);
            _loadBalancer = loadBalancer;

            var (redis, _) = ConstructElasticCache(name, vpc, privateSubet);
            _redisCluster = redis;

            var rolePolicy = CreateEcsRolePolicy(name);

            var targetGroup = CreateTargetGroup(name, vpc, loadBalancer);

            _service = ConstructEcsService(
                name,
                buildDirectory,
                vpc,
                privateSubet,
                fargateCluster,
                targetGroup,
                rolePolicy,
                redis);
        }

        public Output<string> LoadBalancerId => _loadBalancer.Id;

        public Output<string> VpcId => _vpc.Id;

        public Output<string> RedisClusterId => _redisCluster.Id;

        public Output<string> FargateId => _service.Id;

        private static (Vpc Vpc, Subnet Public, Subnet Private) ConstructVpcAndSubnets(string name)
        {
            var vpc = new Vpc($"{name}-Vpc",
                new VpcArgs
                {
                    CidrBlock = "10.55.0.0/16",
                    //MaxAzs = 2,
                    //NatGateways = 1
                });

            var publicSubnet = new Subnet($"{name}-SubNet-Public", new SubnetArgs
            {
                VpcId = vpc.Id,
                MapPublicIpOnLaunch = true,
                CidrBlock = "0.0.0.0/24",
                //CidrMask = 24,
                //SubnetType = SubnetType.PUBLIC,
            });

            var privateSubnet = new Subnet($"{name}-SubNet-Private", new SubnetArgs
            {
                VpcId = vpc.Id,
                CidrBlock = "0.0.0.0/24"
                //CidrMask = 24,
                //SubnetType = SubnetType.PRIVATE_WITH_NAT,
            });

            return (vpc, publicSubnet, privateSubnet);
        }

        private static Cluster ConstructFargeteCluster(string name)
        {
            var cluster = new Cluster($"{name}-Cluster", new ClusterArgs
            {
                Name = $"{name}-Cluster"
            });
            return cluster;
        }

        private static LoadBalancer ConstructPublicLoadBalancer(string name, Vpc vpc, Subnet publicSubnet)
        {
            var securityGroup = new SecurityGroup($"{name}-SecurityGroup", new SecurityGroupArgs()
            {
                Name = $"{name}-SecurityGroup-PublicAccessForAlb",
                VpcId = vpc.Id,
                Description = "Security group for the public ALB",
                Egress = { AllowAllOutboardForSubnet() },
                Ingress =
                {
                    new SecurityGroupIngressArgs
                    {
                        Protocol = "tcp",
                        FromPort = 80,
                        ToPort = 80,
                        CidrBlocks = {"0.0.0.0/0"}
                    }
                }
            });

            var alb = new LoadBalancer($"{name}-Alb", new LoadBalancerArgs
            {
                Name = $"{name}-Alb",
                SecurityGroups = [ securityGroup.Id ],
                Internal = false,
                EnableCrossZoneLoadBalancing = true,
                Subnets = [ publicSubnet.Id ]
            });

            _ = new Listener($"{name}-Alb-HttpListener", new ListenerArgs
            {
                Protocol = "HTTP",
                LoadBalancerArn = alb.Arn,
                DefaultActions =
                [
                    new ListenerDefaultActionArgs
                    {
                        Type = "fixed-response",
                        FixedResponse = new ListenerDefaultActionFixedResponseArgs { StatusCode = "5XX" }
                    }
                ]
            });

            return alb;
        }
                
        private static (Pulumi.Aws.ElastiCache.Cluster Redis, SecurityGroup RegidSg) ConstructElasticCache(string name, Vpc vpc, Subnet privateSubnet)
        {
            var subnetGroup = new Pulumi.Aws.ElastiCache.SubnetGroup($"{name}-SubNet-Redis",
                new Pulumi.Aws.ElastiCache.SubnetGroupArgs
                {
                    Description = $"Subnet group for the {name} redis cluster",
                    Name = $"{name}-SubNet-Redis",
                    SubnetIds = [ privateSubnet.Id ]
                });

            var redisSg = new SecurityGroup($"{name}SecurityGroup-Redis",
                new SecurityGroupArgs()
                {
                    VpcId = vpc.Id,
                    Name = $"{name}-SecurityGroup-Redis",
                    Description = $"Security group for the {name} redis cluster",
                    Egress = { AllowAllOutboardForSubnet() }
                });

            var redis = new Pulumi.Aws.ElastiCache.Cluster($"{name}-CacheCluster-Redis",
                new Pulumi.Aws.ElastiCache.ClusterArgs
                {
                    Engine = "redis",
                    NodeType = "cache.t3.micro",
                    NumCacheNodes = 1,
                    Port = 6379,
                    SubnetGroupName = subnetGroup.Name,
                    SecurityGroupIds = [ redisSg.Id ]
                });

            return (redis, redisSg);
        }

        private static Role CreateEcsRolePolicy(string name)
        {
            // Create an IAM role that Fargate tasks can assume
            var rolePolicyJson = JsonSerializer.Serialize(new
            {
                Version = "2008-10-17",
                Statement = new[]
            {
                new
                {
                    Sid = "",
                    Effect = "Allow",
                    Principal = new
                    {
                        Service = "ecs-tasks.amazonaws.com"
                    },
                    Action = "sts:AssumeRole"
                }
            }
            });

            // Create an IAM role that can be used by our service's task.
            var taskExecRole = new Role($"{name}-task-exec-role", new RoleArgs
            {
                AssumeRolePolicy = rolePolicyJson
            });

            _ = new RolePolicyAttachment($"{name}-task-exec-policy", new RolePolicyAttachmentArgs
            {
                Role = taskExecRole.Name,
                PolicyArn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
            });

            return taskExecRole;
        }

        private static Service ConstructEcsService(
            string name,
            string buildDirectory,
            Vpc vpc,
            Subnet privateSubnet,
            Cluster cluster,
            TargetGroup targetGroup,
            Role taskExecRole,
            Pulumi.Aws.ElastiCache.Cluster redis)
        {
            var dockerImageAsset = new Pulumi.Docker.Image($"{name}-DockerImage", new Pulumi.Docker.ImageArgs
            {
                Build = new Pulumi.Docker.Inputs.DockerBuildArgs
                {
                    Context = buildDirectory,
                    Dockerfile = $"{buildDirectory}/Dockerfile.txt"
                },
                SkipPush = true
            });

            var environmentDictionary = new Dictionary<string, string>();
            redis.ConfigurationEndpoint.Apply(x =>
            {
                environmentDictionary.Add("CACHE_URL", x);
                return Task.CompletedTask;
            });

            // Set up a Fargate Task Definition, with the app container specification
            var task = new TaskDefinition($"{name}-app-task", new TaskDefinitionArgs
            {
                Cpu = "512",
                Family = $"{name}-FargateTaskDefinition",
                Memory = "1024",
                NetworkMode = "awsvpc",
                RequiresCompatibilities = { "FARGATE" },
                ContainerDefinitions = dockerImageAsset.ImageName.Apply(imageName => JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        Name = $"{name}-TaskContainer",
                        Image = imageName,
                        PortMappings = new[]
                        {
                            new
                            {
                                ContainerPort = 80
                            }
                        },
                        Environment = environmentDictionary
                    }
                })),
                // Make use of the created IAM role
                ExecutionRoleArn = taskExecRole.Arn
            });

            var sg = new SecurityGroup($"{name}-SecurityGroup-Svc-Ecs-Id",
                new SecurityGroupArgs
                {
                    Name = $"{name}-SecurityGroup-Svc-Ecs",
                    Description = "Allow traffic from ALB to app",
                    VpcId = vpc.Id,
                    Ingress = new SecurityGroupIngressArgs
                    {
                        // from applicatonLoadBalancer
                        // TODO? Map alb security group?
                        FromPort = 80,
                        ToPort = 80,
                        Protocol = "TCP",
                        Description = "Allow connection from the ALB to the Fargate Service."
                    },
                    Egress = new SecurityGroupEgressArgs
                    {
                        // to redisSecurityGroup
                        // TODO? Map redis security group?
                        FromPort = 6379,
                        ToPort = 6379,
                        Protocol = "TCP",
                        Description = "Allow the Fargate service to connect to the Redis Cluster."
                    }
                });

            var service = new Service($"{name}-FargateService",
                new ServiceArgs
                {
                    Name = $"{name}-FargateService",
                    TaskDefinition = task.Arn,
                    Cluster = cluster.Arn,
                    DesiredCount = 3,
                    DeploymentMinimumHealthyPercent = 100,
                    DeploymentMaximumPercent = 200,
                    NetworkConfiguration= new Pulumi.Aws.Ecs.Inputs.ServiceNetworkConfigurationArgs
                    {
                        AssignPublicIp = true,
                        Subnets = [ privateSubnet.Id ],
                        SecurityGroups = [ sg.Id ]
                    },
                    LoadBalancers = new Pulumi.Aws.Ecs.Inputs.ServiceLoadBalancerArgs
                    {
                        ContainerName = $"{name}-TaskContainer",
                        ContainerPort = 80,
                        TargetGroupArn = targetGroup.Arn
                    }
                });

            return service;
        }

        private static TargetGroup CreateTargetGroup(string name, Vpc vpc, LoadBalancer loadBalancer)
        {
            var targetGroup = new TargetGroup($"{name}-TargetGroup-Ecs-Id",
                new TargetGroupArgs
                {
                    Name = $"{name}-TargetGroup-Ecs-Id",
                    Port = 80,
                    TargetType = "ip",
                    Protocol = "HTTP",
                    ProtocolVersion = "HTTP1",
                    VpcId = vpc.Id,
                    Stickiness = new TargetGroupStickinessArgs
                    {
                        CookieDuration = (int)TimeSpan.FromDays(1).TotalSeconds,
                        Enabled = true,
                        Type = "lb_cookie"
                    },
                    HealthCheck = new TargetGroupHealthCheckArgs
                    {
                        Protocol = "HTTP",
                        HealthyThreshold = 3,
                        Path = "/health",
                        Port = "80",
                        Interval = 1,
                        Timeout = 8,
                        UnhealthyThreshold = 10,
                        Matcher = "200"
                    }
                });

            _ = new Listener($"{name}-AlbListener", new ListenerArgs
            {
                LoadBalancerArn = loadBalancer.Arn,
                Port = 80,
                DefaultActions =
                {
                    new ListenerDefaultActionArgs
                    {
                        Type = "forward",
                        TargetGroupArn = targetGroup.Arn
                    }
                }
            });

            return targetGroup;
        }

        private static SecurityGroupEgressArgs AllowAllOutboardForSubnet()
        {
            return new SecurityGroupEgressArgs
            {
                Protocol = "-1",
                FromPort = 0,
                ToPort = 0,
                CidrBlocks = { "0.0.0.0/0" }
            };
        }
    }
}