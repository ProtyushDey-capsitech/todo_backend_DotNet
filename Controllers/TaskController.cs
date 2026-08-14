using Capsitech;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Projects.Dtos.Common;
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

        //[HttpGet("getList")]
        //public async Task<ApiResponse<List<ResponseData>>> get([FromQuery] PaginatedQueryDto dto)
        //{
        //    string userId = User.GetUserId();
        //    List<ResponseData> data = await _todoService.getAsync(userId , dto);
        //    var response = new ApiResponse<List<ResponseData>>()    
        //    {
        //        Result = data
        //    };
        //    response.Message = "gat datas";
        //    return response;
        //}

        [HttpPost("postData")]
        public async Task<ApiResponse<string>> Create(UpdateTask dto, string projectId)
        {
            string userId = User.GetUserId();
            string CreatedId = await _taskService.CreateAsync(dto, userId , projectId);
            var response = new ApiResponse<string>()
            {
                Result = CreatedId
            };
            response.Message = "Task created Completly";
            return (response);
        }

        [HttpPatch("UpdateTask")]
        public async Task<ApiResponse<string>> UpdateData(string id, string projectId,  UpdateTask dto)
        {
            string userId = User.GetUserId();
            await _taskService.UpdateTaskAsync(id, dto, projectId, userId);
            var response = new ApiResponse<string>();
            response.Message = "Updated completely";
            return (response);
        }
        //[HttpPut("UpdateStatus")]
        //public async Task<ApiResponse<string>> UpdateStatus(string id)
        //{
        //    string userId = User.GetUserId();
        //    var response = new ApiResponse<string>();
        //    try
        //    {
        //        await _todoService.UpdateAsync(id, userId);
        //        response.Message = "Updated the task status ";

        //    }
        //    catch (Exception e)
        //    {
        //        response.AddError(e);
        //    }
        //    return (response);
        //}
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
