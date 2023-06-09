﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAdminPortal.API.DataModels;
using StudentAdminPortal.API.DomainModels;

namespace StudentAdminPortal.API.Controllers
{
    [ApiController]
    public class StudentsController : Controller
    {
        private readonly IUnitOfWork uow;

        //private readonly IStudentRepository studentRepository;
        private readonly IMapper mapper;


        //public StudentsController(IStudentRepository studentRepository,
        //    IMapper mapper)
        //{
        //    this.studentRepository = studentRepository;
        //    this.mapper = mapper;
        //}

        public StudentsController(IUnitOfWork uow,
            IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("[controller]")]
        [Authorize]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await uow.StudentRepository.GetStudentsAsync();

           return Ok(mapper.Map<List<DomainModels.Student>>(students));
        }

        [HttpGet]
        [Route("[controller]/{studentId:guid}"),ActionName("GetStudentAsync")]
        public async Task<IActionResult> GetStudentAsync([FromRoute] Guid studentId)
        {
            //Fetch student details

            var student = await uow.StudentRepository.GetStudentAsync(studentId);

            if (student == null)
            { 
                return NotFound();
            }
            //Return student
            return Ok(mapper.Map<DomainModels.Student>(student));
        }

        [HttpPut]
        [Route("[controller]/{studentId:guid}")]
        public async Task<IActionResult> UpdateStudentAsync([FromRoute] Guid studentId, [FromBody] UpdateStudentRequet requet)
        {
            //Check user exist
           if(await uow.StudentRepository.Exist(studentId))
            {
                //Update student
                var updatedStudent = await uow.StudentRepository.UpdateStudentAsync(studentId, mapper.Map<DataModels.Student>(requet));

                if(updatedStudent != null)
                {
                    //Return student
                    return Ok(mapper.Map<DomainModels.Student>(updatedStudent));
                }
                
            }
                return NotFound();
            
        }

        [HttpDelete]
        [Route("[controller]/{studentId:guid}")]
        public async Task<IActionResult> deleteStudentAsync([FromRoute] Guid studentId)
        {
            //Check user exist
            if (await uow.StudentRepository.Exist(studentId))
            {
                //Delete student
                var deletedStudent = await uow.StudentRepository.DeleteStudentAsync(studentId);

                if (deletedStudent != null)
                {
                    //Return student
                    return Ok(mapper.Map<DomainModels.Student>(deletedStudent));
                }

            }
            //USer not found
            return NotFound();

        }

        [HttpPost]
        [Route("[controller]/Add")]
        public async Task<IActionResult> AddStudentAsync([FromBody] AddStudentRequest request)
        {
            //Add student
            var student = await uow.StudentRepository.AddStudentAsync(mapper.Map<DataModels.Student>(request));

            //Return student
            return CreatedAtAction(nameof(GetStudentAsync), new { studentId = student.Id }, mapper.Map<DomainModels.Student>(student));
            
        }

        [HttpPost]
        [Route("[controller]/{studentId:guid}/upload-image")]
        public async Task<IActionResult> UploadImage([FromRoute] Guid studentId, IFormFile profileImage)
        {
            var validExtensions = new List<string>
            {
                ".jpeg",
                ".png",
                ".gif",
                ".jpg"
            };

            if(profileImage != null && profileImage.Length > 0 )
            {
                var extension = Path.GetExtension(profileImage.FileName);
               if( validExtensions.Contains(extension) )
                {
                    //Check user exist
                    if (await uow.StudentRepository.Exist(studentId))
                    {
                        //Generate file name
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                        //Upload image to local storage
                        var fileImagePath = await uow.ImageRepository.Upload(profileImage, fileName);
                        //Update profil image path to the database
                        if (await uow.StudentRepository.UpdateProfileImage(studentId, fileImagePath))
                        {
                            //Return image path
                            return Ok(fileImagePath);
                        }
                        return StatusCode(StatusCodes.Status500InternalServerError, "Error while uploading image");


                    }
                }
                return BadRequest("this is not a valid image format");
            }
            


            return NotFound();
        }

    }
}
