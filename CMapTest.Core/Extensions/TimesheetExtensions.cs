using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Entities;
using Humanizer;

namespace CMapTest.Core.Extensions;

public static class TimesheetExtensions
{
    extension(TimesheetEntity entity)
    {
        public TimesheetResponse ToResponse()
        {
            var duration = entity.EndTime - entity.StartTime;
            return new TimesheetResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ProjectId = entity.ProjectId,
                Date = entity.Date,
                Description = entity.Description,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                HoursWorked = (decimal)duration.TotalHours,
                Duration = duration.Humanize()
            };
        }
    }
}