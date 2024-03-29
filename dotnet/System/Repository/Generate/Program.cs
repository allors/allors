// <copyright file="Program.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Generate;

using System;
using System.IO;
using System.Threading.Tasks;
using Allors.Repository;
using Allors.Repository.Code;
using Allors.Repository.Generation;
using Allors.Repository.Model;
using NLog;

public class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {
                Logger.Error("missing required arguments");
            }

            await RepositoryGenerate(args);
        }
        catch (RepositoryException e)
        {
            Logger.Error(e.Message);
            return 1;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Info("Finished with errors");
            return 1;
        }

        Logger.Info("Finished");
        return 0;
    }

    private static async Task RepositoryGenerate(string[] args)
    {
        var projectPath = args[0];
        var template = args[1];
        var output = args[2];

        var fileInfo = new FileInfo(projectPath);

        Logger.Info("Generate " + fileInfo.FullName);
        var project = new Project(fileInfo.FullName);
        await project.InitializeAsync();

        if (project.HasErrors)
        {
            throw new RepositoryException("Repository project has errors.");
        }

        var templateFileInfo = new FileInfo(template);
        var stringTemplate = new StringTemplate(templateFileInfo);
        var outputDirectoryInfo = new DirectoryInfo(output);

        var repository = project.Repository;
        var repositoryModel = new RepositoryModel(repository);

        if (repositoryModel.HasErrors)
        {
            throw new RepositoryException("Repository model has errors.");
        }

        stringTemplate.Generate(repositoryModel, outputDirectoryInfo);
    }
}
