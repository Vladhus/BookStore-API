using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts with the BOOKS Table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        
        public BooksController(IBookRepository bookRepository, ILoggerService loggerService, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>A List Of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Attemped Get ");

                var books = await _bookRepository.FindAll();
                var responce = _mapper.Map<IList<BookDTO>>(books);

                _loggerService.LogInfo($"{location}: Successfully got all Books");

                return Ok(responce);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Get`s Book By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A Book Record</returns>
        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Attemped Get Book By Id {id}");

                var books = await _bookRepository.FindById(id);
                if (books == null)
                {
                    _loggerService.LogWarn($"{location}: Failed to retrieve record with id: {id}");
                    return NotFound();
                }
                var responce = _mapper.Map<IList<BookDTO>>(books);

                _loggerService.LogInfo($"{location}: Successfully Got Book With Id: {id}");

                return Ok(responce);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create New Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Book Object</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Create Attempted");
                if (bookDTO == null)
                {
                    _loggerService.LogWarn($"{location}: Empty Request Was Submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data Was Incomplete");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Creation Failed");
                }

                _loggerService.LogInfo($"{location} Book Created");
                return Created("Create", new {book});
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Put Updated Book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
                {
                    _loggerService.LogInfo($"{location}: Update Attempted on record with id: {id}");
                    if (id < 1 || bookDTO == null || id != bookDTO.Id)
                    {
                        _loggerService.LogWarn($"{location}: Update Failed With Bad Data - id: {id}");
                        return BadRequest();
                    }
                    var isExist = await _bookRepository.isExist(id);
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
                    var book = _mapper.Map<Book>(bookDTO);
                    var isSuccess = await _bookRepository.Update(book);
                    if (!isSuccess)
                    {
                        return InternalError($"{location}: Update operation failed for record with id: {id}");
                    }
                    _loggerService.LogInfo($"{location}: Record with id: {id} seccessfully updated");
                    return NoContent();
                }
                catch (Exception ex)
                {

                    return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
                }
        }


        /// <summary>
        /// Removes an book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location}: Author with id: {id} Delete Attempted");
                if (id < 1)
                {
                    _loggerService.LogWarn($"{location}: Author Delete Failed with bad data");
                    return BadRequest();
                }
                var isExist = await _bookRepository.isExist(id);
                if (!isExist)
                {
                    _loggerService.LogWarn($"{location}: Author with id: {id} was not found");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Author Delete Failed");
                }
                _loggerService.LogInfo($"{location}: Author with id: {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
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
