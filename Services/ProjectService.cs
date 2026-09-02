using Azure.Storage.Blobs.Specialized;
using Capsitech.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Project;
using Projects.Dtos.Task;
using Projects.Dtos.Todo;
using Projects.Models;
using System.Runtime.CompilerServices;

namespace Projects.Services
{
    public class ProjectService
    {
        private readonly IMongoCollection<ProjectModel> _project;
        private readonly IMongoCollection<TaskModel> _task;

        public ProjectService(IOptions<DbSettings> dbsettings)
        {
            var mongoclient = new MongoClient(dbsettings.Value.ConnectionString);
            var dataBase = mongoclient.GetDatabase(dbsettings.Value.DatabaseName);
            _project = dataBase.GetCollection<ProjectModel>(DbCollections.Project);
            _task = dataBase.GetCollection<TaskModel>(DbCollections.Tasks);
        }

        public async Task<string> CreateProject(ProjectDto dto, string userId)
        {
            var existProject = _project.Find(x => x.Name == dto.Name && x.UserId == userId).FirstOrDefault();
            if (existProject != null) throw new InvalidCastException("Project already exist");
            var newProject = new ProjectModel()
            {
                Name = dto.Name,
                Desc = dto.Desc,
                UserId = userId,
                Status = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _project.InsertOneAsync(newProject);

            return newProject.Id;
        }

        public async Task<PaginatedResultDto<ResponseProjectData>> GetAllProject(string uerId, ProjectPaginatedQueryDto dto)
        {
            int skip = (dto.Page - 1) * dto.PageSize;
            var filter = Builders<ProjectModel>.Filter.Eq(x => x.UserId, uerId);

            if (!string.IsNullOrEmpty(dto.Search))
            {
                filter &= Builders<ProjectModel>.Filter.Regex(
                        x => x.Name,
                       new BsonRegularExpression(dto.Search)
                );
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                bool status = bool.Parse(dto.Status);

                filter &= Builders<ProjectModel>.Filter.Eq(x => x.Status, status);
            }
            long total = await _project.CountDocumentsAsync(filter);
            var projects = await _project.Aggregate()
                .Match(filter)
                .SortByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Limit(dto.PageSize)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                _task,
                x => x.Id,
                x => x.ProjectId,
                x => x.Tasks
                )
                .Project(x => new ResponseProjectData
                {
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Id = x.Id,
                    TaskCount = x.Tasks.Count()
                })
                .ToListAsync();
            var response = new PaginatedResultDto<ResponseProjectData>
            {
                Results = projects,
                Total = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
            return response;

        }

        public async Task<List<ProjectNameDto>> GetAllProjectName(string uerId)
        {
            var filter = Builders<ProjectModel>.Filter.Eq(x => x.UserId, uerId);

            var response = await _project.Find(filter)
                .Project(x => new ProjectNameDto
                {
                    name = x.Name,
                    Id = x.Id
                })
                .ToListAsync();
            return response;

        }

        public async Task<ResponseProjectTaskDto> GetProject(string uerId, string PId, TaskQueryDto dto)
        {
            var projectFilter = Builders<ProjectModel>.Filter.And(Builders<ProjectModel>.Filter.Eq(x => x.UserId, uerId),
                Builders<ProjectModel>.Filter.Eq(x => x.Id, PId));
            var existProject = await _project.Find(projectFilter).FirstOrDefaultAsync();
            if (existProject == null) throw new Exception("Project not found");
            var project = await _project.Aggregate()
                .Match(projectFilter)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                    _task,
                    x => x.Id,
                    x => x.ProjectId,
                    x => x.Tasks

                )
                .Project(x => new ResponseProjectTaskDto
                {
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Id = x.Id!.ToString(),
                    TaskCount = x.Tasks!.Count(),
                    Tasks = x.Tasks!.Select(t => new ResponseTaskData
                    {
                        Id = t.Id!.ToString(),
                        Name = t.Name,
                        Desc = t.Desc,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedAt = t.CreatedAt
                    }).ToList(),
                }).FirstOrDefaultAsync();
            project.Tasks = FilterAndPaginateTasks(project.Tasks!, dto);
            return project;

        }

