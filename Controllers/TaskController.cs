using Capsitech;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Projects.Dtos.Common;
using Projects.Dtos.Task;
using Projects.Dtos.Todo;
using Projects.Models;
using Projects.Services;

namespace Projects.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]

    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;
        public TaskController(TaskService todoService)
        {
            _taskService = todoService;
        }

        [HttpPost("getList")]
        public async Task<ApiResponse<List<ResponseAllTask>>> getList(projectListReq projectIds, [FromQuery] string? name)
        {
            string userId = User.GetUserId();
            List<ResponseAllTask> data = await _taskService.getAsync(userId, projectIds, name);
            var response = new ApiResponse<List<ResponseAllTask>>()
            {
                Result = data
            };
            response.Message = "get datas";
            return response;
        }

        [HttpPost("postData")]
        public async Task<ApiResponse<string>> Create(UpdateTask dto, string projectId)
        {
            string userId = User.GetUserId();
            string CreatedId = await _taskService.CreateAsync(dto, userId, projectId);
            var response = new ApiResponse<string>()
            {
                Result = CreatedId
            };
            response.Message = "Task created Completly";
            return (response);
        }

        [HttpPatch("UpdateTask")]
        public async Task<ApiResponse<string>> UpdateData(string id, string projectId, UpdateTask dto)
        {
            string userId = User.GetUserId();
            await _taskService.UpdateTaskAsync(id, dto, projectId, userId);
            var response = new ApiResponse<string>();
            response.Message = "Updated completely";
            return (response);
        }

        [HttpPatch("UpdateStatus")]
        public async Task<ApiResponse<string>> UpdateStatus(string id, string status)
        {
            string userId = User.GetUserId();
            var response = new ApiResponse<string>();
            try
            {
                await _taskService.UpdateAsync(id, userId, status);
                response.Message = "Updated the task status ";

            }
            catch (Exception e)
            {
                response.AddError(e);
            }
            return (response);
        }

        [HttpDelete("DeleteTask")]
        public async Task<ApiResponse<string>> DeleteTodo(string id)
        {
            string userId = User.GetUserId();
            var response = new ApiResponse<string>();
            try
            {
                await _taskService.DeleteAsync(id);
                response.Message = "Deleted the task";

            }
            catch (Exception e)
            {
                response.AddError(e);
            }
            return (response);
        }
    }
}
