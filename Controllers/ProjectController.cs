using Capsitech;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Dtos.Project;
using Projects.Services;

namespace Projects.Controller
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]

    [Route("api/[controller]")]
    public class ProjectController:ControllerBase
    {
        private readonly ProjectService _projectService;
        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost("postProject")]
        public async Task<ApiResponse<string>> CrateProject(ProjectDto dto)
        {
            var response = new ApiResponse<string>();
            try
            {
                string userId = User.GetUserId();
                string projectId = await _projectService.CreateProject(dto, userId);
                response.Result = projectId;
                response.Message = "Project created successfully";
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpGet("GetAllProject")]
        public async Task<ApiResponse<List<ResponseProjectData>>> GetallProject()
        {
            var response = new ApiResponse<List<ResponseProjectData>>();
            try
            {
                string userId = User.GetUserId();
                List<ResponseProjectData> projects = await _projectService.GetAllProject(userId);
                response.Result = projects;
                response.Message = "Get All Projects";
            }
            catch(Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpGet("GetProjectById")]
        public async Task<ApiResponse<ResponseProjectTaskDto>> GetProjectById(string projectId)
        {
            var response = new ApiResponse<ResponseProjectTaskDto>();
            try
            {
                string userid = User.GetUserId();
                var project =await _projectService.GetProject(userid, projectId);
                response.Result = project;
                response.Message = "Get the project by id Successfully";
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpPatch("UpdateProject")]
        public async Task<ApiResponse<string>> UpdateProject(ProjectDto dto ,string projectId)
        {
            var response = new ApiResponse<string>();
            try
            {
            string userid = User.GetUserId();
                if (dto.Desc == "" && dto.Name == "") throw new Exception("Fill min one data to update");
                await _projectService.UpdateProject(userid, projectId, dto);
                response.Message = "Project Updated Successfully";
            }
            catch(Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpPatch("UpdateProjectStatus")]
        public async Task<ApiResponse<string>> UpdateProjectStatus( string projectId)
        {
            var response = new ApiResponse<string>();
            try
            {
                string userid = User.GetUserId();
                await _projectService.UpdateStatus(userid, projectId);
                response.Message = "Project Updated Successfully";
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpDelete("DeleteTAsk")]
        public async Task<ApiResponse<string>> DeleteTAsk(string projectId)
        {
            var response = new ApiResponse<string>();
            try
            {
                string userid = User.GetUserId();
                await _projectService.DeleteProject(userid, projectId);
                response.Message = "Project Deleted Successfully";
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }
    }
}