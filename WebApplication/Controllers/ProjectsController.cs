﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Forge.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using WebApplication.Definitions;
using WebApplication.Utilities;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IForgeOSS _forge;
        private readonly ResourceProvider _resourceProvider;
        private readonly LinkGenerator _linkGenerator;

        public ProjectsController(ILogger<ProjectsController> logger, IForgeOSS forge, ResourceProvider resourceProvider, LinkGenerator linkGenerator)
        {
            _logger = logger;
            _forge = forge;
            _resourceProvider = resourceProvider;
            _linkGenerator = linkGenerator;
        }

        [HttpGet("")]
        public async Task<IEnumerable<ProjectDTO>> ListAsync()
        {
            // TODO move to projects repository?
            List<ObjectDetails> objects = await _forge.GetBucketObjectsAsync(_resourceProvider.BucketKey, $"{ONC.ProjectsFolder}-");
            var projectDTOs = new List<ProjectDTO>();
            foreach(ObjectDetails objDetails in objects)
            {
                var projectName = ONC.ToProjectName(objDetails.ObjectKey);

                ProjectStorage projectStorage = _resourceProvider.GetProjectStorage(projectName);
                Project project = projectStorage.Project;

                var modelDownloadUrl = _linkGenerator.GetPathByAction(HttpContext, controller: "Download", action: "Model", values: new { projectName = projectName, hash = projectStorage.Metadata.Hash });

                var dto = new ProjectDTO
                {
                    Id = project.Name,
                    Label = project.Name,
                    Image = _resourceProvider.ToDataUrl(project.LocalAttributes.Thumbnail),
                    Svf = _resourceProvider.ToDataUrl(projectStorage.GetLocalNames().SvfDir),
                    ModelDownloadUrl = modelDownloadUrl
                };
                projectDTOs.Add(dto);
            }

            return projectDTOs.OrderBy(project => project.Label);
        }
    }
}
