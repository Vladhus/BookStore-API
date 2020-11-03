using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store`s database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository, ILoggerService loggerService, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }


        /// <summary>
        /// Get All Author`s
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Attemped Get ");

                var authors = await _authorRepository.FindAll();
                var responce = _mapper.Map<IList<AuthorDTO>>(authors);

                _loggerService.LogInfo($"{location}: Successfully got all Authors");

                return Ok(responce);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Get An Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Author`s record</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Attemped Get Book By Id {id}");

                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _loggerService.LogWarn($"{location}: Failed to retrieve record with id: {id}");
                    return NotFound();
                }
                var responce = _mapper.Map<AuthorDTO>(author);

                _loggerService.LogInfo($"{location}: Successfully Got Author With Id: {id}");
                return Ok(responce);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Create an Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Create Attempted");
                if (authorDTO == null )
                {
                    _loggerService.LogWarn($"{location}: Empty Request Was Submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data Was Incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Creation failed");
                }

                _loggerService.LogInfo($"{location} Author Created");
                return Created("Create", new {author});
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Put Update Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id,[FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Update Attempted on record with id: {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _loggerService.LogWarn($"{location}: Update Failed With Bad Data - id: {id}");
                    return BadRequest();
                }
                var isExist = await _authorRepository.isExist(id);
                if (!isExist)
                {
                    _loggerService.LogWarn($"{location}: Failed to retriew record with id: {id}");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update operation failed for record with id: {id}");
                }
                _loggerService.LogInfo($"{location}: Record with id: {id} seccessfully updated");
                return NoContent();
            }
            catch (Exception ex)
            {

                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} Author with id: {id} Delete Attempted");
                if (id < 1)
                {
                    _loggerService.LogWarn($"{location} Author Delete Failed with bad data");
                    return BadRequest();
                }
                var isExist = await _authorRepository.isExist(id);
                if (!isExist)
                {
                    _loggerService.LogWarn($"{location} Author with id: {id} was not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location} Author Delete Failed");
                }
                _loggerService.LogInfo($"{location} Author with id: {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location} {ex.Message} - {ex.InnerException}"); ;
            }
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong.Please contact the Administrator");
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
    }
}
