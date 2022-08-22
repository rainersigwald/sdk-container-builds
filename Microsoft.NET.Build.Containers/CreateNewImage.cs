﻿using Microsoft.Build.Framework;

namespace Microsoft.NET.Build.Containers.Tasks;

public class CreateNewImage : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// The base registry to pull from.
    /// Ex: https://mcr.microsoft.com
    /// </summary>
    [Required]
    public string BaseRegistry { get; set; }

    /// <summary>
    /// The base image to pull.
    /// Ex: dotnet/runtime
    /// </summary>
    [Required]
    public string BaseImageName { get; set; }

    /// <summary>
    /// The base image tag.
    /// Ex: 6.0
    /// </summary>
    [Required]
    public string BaseImageTag { get; set; }

    /// <summary>
    /// The registry to push to.
    /// </summary>
    [Required]
    public string OutputRegistry { get; set; }

    /// <summary>
    /// The name of the output image that will be pushed to the registry.
    /// </summary>
    [Required]
    public string ImageName { get; set; }

    /// <summary>
    /// The tag to associate with the new image.
    /// </summary>
    public string ImageTag { get; set; }

    /// <summary>
    /// The directory for the build outputs to be published.
    /// Constructed from "$(MSBuildProjectDirectory)\$(PublishDir)"
    /// </summary>
    [Required]
    public string PublishDirectory { get; set; }

    /// <summary>
    /// The working directory of the container.
    /// </summary>
    [Required]
    public string WorkingDirectory { get; set; }

    /// <summary>
    /// The entrypoint application of the container.
    /// </summary>
    [Required]
    public ITaskItem[] Entrypoint { get; set; }

    /// <summary>
    /// Arguments to pass alongside Entrypoint.
    /// </summary>
    public ITaskItem[] EntrypointArgs { get; set; }

    /// <summary>
    /// Ports that the application declares that it will use.
    /// Note that this means nothing to container hosts, by default -
    /// it's mostly documentation.
    /// </summary>
    public ITaskItem[] ExposedPorts { get; set; }

    /// <summary>
    /// Labels that the image configuration will include in metadata
    /// </summary>
    public ITaskItem[] Labels { get; set; }

    private bool IsDockerPush { get => OutputRegistry == "docker://"; }

    public CreateNewImage()
    {
        BaseRegistry = "";
        BaseImageName = "";
        BaseImageTag = "";
        OutputRegistry = "";
        ImageName = "";
        ImageTag = "";
        PublishDirectory = "";
        WorkingDirectory = "";
        Entrypoint = Array.Empty<ITaskItem>();
        EntrypointArgs = Array.Empty<ITaskItem>();
        Labels = Array.Empty<ITaskItem>();
        ExposedPorts = Array.Empty<ITaskItem>();
    }

    private void SetPorts(Image image, ITaskItem[] exposedPorts)
    {
        foreach (var port in exposedPorts)
        {
            var portNo = port.ItemSpec;
            var portTy = port.GetMetadata("Type");
            var parsePortResult = ContainerHelpers.ParsePort(portNo, portTy);
            if (!parsePortResult.success)
            {
                ContainerHelpers.ParsePortError errors = (ContainerHelpers.ParsePortError)parsePortResult.parseErrors!;
                var portString = portTy == null ? portNo : $"{portNo}/{portTy}";
                if (errors.HasFlag(ContainerHelpers.ParsePortError.MissingPortNumber))
                {
                    Log.LogError("A ContainerPort item was provided without an Include metadata specifying the port number. Please provide a ContainerPort item with an ItemSpec: <ContainerPort Include=\"80\" />");
                }
                else
                {
                    var message = "A ContainerPort item was provided with ";
                    var arguments = new List<string>(2);
                    if (errors.HasFlag(ContainerHelpers.ParsePortError.InvalidPortNumber) && errors.HasFlag(ContainerHelpers.ParsePortError.InvalidPortNumber))
                    {
                        message += "an invalid port number '{0}' and an invalid port type '{1}'";
                        arguments.Add(portNo);
                        arguments.Add(portTy!);
                    }
                    else if (errors.HasFlag(ContainerHelpers.ParsePortError.InvalidPortNumber))
                    {
                        message += "an invalid port number '{0}'";
                        arguments.Add(portNo);
                    }
                    else if (errors.HasFlag(ContainerHelpers.ParsePortError.InvalidPortNumber))
                    {
                        message += "an invalid port type '{0}'";
                        arguments.Add(portTy!);
                    }
                    message += ". ContainerPort items must have an Include value that is an integer, and a Type value that is either 'tcp' or 'udp'";

                    Log.LogError(message, arguments);
                }
            }
            else
            {
                image.ExposePort(parsePortResult.port!.number, parsePortResult.port.type);
            }
        }

    }

    public override bool Execute()
    {
        if (!Directory.Exists(PublishDirectory))
        {
            Log.LogError("{0} '{1}' does not exist", nameof(PublishDirectory), PublishDirectory);
            return !Log.HasLoggedErrors;
        }

        Registry reg;
        Image image;

        try
        {
            reg = new Registry(new Uri(BaseRegistry, UriKind.RelativeOrAbsolute));
            image = reg.GetImageManifest(BaseImageName, BaseImageTag).Result;
        }
        catch
        {
            throw;
        }

        if (BuildEngine != null)
        {
            Log.LogMessage($"Loading from directory: {PublishDirectory}");
        }

        Layer newLayer = Layer.FromDirectory(PublishDirectory, WorkingDirectory);
        image.AddLayer(newLayer);
        image.WorkingDirectory = WorkingDirectory;
        image.SetEntrypoint(Entrypoint.Select(i => i.ItemSpec).ToArray(), EntrypointArgs.Select(i => i.ItemSpec).ToArray());

        foreach (var label in Labels)
        {
            image.Label(label.ItemSpec, label.GetMetadata("Value"));
        }

        SetPorts(image, ExposedPorts);

        // at the end of this step, if any failed then bail out.
        if (Log.HasLoggedErrors)
        {
            return false;
        }

        if (IsDockerPush)
        {
            try
            {
                LocalDocker.Load(image, ImageName, ImageTag, BaseImageName).Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is DockerLoadException dle)
            {
                Log.LogErrorFromException(dle, showStackTrace: false);
                return !Log.HasLoggedErrors;
            }
        }
        else
        {
            Registry outputReg = new Registry(new Uri(OutputRegistry));
            try
            {
                outputReg.Push(image, ImageName, ImageTag, BaseImageName).Wait();
            }
            catch (Exception e)
            {
                if (BuildEngine != null)
                {
                    Log.LogError("Failed to push to the output registry: {0}", e);
                }
                return !Log.HasLoggedErrors;
            }
        }

        if (BuildEngine != null)
        {
            if (IsDockerPush)
            {
                Log.LogMessage(MessageImportance.High, "Pushed container '{0}:{1}' to local Docker daemon", ImageName, ImageTag);
            }
            else
            {
                Log.LogMessage(MessageImportance.High, "Pushed container '{0}:{1}' to registry '{2}'", ImageName, ImageTag, OutputRegistry);
            }
        }

        return !Log.HasLoggedErrors;
    }
}