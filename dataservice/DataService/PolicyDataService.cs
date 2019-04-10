using DataService.Models;
using DataService.Rpc;
using Grpc.Core;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataService.Rpc.PolicyData;

namespace DataService
{
    class PolicyDataService : PolicyDataBase
    {
        private IMongoDatabase _database;
        public PolicyDataService() {
            // build mongo client, get the policy database
            _database = new MongoClient("mongodb://mongo:27017").GetDatabase("policy");

            Populate();
        }

        // Get all permissions from all roles assigned to the user of an app.
        public override Task<PermissionsResponse> GetPermissions(UserRequest request, ServerCallContext context)
        {
            var user = _database.GetCollection<User>("Users").AsQueryable().Where(x => x.QualifiedName == request.Id).FirstOrDefault();
            if (user == null) 
            {
                return Task.FromResult(new PermissionsResponse());
            }

            var permissions = _database.GetCollection<Role>("Roles").AsQueryable().Where(x => user.Roles.Contains(x.Name)).ToList().SelectMany(x => x.Permissions);
            var response = new PermissionsResponse();
            response.Permissions.AddRange(permissions);

            return Task.FromResult(response);
        }

        // Get all roles assigned to the user of an app.
        public override Task<RolesResponse> GetRoles(UserRequest request, ServerCallContext context)
        {
            var user = _database.GetCollection<User>("Users").AsQueryable().Where(x => x.QualifiedName == request.Id).FirstOrDefault();
            if (user == null) 
            {
                return Task.FromResult(new RolesResponse());
            }

            var permissions = _database.GetCollection<Role>("Roles").AsQueryable().Where(x => user.Roles.Contains(x.Name)).Select(x => x.Name);
            var response = new RolesResponse();
            response.Roles.AddRange(permissions);

            return Task.FromResult(response);
        }

        // Check if a user has a specific permission.
        public override Task<HasPermissionsResponse> HasPermissions(HasPermissionsRequest request, ServerCallContext context)
        {
            var requestedPermissions = request.Permissions.ToList();
            var user = _database.GetCollection<User>("Users").AsQueryable().Where(x => x.QualifiedName == request.Id).FirstOrDefault();
            if (user == null) {
                return Task.FromResult(new HasPermissionsResponse { Result = false });
            }

            var permissions = _database.GetCollection<Role>("Roles").AsQueryable().Where(x => user.Roles.Contains(x.Name)).ToList().SelectMany(x => x.Permissions);
            var result = requestedPermissions.Union(permissions).Count() == requestedPermissions.Count;

            return Task.FromResult(new HasPermissionsResponse {
                Result = result
            });
        }

        // Check if a user has a specific role.
        public override Task<HasRolesResponse> HasRoles(HasRolesRequest request, ServerCallContext context)
        {
            var requestedRoles = request.Roles.ToList();
            var user = _database.GetCollection<User>("Users").AsQueryable().Where(x => x.QualifiedName == request.Id).FirstOrDefault();
            if (user == null) {
                return Task.FromResult(new HasRolesResponse { Result = false });
            }

            var roles = _database.GetCollection<Role>("Roles").AsQueryable().Where(x => user.Roles.Contains(x.Name)).ToList().Select(x => x.Name);
            var result = requestedRoles.Union(roles).Count() == requestedRoles.Count;

            return Task.FromResult(new HasRolesResponse {
                Result = result
            });
        }

        // This method populates some policy data that we can query for in the client applicaiton.
        private void Populate(){
            try {
            _database.GetCollection<User>("Users").InsertOne(new User {
                QualifiedName = "app/identifier/client",
                Roles = new List<string> {
                    "admin"
                }
            });
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            try {
                _database.GetCollection<Role>("Roles").InsertOne(new Role {
                Name = "admin",
                Permissions = new List<string> {"viewSalary"}
            });
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
