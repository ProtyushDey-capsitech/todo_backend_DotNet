using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Project;
using Projects.Dtos.Todo;
using Projects.Models;
using System.Runtime.CompilerServices;

namespace Projects.Services
{
    public class ProjectService
    {
        private readonly IMongoCollection<ProjectModel> _project;
        private readonly IMongoCollection<TaskModel> _taskModel;

        public ProjectService(IOptions<DbSettings> dbsettings)
        {
            var mongoclient = new MongoClient(dbsettings.Value.ConnectionString);
            var dataBase = mongoclient.GetDatabase(dbsettings.Value.DatabaseName);
            _project = dataBase.GetCollection<ProjectModel>(DbCollections.Project);
            _taskModel = dataBase.GetCollection<TaskModel>(DbCollections.Tasks);
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

        public async Task<List<ResponseProjectData>> GetAllProject(string uerId)
        {
            var filter = Builders<ProjectModel>.Filter.Eq(x => x.UserId, uerId);
            var projects = await _project.Find(filter)
                .Project(x => new ResponseProjectData
                {
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Id = x.Id,
                })
                .ToListAsync();
            return projects;

        }

        public async Task<ResponseProjectTaskDto> GetProject(string uerId, string PId)
        {
            var projectFilter = Builders<ProjectModel>.Filter.And(Builders<ProjectModel>.Filter.Eq(x => x.UserId, uerId),
                Builders<ProjectModel>.Filter.Eq(x => x.Id, PId));
            var existProject = await _project.Find(projectFilter).FirstOrDefaultAsync();
            if (existProject == null) throw new Exception("Project not fount");
            var project =await _project.Aggregate()
                .Match(projectFilter)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                    _taskModel,
                    x => x.Id,
                    x => x.ProjectId,
                    x => x.Tasks

                )
                .Project(x => new ResponseProjectTaskDto
                {
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Id = x.Id.ToString(),
                    Tasks = x.Tasks!.Select(t => new ResponseTaskData
                    {
                        Id = t.Id.ToString(),
                        Name = t.Name,
                        Desc = t.Desc,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedAt = t.CreatedAt
                    }).ToList(),
                }).FirstOrDefaultAsync();

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
            var existProject = await _project.Find(filter).FirstOrDefaultAsync();
            if (existProject == null) 
                throw new Exception("project not found");
            var update = Builders<ProjectModel>.Update
                .Set(x => x.Status, !existProject?.Status);
            if (existProject.Status)
            {
                await _project.UpdateOneAsync(filter, update);
            }
            else
            {
                var Tasks = await _project.Aggregate()
                .Match(filter)
                .Lookup<ProjectModel, TaskModel, ResponseProjectTaskDto>(
                    _taskModel,
                    x => x.Id,
                    x => x.ProjectId,
                    x => x.Tasks
                )
               .Match(x => x.Tasks.Any(t => t.Status != "Completed")).ToListAsync();
                if (Tasks.Count> 0) throw new Exception("Complete all tasks");
                await _project.UpdateOneAsync(filter, update);
            }
        }

        public async Task DeleteProject(string pId, string uId)
        {
            var filter = Builders<ProjectModel>.Filter.And(
               Builders<ProjectModel>.Filter.Eq(x => x.Id, pId), Builders<ProjectModel>.Filter.Eq(x => x.UserId, uId));
            await _project.DeleteOneAsync(filter);
        }
    }
}
