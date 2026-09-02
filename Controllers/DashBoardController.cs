using Capsitech;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Projects.Dtos.Common;
using Projects.Dtos.Project;
using Projects.Dtos.Task;
using Projects.Dtos.Todo;
using Projects.Models;
using Projects.Services;

namespace Projects.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]

    [Route("api/[controller]")]
    public class DashBoardController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly ProjectService _projectService;
        public DashBoardController(TaskService todoService, ProjectService projectService)
        {
            _taskService = todoService;
            _projectService = projectService;
        }

        [HttpGet("CountTask")]
        public async Task<ApiResponse<CountTaskDto>> CountTask()
        {
            string userId = User.GetUserId();
            CountTaskDto data = await _taskService.CountTask(userId);
            var response = new ApiResponse<CountTaskDto>()
            {
                Result = data
            };
            response.Message = "Get count from data";
            return response;
        }

        [HttpGet("GetCountStatus")]
        public async Task<ApiResponse<List<ResponseStatusCount>>> GetCountStatus()
        {
            string userId = User.GetUserId();
            List<ResponseStatusCount> data = await _taskService.GetCountStatusAsync(userId);
            var response = new ApiResponse<List<ResponseStatusCount>>()
            {
                Result = data
            };
            response.Message = "Get count from data";
            return response;
        }

        [HttpGet("GetCountPriority")]
        public async Task<ApiResponse<List<ResponsePriorityCount>>> GetCountPriority()
        {
            string userId = User.GetUserId();
            List<ResponsePriorityCount> data = await _taskService.GetCountPriorityAsync(userId);
            var response = new ApiResponse<List<ResponsePriorityCount>>()
            {
                Result = data
            };
            response.Message = "Get count from data";
            return response;
        }

        [HttpGet("GetProjectTaskCount")]
        public async Task<ApiResponse<List<ProjectTaskStatusCount>>> GetProjectTaskCount()
        {
            string userId = User.GetUserId();
            List<ProjectTaskStatusCount> data = await _projectService.GetProjectTaskCount(userId);
            var response = new ApiResponse<List<ProjectTaskStatusCount>>()
            {
                Result = data
            };
            response.Message = "Get count from data";
            return response;
        }

        [HttpGet("GetRecentTask")]
        public async Task<ApiResponse<List<TaskwithProject>>> GetRecentTask()
        {
            string userId = User.GetUserId();
            List<TaskwithProject> data = await _taskService.GetRecentTask(userId);
            var response = new ApiResponse<List<TaskwithProject>>()
            {
                Result = data
            };
            response.Message = "Get count from data";
            return response;
        }
    }
}
