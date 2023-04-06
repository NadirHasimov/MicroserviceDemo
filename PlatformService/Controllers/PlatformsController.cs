using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PLatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--->Getting platforms");
            var dtos = _mapper.Map<IEnumerable<PLatformReadDto>>(_repository.GetAllPlatforms());
            return Ok(dtos);
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PLatformReadDto> GetPlatformById(int id)
        {
            Console.WriteLine("--->Get platform by id");
            var dto = _mapper.Map<PLatformReadDto>(_repository.GetPlatformById(id));
            if (dto != null)
                return Ok(dto);
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PLatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            Console.WriteLine("--->Creating platform");
            var entity = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(entity);
            _repository.SaveChanges();

            var dto = _mapper.Map<PLatformReadDto>(entity);

            //Send sync message
            try
            {
                await _commandDataClient.SendPlatformToCommand(dto);
            }
            catch (System.Exception exc)
            {
                Console.WriteLine($"Could not send syncronously: {exc.Message}");
                throw;
            }

            //Send async message
            try
            {
                var platformPublishedDto = _mapper.Map<PLatformPublishedDto>(dto);
                platformPublishedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (System.Exception exc)
            {
                Console.WriteLine($"Could not send asyncronously: {exc.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { dto.Id }, dto);
        }
    }
}