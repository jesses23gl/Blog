using Api.Common.Util;
using Api.IServices;
using Api.Model;
using Api.Task;
using Api.Task.Quartz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITasksQzServices _tasksQzServices;
        private readonly ISchedulerCenter _schedulerCenter;

        public TasksController(ITasksQzServices tasksQzServices, ISchedulerCenter schedulerCenter)
        {
            _tasksQzServices = tasksQzServices;
            _schedulerCenter = schedulerCenter;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<string>> StartJob(int jobId) 
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzServices.QueryByID(jobId);
            if (model !=null)
            {
                model.IsStart = true;
                data.success = await _tasksQzServices.Update(model);
                data.response = jobId.ObjToString();
                if (data.success)
                {
                    data.msg = "更新成功";
                    var ResuleModel = await _schedulerCenter.AddScheduleJobAsync(model);
                    data.success = ResuleModel.success;
                    if (ResuleModel.success)
                    {
                        data.msg = $"{data.msg}=>启动成功=>{ResuleModel.msg}";

                    }
                    else
                    {
                        data.msg = $"{data.msg}=>启动失败=>{ResuleModel.msg}";
                    }
                }
                else
                {
                    data.msg = "更新失败";
                }
            }
            else
            {
                data.msg = "任务不存在";
            }
            return data;
        }
    }
}