        public async Task UpdateProject(string uId, string pid, ProjectDto dto)
        {
            var filter = Builders<ProjectModel>.Filter.Eq(x => x.UserId, uId);
            var allFilters = Builders<ProjectModel>.Filter.And(filter, Builders<ProjectModel>.Filter.Eq(x => x.Id, pid));
            var update = new List<UpdateDefinition<ProjectModel>>();
            if (dto.Desc != "") update.Add(Builders<ProjectModel>.Update.Set(x => x.Desc, dto.Desc));
            if (dto.Name != "")
            {
                var matchFilters = Builders<ProjectModel>.Filter.And(filter, Builders<ProjectModel>.Filter.Eq(x => x.Id, pid));
                var existProject = _project.Find(matchFilters).FirstOrDefault();
                if (existProject != null && existProject.Id != pid) throw new Exception("This Task Name already Exists");
                update.Add(Builders<ProjectModel>.Update.Set(x => x.Name, dto.Name));
            }
            await _project.UpdateOneAsync(filter, Builders<ProjectModel>.Update.Combine(update));
        }

        public async Task UpdateStatus(string uid, string pid)
        {
            var filter = Builders<ProjectModel>.Filter.And(
               Builders<ProjectModel>.Filter.Eq(x => x.Id, pid),
               Builders<ProjectModel>.Filter.Eq(x => x.UserId, uid)
               );
            var existProject = await _project.Find(filter).FirstOrDefaultAsync() ?? throw new Exception("project not found");
            var update = Builders<ProjectModel>.Update
                .Set(x => x.Status, !existProject?.Status);
            if (existProject!.Status)
            {
                await _project.UpdateOneAsync(filter, update);
            }
            else
            {
                var result = await _project.Aggregate()
                .Match(filter)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                    _task,
                    x => x.Id,
                    x => x.ProjectId,
                    x => x.Tasks
                )
               .Project(x => new ResponseProjectTaskDto
               {
                   Name = x.Name,
                   Desc = x.Desc,
                   Status = x.Status,
                   Id = x.Id!.ToString(),
                   Tasks = x.Tasks!.Select(t => new ResponseTaskData
                   {
                       Id = t.Id!.ToString(),
                       Name = t.Name,
                       Desc = t.Desc,
                       Status = t.Status,
                       Priority = t.Priority,
                       CreatedAt = t.CreatedAt
                   }).ToList(),
               }).FirstOrDefaultAsync();

                for (int i = 0; i < result.Tasks.Count(); i++)
                {
                    if (result.Tasks[i].Status != "Completed") throw new Exception("Please complete your Task");
                }

                await _project.UpdateOneAsync(filter, update);
            }
        }

        public async Task DeleteProject(string pId, string uId)
        {
            var filter = Builders<ProjectModel>.Filter.And(
               Builders<ProjectModel>.Filter.Eq(x => x.Id, pId), Builders<ProjectModel>.Filter.Eq(x => x.UserId, uId));
            await _project.DeleteOneAsync(filter);
        }

        public async Task<List<ProjectTaskStatusCount>> GetProjectTaskCount(string userId)
        {
            var res = await _project.Aggregate()
                .Match(x => x.UserId == userId)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                _task,
                x => x.Id,
                x => x.ProjectId,
                x => x.Tasks)
                .Unwind(x => x.Tasks)
                .As<projectTaskunwind>()
                .Group(
                x => new
                {
                    x.Id,
                    x.Name,
                    x.Tasks.Status
                },
                g => new
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Status = g.Key.Status,
                    Count = g.Count()
                })
                .Group(
                x => new
                {
                    x.Name,
                    x.Id,
                },
                g => new ProjectTaskStatusCount
                {
                    name = g.Key.Name,
                    count = g.Select(x => new ResponseStatusCount
                    {
                        Status = x.Status,
                        count = x.Count
                    }).ToList()
                })
    .ToListAsync();
            return res;
        }

        private List<ResponseTaskData> FilterAndPaginateTasks(List<ResponseTaskData> tasks, TaskQueryDto dto)
        {
            var query = tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                query = query.Where(x =>
                    x.Name!.Contains(dto.Search, StringComparison.OrdinalIgnoreCase));
            }

            if (dto.Month.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt!.Value.Month == dto.Month.Value);
            }

            if (dto.Year.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt!.Value.Year == dto.Year.Value);
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            var skip = (dto.Page - 1) * dto.PageSize;

            return query
                .Skip(skip)
                .Take(dto.PageSize)
                .Select(t => new ResponseTaskData
                {
                    Id = t.Id!.ToString(),
                    Name = t.Name,
                    Desc = t.Desc,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt
                })
                .ToList();
        }
    }
}
