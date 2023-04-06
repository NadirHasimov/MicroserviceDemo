using System;
using System.Collections.Generic;
using CommandsService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                SyncDataServices.Grpc.IPlatformDataClient grpcClient = serviceScope.ServiceProvider.GetService<SyncDataServices.Grpc.IPlatformDataClient>();

                var platforms = grpcClient.ReturnPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--->Seeding new platfomrs...");

            foreach (var plat in platforms)
            {
                if (!repo.ExternalPlatformExist(plat.ExternalId))
                {
                    repo.CreatePlatform(plat);
                }
                repo.SaveChanges();
            }
        }
    }
}