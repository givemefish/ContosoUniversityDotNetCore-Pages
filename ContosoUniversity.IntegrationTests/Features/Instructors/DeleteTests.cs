﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Models;
using ContosoUniversity.Pages.Instructors;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Features.Instructors
{
    using static SliceFixture;

    public class DeleteTests : IntegrationTestBase
    {
        [Fact]
        public async Task Should_query_for_command()
        {
            var englishDept = new Department
            {
                Name = "English",
                StartDate = DateTime.Today
            };
            var english101 = new Course
            {
                Department = englishDept,
                Title = "English 101",
                Credits = 4,
                Id = NextCourseNumber()
            };
            var command = new CreateEdit.Command
            {
                FirstMidName = "George",
                LastName = "Costanza",
                OfficeAssignmentLocation = "Austin",
                HireDate = DateTime.Today,
                SelectedCourses = new []{ english101.Id.ToString()}
            };
            var instructorId = await SendAsync(command);

            await InsertAsync(englishDept, english101);

            var result = await SendAsync(new Delete.Query { Id = instructorId });

            result.ShouldNotBeNull();
            result.FirstMidName.ShouldBe(command.FirstMidName);
            result.OfficeAssignmentLocation.ShouldBe(command.OfficeAssignmentLocation);
        }

        [Fact]
        public async Task Should_delete_instructor()
        {
            var instructorId = await SendAsync(new CreateEdit.Command
            {
                FirstMidName = "George",
                LastName = "Costanza",
                OfficeAssignmentLocation = "Austin",
                HireDate = DateTime.Today
            });
            var englishDept = new Department
            {
                Name = "English",
                StartDate = DateTime.Today,
                InstructorId = instructorId
            };
            var english101 = new Course
            {
                Department = englishDept,
                Title = "English 101",
                Credits = 4,
                Id = NextCourseNumber()
            };

            await InsertAsync(englishDept, english101);

            await SendAsync(new CreateEdit.Command
            {
                Id = instructorId,
                FirstMidName = "George",
                LastName = "Costanza",
                OfficeAssignmentLocation = "Austin",
                HireDate = DateTime.Today,
                SelectedCourses = new[] { english101.Id.ToString() }
            });

            await SendAsync(new Delete.Command { Id = instructorId });

            var instructorCount = await ExecuteDbContextAsync(db => db.Instructors.Where(i => i.Id == instructorId).CountAsync());

            instructorCount.ShouldBe(0);

            var englishDeptId = englishDept.Id;
            englishDept = await ExecuteDbContextAsync(db => db.Departments.FindAsync(englishDeptId));

            englishDept.InstructorId.ShouldBeNull();

            var courseInstructorCount = await ExecuteDbContextAsync(db => db.CourseAssignments.Where(ci => ci.InstructorId == instructorId).CountAsync());

            courseInstructorCount.ShouldBe(0);
        }

    }
}