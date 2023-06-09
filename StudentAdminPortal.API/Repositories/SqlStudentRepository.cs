﻿using Microsoft.EntityFrameworkCore;
using StudentAdminPortal.API.DataModels;

namespace StudentAdminPortal.API.Repositories
{
    public class SqlStudentRepository : IStudentRepository
    {
        private readonly StudentAdminContext context;
        public SqlStudentRepository(StudentAdminContext context)
        {
            this.context = context;
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            return await context.Student.Include(nameof(Gender))
                .Include(nameof(Address)).ToListAsync();
        }


        public async Task<Student> GetStudentAsync(Guid studentId)
        {
            return await context.Student
                .Include(nameof(Gender)).Include(nameof(Address))
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        

        public async Task<List<Gender>> GetAllGenders()
        {
            return await context.Gender.ToListAsync();
        }

        public async Task<bool> Exist(Guid studentId)
        {
            return await context.Student.AnyAsync(s => s.Id == studentId);
        }

        public async Task<Student> UpdateStudentAsync(Guid studentId, Student request)
        {
            var existingStudent = await GetStudentAsync(studentId);

            if (existingStudent != null)
            {
               existingStudent.FirstName = request.FirstName;
                existingStudent.LastName = request.LastName;
                existingStudent.DateofBirth = request.DateofBirth;
                existingStudent.Email = request.Email;
                existingStudent.Mobile = request.Mobile;
                existingStudent.GenderId = request.GenderId;
                existingStudent.Address.PhysicalAddress = request.Address.PhysicalAddress;
                existingStudent.Address.PostalAddress = request.Address.PostalAddress;

                await context.SaveChangesAsync();

                return existingStudent;

            }   

            return null;
        }
    }
}
