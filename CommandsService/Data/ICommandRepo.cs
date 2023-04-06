using System.Collections.Generic;
using CommandsService.Models;

namespace CommandsService.Data
{
    public interface ICommandRepo
    {
        bool SaveChanges();

        //platforms
        IEnumerable<Platform> GetAllPlatForms();
        void CreatePlatform(Platform platform);
        bool PlatformExist(int platformId);

        //commands
        IEnumerable<Command> GetCommandsForPlatform(int platformId);
        Command GetCommand(int platformId, int commandId);
        void CreateCommand(int platformId, Command command);

        bool ExternalPlatformExist(int externalPlatformId);
    }
}