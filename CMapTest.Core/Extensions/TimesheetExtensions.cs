using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Entities;

namespace CMapTest.Core.Extensions;

public static class TimesheetExtensions
{
    extension(TimesheetEntity entity)
    {
        public TimesheetResponse ToResponse()
        {
            return new TimesheetResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ProjectId = entity.ProjectId,
                Date = entity.Date,
                Description = entity.Description,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
            };
        }
    }
}